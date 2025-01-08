using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class AnnotationManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject anchorPrefab;
    [SerializeField] private GameObject calloutPrefab;
    [SerializeField] private Transform stylusTip;
    [SerializeField] private InputActionProperty triggerAction;
    [SerializeField] private Canvas worldSpaceCanvas;
    

    [Header("Settings")]
    [SerializeField] private float previewAlpha = 0.5f;
    [SerializeField] private Color lineColor = Color.white;
    [SerializeField] private float calloutOffset = 0.2f;

    private GameObject currentAnchorPreview;
    private bool isAnnotationMode;

    private void OnEnable()
    {
        triggerAction.action.Enable();
        triggerAction.action.performed += OnTriggerPressed;
        Debug.Log("AnnotationManager enabled");
    }

    private void OnDisable()
    {
        triggerAction.action.Disable();
        triggerAction.action.performed -= OnTriggerPressed;
    }

    public void StartAnnotationMode()
    {
        isAnnotationMode = true;
        CreateAnchorPreview();
    }

    public void StopAnnotationMode()
    {
        isAnnotationMode = false;
        if (currentAnchorPreview)
        {
            Destroy(currentAnchorPreview);
        }
    }

    private void Update()
    {
        if (isAnnotationMode && currentAnchorPreview)
        {
            currentAnchorPreview.transform.position = stylusTip.position;
        }
    }

    private void CreateAnchorPreview()
    {
        currentAnchorPreview = Instantiate(anchorPrefab, stylusTip.position, Quaternion.identity);
        
        // Make preview semi-transparent
        var renderers = currentAnchorPreview.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            Material material = renderer.material;
            Color color = material.color;
            color.a = previewAlpha;
            material.color = color;
        }
    }

    private void OnTriggerPressed(InputAction.CallbackContext context)
    {
        Debug.Log("Trigger pressed");
        if (!isAnnotationMode || !currentAnchorPreview) return;

        // Create permanent anchor
        GameObject anchor = Instantiate(anchorPrefab, currentAnchorPreview.transform.position, Quaternion.identity);
        
        // Create callout
        CreateCallout(anchor.transform);

        // Reset preview
        Destroy(currentAnchorPreview);
        CreateAnchorPreview();
    }

    private void CreateCallout(Transform anchor)
    {
        // Spawn callout UI
        GameObject callout = Instantiate(calloutPrefab, worldSpaceCanvas.transform);
        
        // Position callout to the right of the anchor with offset
        Vector3 calloutPosition = anchor.position + (Camera.main.transform.right * calloutOffset);
        callout.transform.position = calloutPosition;

        // Set up line renderer between anchor and callout
        LineRenderer line = callout.GetComponent<LineRenderer>();
        if (line)
        {
            line.startColor = line.endColor = lineColor;
            line.positionCount = 2;
            
            // Update line positions
            CalloutController controller = callout.GetComponent<CalloutController>();
            if (controller)
            {
                controller.Initialize(anchor, line);
            }
        }
    }
}

public class CalloutController : MonoBehaviour
{
    private Transform anchorTransform;
    private LineRenderer lineRenderer;
    private RectTransform rectTransform;

    public void Initialize(Transform anchor, LineRenderer line)
    {
        anchorTransform = anchor;
        lineRenderer = line;
        rectTransform = GetComponent<RectTransform>();
        
        // Update line positions immediately
        UpdateLinePositions();
    }

    private void Update()
    {
        if (anchorTransform && lineRenderer)
        {
            UpdateLinePositions();
        }
    }

    private void UpdateLinePositions()
    {
        lineRenderer.SetPosition(0, anchorTransform.position);
        lineRenderer.SetPosition(1, rectTransform.position);
    }
}