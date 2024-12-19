using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class DomainBoxManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private XRDirectInteractor rightHandInteractor;
    [SerializeField] private Transform stylusTip;
    [SerializeField] private GameObject vertexPrefab;
    [SerializeField] private Material lineMaterial;
    [SerializeField] private Material domainMaterial;

    [Header("Input Actions")]
    [SerializeField] private InputActionProperty triggerAction;
    [SerializeField] private InputActionProperty gripAction;
    [SerializeField] private InputActionProperty buttonAAction; // For UI toggle

    [Header("Settings")]
    [SerializeField] private float vertexSnapDistance = 0.05f;
    [SerializeField] private float extrusionSensitivity = 1f;
    [SerializeField] private float lineWidth = 0.005f;
    [SerializeField] private float edgeDetectionRadius = 0.1f; // Radius to detect edges for extrusion
    [SerializeField] private LayerMask edgeLayer; // Layer for edge detection

    private List<GameObject> vertices = new List<GameObject>();
    private List<LineRenderer> edges = new List<LineRenderer>();
    private GameObject domainBox;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private bool isCreatingBase = true;
    private bool isExtruding = false;
    private Vector3 extrusionStartPoint;
    private float currentHeight = 0f;
    private float baseHeight; // Height at which the base plane is created
    private Vector3 selectedEdgeStart;
    private Vector3 selectedEdgeEnd;
    private bool isNearEdge = false;

    private void OnEnable()
    {
        EnableActions();
    }

    private void OnDisable()
    {
        DisableActions();
    }

    private void EnableActions()
    {
        triggerAction.action.Enable();
        gripAction.action.Enable();
        buttonAAction.action.Enable();

        triggerAction.action.performed += OnTriggerPerformed;
        triggerAction.action.canceled += OnTriggerCanceled;
    }

    private void DisableActions()
    {
        triggerAction.action.Disable();
        gripAction.action.Disable();
        buttonAAction.action.Disable();

        triggerAction.action.performed -= OnTriggerPerformed;
        triggerAction.action.canceled -= OnTriggerCanceled;
    }

    private void Start()
    {
        InitializeDomainBox();
    }

    private void InitializeDomainBox()
    {
        domainBox = new GameObject("DomainBox");
        domainBox.transform.parent = transform;
        meshFilter = domainBox.AddComponent<MeshFilter>();
        meshRenderer = domainBox.AddComponent<MeshRenderer>();
        meshRenderer.material = domainMaterial;
        meshRenderer.material.color = new Color(0.5f, 0.5f, 1f, 0.3f);
        domainBox.SetActive(false);
    }

    private void OnTriggerPerformed(InputAction.CallbackContext context)
    {
        if (!gripAction.action.IsPressed()) return;

        if (isCreatingBase && vertices.Count < 4)
        {
            PlaceVertex();
        }
        else if (!isCreatingBase)
        {
            StartExtrusion();
        }
    }

    private void OnTriggerCanceled(InputAction.CallbackContext context)
    {
        if (isExtruding)
        {
            isExtruding = false;
        }
    }

    private void PlaceVertex()
    {
        Vector3 position = stylusTip.position;

        // If this is the first vertex, set the base height
        if (vertices.Count == 0)
        {
            baseHeight = position.y;
        }

        // Snap to base height
        position.y = baseHeight;

        // Snap to existing vertices horizontally
        foreach (var existingVertex in vertices)
        {
            Vector3 existingPos = existingVertex.transform.position;
            float xzDistance = Vector2.Distance(
                new Vector2(existingPos.x, existingPos.z),
                new Vector2(position.x, position.z)
            );

            if (xzDistance < vertexSnapDistance)
            {
                // Snap to existing X or Z based on which is closer
                if (Mathf.Abs(existingPos.x - position.x) < Mathf.Abs(existingPos.z - position.z))
                {
                    position.x = existingPos.x;
                }
                else
                {
                    position.z = existingPos.z;
                }
                break;
            }
        }

        GameObject vertex = Instantiate(vertexPrefab, position, Quaternion.identity, transform);
        vertices.Add(vertex);

        if (vertices.Count > 1)
        {
            CreateEdge(vertices[vertices.Count - 2].transform.position, position);
        }

        if (vertices.Count == 4)
        {
            CompleteBase();
        }
    }

    private void CreateEdge(Vector3 start, Vector3 end)
    {
        GameObject edgeObj = new GameObject($"Edge_{edges.Count}");
        edgeObj.transform.parent = transform;
        LineRenderer line = edgeObj.AddComponent<LineRenderer>();
        
        line.material = lineMaterial;
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
        line.positionCount = 2;
        line.SetPosition(0, start);
        line.SetPosition(1, end);
        
        edges.Add(line);
    }

    private void CompleteBase()
    {
        // Create closing edge
        CreateEdge(vertices[3].transform.position, vertices[0].transform.position);
        isCreatingBase = false;
        domainBox.SetActive(true);
        UpdateDomainMesh();
    }


    private void Update()
    {
        if (!gripAction.action.IsPressed()) return;

        if (isCreatingBase)
        {
            // Show preview of horizontal snap while moving stylus
            // You can add visual feedback here
        }
        else if (!isExtruding)
        {
            // Check if stylus is near any edge
            CheckEdgeProximity();
        }
        else if (isExtruding)
        {
            UpdateExtrusion();
        }
    }

    private void CheckEdgeProximity()
    {
        isNearEdge = false;
        Vector3 tipPosition = stylusTip.position;

        for (int i = 0; i < edges.Count; i++)
        {
            Vector3 start = edges[i].GetPosition(0);
            Vector3 end = edges[i].GetPosition(1);
            Vector3 closestPoint = GetClosestPointOnLine(start, end, tipPosition);
            
            if (Vector3.Distance(tipPosition, closestPoint) < edgeDetectionRadius)
            {
                isNearEdge = true;
                selectedEdgeStart = start;
                selectedEdgeEnd = end;
                
                // Visual feedback that edge can be extruded
                edges[i].startWidth = lineWidth * 2;
                edges[i].endWidth = lineWidth * 2;
            }
            else
            {
                edges[i].startWidth = lineWidth;
                edges[i].endWidth = lineWidth;
            }
        }
    }


    public void Reset()
    {
        foreach (var vertex in vertices)
        {
            Destroy(vertex);
        }
        foreach (var edge in edges)
        {
            Destroy(edge.gameObject);
        }
        vertices.Clear();
        edges.Clear();
        isCreatingBase = true;
        isExtruding = false;
        currentHeight = 0f;
        domainBox.SetActive(false);
    }

        private void StartExtrusion()
    {
        if (!isNearEdge) return;
        
        isExtruding = true;
        extrusionStartPoint = stylusTip.position;
        currentHeight = 0f;
        
        // Make sure domain box is visible
        domainBox.SetActive(true);
    }

    private void UpdateExtrusion()
    {
        float heightDelta = stylusTip.position.y - extrusionStartPoint.y;
        currentHeight = Mathf.Max(0, heightDelta);
        UpdateDomainMesh();
    }

    private void UpdateDomainMesh()
    {
        if (vertices.Count != 4) return;

        Vector3[] meshVertices = new Vector3[8];
        
        // Get vertices in correct order for the box
        for (int i = 0; i < 4; i++)
        {
            meshVertices[i] = vertices[i].transform.position - domainBox.transform.position;
        }

        // Create top vertices
        for (int i = 0; i < 4; i++)
        {
            meshVertices[i + 4] = meshVertices[i] + Vector3.up * currentHeight;
        }

        int[] triangles = new int[]
        {
            // Bottom face
            0, 2, 1,
            0, 3, 2,
            // Top face
            4, 5, 6,
            4, 6, 7,
            // Side faces
            0, 1, 5,
            0, 5, 4,
            1, 2, 6,
            1, 6, 5,
            2, 3, 7,
            2, 7, 6,
            3, 0, 4,
            3, 4, 7
        };

        Mesh mesh = new Mesh();
        mesh.vertices = meshVertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
        meshRenderer.material.SetColor("_Color", new Color(0.5f, 0.5f, 1f, 0.3f));
    }

    private Vector3 GetClosestPointOnLine(Vector3 start, Vector3 end, Vector3 point)
    {
        Vector3 line = end - start;
        float len = line.magnitude;
        line.Normalize();

        Vector3 v = point - start;
        float d = Vector3.Dot(v, line);
        d = Mathf.Clamp(d, 0f, len);

        return start + line * d;
    }
}