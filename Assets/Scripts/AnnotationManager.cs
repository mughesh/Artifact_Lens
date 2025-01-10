using UnityEngine;
using UnityEngine.InputSystem;

public class AnnotationManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject pinPrefab;
    [SerializeField] private GameObject lineRendererPrefab;
    [SerializeField] private Transform stylusTip;
    [SerializeField] private InputActionProperty triggerAction;
    
    [Header("UI References")]
    [SerializeField] private CanvasGroup emptyAnnotationCanvas;
    [SerializeField] private CanvasGroup aiInputAnnotationCanvas;
    [SerializeField] private GameObject switchButton;

    
    private GameObject activePin;
    private GameObject pinPreview;
    private GameObject currentLineRenderer;
    private bool isAnnotationMode = false;
    private bool hasPinPlaced = false;
    private Transform currentLineEndPoint;

    private void OnEnable()
    {
        triggerAction.action.Enable();
        triggerAction.action.performed += OnTriggerPressed;
        
        // Ensure UIs start hidden
        emptyAnnotationCanvas.gameObject.SetActive(false);
        aiInputAnnotationCanvas.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        triggerAction.action.Disable();
        triggerAction.action.performed -= OnTriggerPressed;
    }

    private void Update()
    {
        if (isAnnotationMode && !hasPinPlaced)
        {
            UpdatePinPreview();
        }

        if (hasPinPlaced && activePin != null)
        {
            UpdateLineRenderer();
        }

        CheckSwitchButtonProximity();
    }

    public void StartAnnotationMode()
    {
        isAnnotationMode = true;
        hasPinPlaced = false;
        CreatePinPreview();
    }

    public void StopAnnotationMode()
    {
        isAnnotationMode = false;
        if (pinPreview != null)
        {
            Destroy(pinPreview);
        }
    }

    private void CreatePinPreview()
    {
        if (pinPreview == null)
        {
            pinPreview = Instantiate(pinPrefab);
        }
    }

    private void UpdatePinPreview()
    {
        if (pinPreview != null)
        {
            pinPreview.transform.position = stylusTip.position;
        }
    }

    private void OnTriggerPressed(InputAction.CallbackContext context)
    {
        if (isAnnotationMode && !hasPinPlaced)
        {
            PlacePin();
        }
    }

    private void PlacePin()
    {
        activePin = Instantiate(pinPrefab, stylusTip.position, Quaternion.identity);
        currentLineRenderer = Instantiate(lineRendererPrefab);
        
        ShowEmptyAnnotation();
        
        Destroy(pinPreview);
        hasPinPlaced = true;
    }

    private void ShowEmptyAnnotation()
    {
        emptyAnnotationCanvas.gameObject.SetActive(true);
        aiInputAnnotationCanvas.gameObject.SetActive(false);
        
        // Find line render point in empty annotation canvas
        Transform lineRenderPoint = emptyAnnotationCanvas.transform.Find("LineRenderPoint");
        if (lineRenderPoint != null)
        {
            currentLineEndPoint = lineRenderPoint;
        }
    }

    private void ShowAIAnnotation()
    {
        emptyAnnotationCanvas.gameObject.SetActive(false);
        aiInputAnnotationCanvas.gameObject.SetActive(true);
        
        // Find line render point in AI annotation canvas
        Transform lineRenderPoint = aiInputAnnotationCanvas.transform.Find("LineRenderPoint");
        if (lineRenderPoint != null)
        {
            currentLineEndPoint = lineRenderPoint;
        }
    }

    private void UpdateLineRenderer()
    {
        if (currentLineRenderer != null)
        {
            LineRenderer lineRenderer = currentLineRenderer.GetComponent<LineRenderer>();
            Transform anchorPoint = activePin.transform.Find("AnchorPoint");
            
            if (lineRenderer != null && anchorPoint != null && currentLineEndPoint != null)
            {
                lineRenderer.positionCount = 2;
                lineRenderer.SetPosition(0, anchorPoint.position);
                lineRenderer.SetPosition(1, currentLineEndPoint.position);
            }
        }
    }

    private void CheckSwitchButtonProximity()
    {
        if (switchButton != null && switchButton.activeSelf && hasPinPlaced)
        {
            float distance = Vector3.Distance(stylusTip.position, switchButton.transform.position);
            if (distance < 0.05f) // Adjust threshold as needed
            {
                Debug.Log("Switch button pressed");
                ShowAIAnnotation();
                switchButton.SetActive(false);
            }
        }
    }
}