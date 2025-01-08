using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class DomainBoxCreator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private XRDirectInteractor rightHandInteractor;
    [SerializeField] private Transform stylusTip;
    [SerializeField] private Material lineMaterial;
    [SerializeField] private GameObject vertexPrefab;
    [SerializeField] private Material boxMaterial;
    [SerializeField] private GameObject resetObject;

    [Header("Input Actions")]
    [SerializeField] private InputActionProperty triggerAction;
    [SerializeField] private InputActionProperty buttonBAction;

    [Header("Settings")]
    [SerializeField] private float edgeProximityDistance = 0.05f;
    [SerializeField] private const float baseEdgeWidth = 0.01f;
    [SerializeField] private float edgeHighlightScale = 1.5f;
    

    private List<Vector3> vertices = new List<Vector3>();
    private List<GameObject> vertexObjects = new List<GameObject>();
    private List<LineRenderer> edges = new List<LineRenderer>();
    private List<GameObject> topVertexObjects = new List<GameObject>();
    private GameObject boxObject;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private bool isCreatingBase = true;
    private bool isExtruding = false;
    private int selectedEdgeIndex = -1;
    private float boxHeight = 0f;
    private Vector3 extrudeStartPosition;
    private bool isDomainEditingEnabled = true;
    private float baseHeight = 0f;
    

    private void OnEnable()
    {
        triggerAction.action.Enable();
        buttonBAction.action.Enable();
        buttonBAction.action.performed += OnButtonBPressed;
    }

    private void OnDisable()
    {
        triggerAction.action.Disable();
        buttonBAction.action.Disable();
        buttonBAction.action.performed -= OnButtonBPressed;
    }

    private void Start()
    {
        InitializeBox();
        enabled = false;  // Start disabled by default

    }

    private void InitializeBox()
    {
        boxObject = new GameObject("DomainBox");
        boxObject.transform.parent = transform;
        meshFilter = boxObject.AddComponent<MeshFilter>();
        meshRenderer = boxObject.AddComponent<MeshRenderer>();
        meshRenderer.material = boxMaterial;
        meshRenderer.material.color = new Color(0.5f, 0.5f, 1f, 0.3f);
    }

    private void Update()
    {
        if (!isDomainEditingEnabled) return;
        
        CheckResetObjectProximity();  // replace with stylus UI buttons
        Vector3 tipPosition = stylusTip.position;

        if (isCreatingBase)
        {
            if (triggerAction.action.WasPressedThisFrame() && vertices.Count < 4)
            {
                if (vertices.Count == 0)
                {
                    // First vertex - use exact stylus position
                    PlaceVertex(tipPosition);
                }
                else
                {
                    // Subsequent vertices - keep them at same height as first vertex
                    Vector3 snappedPosition = tipPosition;
                    snappedPosition.y = vertices[0].y;
                    PlaceVertex(snappedPosition);
                }
            }
        }
        else
        {
            CheckEdgeProximity(tipPosition);
            HandleExtrusion();
        }
    }

    private void PlaceVertex(Vector3 position)
    {
            // If this is the first vertex, set the base height
        if (vertices.Count == 0)
        {
            baseHeight = position.y;
        }
        else
        {
            // For subsequent vertices, use the base height
            position.y = baseHeight;
        }

        GameObject vertexObj = Instantiate(vertexPrefab, position, Quaternion.identity, transform);
            // Vertex Size
        vertexObj.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
        vertexObjects.Add(vertexObj);
        vertices.Add(position);

        if (vertices.Count > 1)
        {
            CreateEdge(vertices.Count - 2, vertices.Count - 1);
        }

        if (vertices.Count == 4)
        {
            CreateEdge(3, 0); // Close the base
            isCreatingBase = false;
            CreateTopVertices();
            UpdateBoxMesh();
        }
    }

    private void CreateTopVertices()
    {
        // Create vertices
        for (int i = 0; i < 4; i++)
        {
            Vector3 topPosition = vertices[i] + Vector3.up * boxHeight;
            GameObject vertexObj = Instantiate(vertexPrefab, topPosition, Quaternion.identity, transform);
            vertexObj.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
            topVertexObjects.Add(vertexObj);
        }

        // Create edges between top vertices
        for (int i = 0; i < 4; i++)
        {
            GameObject edgeObj = new GameObject($"TopEdge_{i}");
            edgeObj.transform.parent = transform;
            LineRenderer line = edgeObj.AddComponent<LineRenderer>();
            line.material = lineMaterial;
            line.startWidth = line.endWidth = baseEdgeWidth;
            line.positionCount = 2;

            // Connect to next vertex (loop back to first for last edge)
            Vector3 startPos = vertices[i] + Vector3.up * boxHeight;
            Vector3 endPos = vertices[(i + 1) % 4] + Vector3.up * boxHeight;
            
            line.SetPosition(0, startPos);
            line.SetPosition(1, endPos);
            edges.Add(line);
        }
    }

    private void UpdateTopVertices()
    {
        // Update top vertex positions
        for (int i = 0; i < 4; i++)
        {
            topVertexObjects[i].transform.position = vertices[i] + Vector3.up * boxHeight;
        }

        // Update top edges
        // Base edges are 0-3, top edges are 4-7
        for (int i = 0; i < 4; i++)
        {
            LineRenderer line = edges[i + 4]; // Top edges start after base edges
            line.SetPosition(0, vertices[i] + Vector3.up * boxHeight);
            line.SetPosition(1, vertices[(i + 1) % 4] + Vector3.up * boxHeight);
        }
    }

    private void CreateEdge(int startIndex, int endIndex)
    {
        GameObject edgeObj = new GameObject($"Edge_{edges.Count}");
        edgeObj.transform.parent = transform;
        LineRenderer line = edgeObj.AddComponent<LineRenderer>();
        line.material = lineMaterial;
        line.startWidth = line.endWidth = baseEdgeWidth;
        line.positionCount = 2;
        line.SetPosition(0, vertices[startIndex]);
        line.SetPosition(1, vertices[endIndex]);
        edges.Add(line);
    }

    private void CheckEdgeProximity(Vector3 position)
    {
        selectedEdgeIndex = -1;
        for (int i = 0; i < edges.Count; i++)
        {
            Vector3 start = edges[i].GetPosition(0);
            Vector3 end = edges[i].GetPosition(1);
            Vector3 closest = GetClosestPointOnLine(start, end, position);

            if (Vector3.Distance(position, closest) < edgeProximityDistance)
            {
                selectedEdgeIndex = i;
                edges[i].startWidth = edges[i].endWidth = baseEdgeWidth * edgeHighlightScale;
                edges[i].material.color = Color.yellow;

                if (triggerAction.action.WasPressedThisFrame() && !isExtruding)
                {
                    StartExtrusion(position);
                }
            }
            else
            {
                edges[i].startWidth = edges[i].endWidth = baseEdgeWidth;
                edges[i].material.color = Color.white; // Reset color
            }
        }
    }

    private void StartExtrusion(Vector3 position)
    {
        isExtruding = true;
        extrudeStartPosition = position;
    }

    private void HandleExtrusion()
    {
        if (isExtruding)
        {
            if (triggerAction.action.IsPressed())
            {
                float heightDelta = stylusTip.position.y - extrudeStartPosition.y;
                boxHeight = Mathf.Max(0.01f, heightDelta);
                UpdateBoxMesh();
                UpdateTopVertices();
            }
            else
            {
                isExtruding = false;
            }
        }
    }

    private void UpdateBoxMesh()
    {
        if (vertices.Count != 4) return;

        Vector3[] meshVertices = new Vector3[8];
        int[] triangles = new int[36];

        // Base vertices
        for (int i = 0; i < 4; i++)
        {
            meshVertices[i] = vertices[i];
            meshVertices[i + 4] = vertices[i] + Vector3.up * boxHeight;
        }

        // Triangle indices for all faces
        triangles = new int[]
        {
            // Bottom face
            0, 2, 1, 0, 3, 2,
            // Top face
            4, 5, 6, 4, 6, 7,
            // Side faces
            0, 1, 5, 0, 5, 4,
            1, 2, 6, 1, 6, 5,
            2, 3, 7, 2, 7, 6,
            3, 0, 4, 3, 4, 7
        };

        Mesh mesh = new Mesh();
        mesh.vertices = meshVertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
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

    private void OnButtonBPressed(InputAction.CallbackContext context)
    {
        isDomainEditingEnabled = false;
        Debug.Log("Domain editing stopped - Starting scan mode");
    }

    public void ResetDomain()
    {
        foreach (var vertex in vertexObjects)
        {
            Destroy(vertex);
        }
        foreach (var vertex in topVertexObjects) // Add this
        {
            Destroy(vertex);
        }
        foreach (var edge in edges)
        {
            Destroy(edge.gameObject);
        }
        vertexObjects.Clear();
        topVertexObjects.Clear(); // Add this
        vertices.Clear();
        edges.Clear();
        boxHeight = 0f;
        baseHeight = 0f; // Reset base height
        isCreatingBase = true;
        isExtruding = false;
        isDomainEditingEnabled = true;
        if (meshFilter.mesh != null)
        {
            meshFilter.mesh.Clear();
        }
    }

    private void CheckResetObjectProximity()
    {
        if (resetObject != null)
        {
            float distance = Vector3.Distance(stylusTip.position, resetObject.transform.position);
            if (distance < edgeProximityDistance)
            {
                ResetDomain();
            }
        }
    }
}