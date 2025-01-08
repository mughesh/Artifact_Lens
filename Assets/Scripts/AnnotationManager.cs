using UnityEngine;
using UnityEngine.InputSystem;

public class AnnotationManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject anchorPrefab;
    [SerializeField] private GameObject annotationUIPanel; // Reference to your existing UI panel
    [SerializeField] private Transform stylusTip;
    [SerializeField] private InputActionProperty triggerAction;
    
    [Header("Line Renderer Settings")]
    [SerializeField] private LineRenderer existingLineRenderer; // Reference to your UI's line renderer
    
    [Header("Settings")]
    [SerializeField] private float previewAlpha = 0.5f;
    
    private GameObject currentAnchorPreview;
    private GameObject placedAnchor;
    private bool isAnnotationMode;
    private bool hasPlacedAnchor;
    private RectTransform uiRectTransform;

    private void Start()
    {
        if (annotationUIPanel != null)
        {
            uiRectTransform = annotationUIPanel.GetComponent<RectTransform>();
            annotationUIPanel.SetActive(false);
        }
        
        if (existingLineRenderer != null)
        {
            existingLineRenderer.enabled = false;
        }
    }

    private void OnEnable()
    {
        triggerAction.action.Enable();
        triggerAction.action.performed += OnTriggerPressed;
    }

    private void OnDisable()
    {
        triggerAction.action.Disable();
        triggerAction.action.performed -= OnTriggerPressed;
    }

    public void StartAnnotationMode()
    {
        isAnnotationMode = true;
        hasPlacedAnchor = false;
        CreateAnchorPreview();
    }

    public void StopAnnotationMode()
    {
        isAnnotationMode = false;
        if (currentAnchorPreview)
        {
            Destroy(currentAnchorPreview);
        }
        
        // Hide UI and line renderer
        if (annotationUIPanel != null)
        {
            annotationUIPanel.SetActive(false);
        }
        if (existingLineRenderer != null)
        {
            existingLineRenderer.enabled = false;
        }
        
        hasPlacedAnchor = false;
        placedAnchor = null;
    }

    private void Update()
    {
        if (isAnnotationMode && currentAnchorPreview && !hasPlacedAnchor)
        {
            currentAnchorPreview.transform.position = stylusTip.position;
        }
        
        UpdateLineRenderer();
    }

    private void UpdateLineRenderer()
    {
        if (!hasPlacedAnchor || placedAnchor == null || existingLineRenderer == null || !annotationUIPanel.activeSelf)
            return;

        // Set line start point at anchor
        existingLineRenderer.SetPosition(0, placedAnchor.transform.position);
        
        // Calculate bottom center of UI panel for line end point
        if (uiRectTransform != null)
        {
            Vector3 bottomCenter = uiRectTransform.position - (uiRectTransform.up * (uiRectTransform.rect.height * 0.5f));
            existingLineRenderer.SetPosition(1, bottomCenter);
        }
    }

    private void CreateAnchorPreview()
    {
        if (!hasPlacedAnchor)
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
    }

    private void OnTriggerPressed(InputAction.CallbackContext context)
    {
        if (!isAnnotationMode || !currentAnchorPreview || hasPlacedAnchor) return;

        // Create permanent anchor
        placedAnchor = Instantiate(anchorPrefab, currentAnchorPreview.transform.position, Quaternion.identity);
        
        // Enable UI and line renderer
        if (annotationUIPanel != null)
        {
            annotationUIPanel.SetActive(true);
        }
        if (existingLineRenderer != null)
        {
            existingLineRenderer.enabled = true;
        }

        // Clean up preview
        Destroy(currentAnchorPreview);
        hasPlacedAnchor = true;
        currentAnchorPreview = null;
    }
}