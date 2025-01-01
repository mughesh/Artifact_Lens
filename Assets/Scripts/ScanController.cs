using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class ScanController : MonoBehaviour
{
    [Header("Asset References")]
    [SerializeField] private Transform coffinRoot;  // Parent of coffin meshes
    [SerializeField] private Transform redMarksRoot;  // Parent of red marks
    [SerializeField] private Transform glyphsRoot;   // Parent of glyph symbols
    [SerializeField] private Transform stylusTip;
    
    [Header("Materials")]
    [SerializeField] private Material hologramMaterial;
    [SerializeField] private Material greenBoundaryMaterial;
    
    [Header("UI References")]
    [SerializeField] private CanvasGroup instructionsPanel;
    [SerializeField] private CanvasGroup scanCompletePanel;
    [SerializeField] private CanvasGroup aiAnalysisPanel;
    [SerializeField] private float uiAnimationDuration = 0.5f;
    [SerializeField] private float uiStartYOffset = -50f;
    
    [Header("Input")]
    [SerializeField] private InputActionProperty buttonBAction;
    [SerializeField] private float stylusProximityDistance = 0.1f;

    private enum ScanState
    {
        NotStarted,
        InitialScan,
        Sculpting,
        SculptingComplete,
        ScanComplete,
        AIAnalysis
    }

    private ScanState currentState = ScanState.NotStarted;
    private Dictionary<MeshRenderer, Material> originalCoffinMaterials = new Dictionary<MeshRenderer, Material>();
    private List<Transform> redMarkObjects = new List<Transform>();
    private bool isScanning = false;

    private void OnEnable()
    {
        buttonBAction.action.Enable();
        buttonBAction.action.performed += OnButtonBPressed;
        
        // Initialize object states
        if (redMarksRoot != null) redMarksRoot.gameObject.SetActive(false);
        if (glyphsRoot != null) glyphsRoot.gameObject.SetActive(false);
        
        // Initialize UI states
        SetupUI();
        
        // Cache red mark objects
        if (redMarksRoot != null)
        {
            foreach (Transform child in redMarksRoot)
            {
                if (child.name.StartsWith("redMark"))
                {
                    redMarkObjects.Add(child);
                }
            }
        }
    }

    private void OnDisable()
    {
        buttonBAction.action.Disable();
        buttonBAction.action.performed -= OnButtonBPressed;
    }

    private void SetupUI()
    {
        CanvasGroup[] panels = { instructionsPanel, scanCompletePanel, aiAnalysisPanel };
        foreach (var panel in panels)
        {
            if (panel != null)
            {
                panel.alpha = 0f;
                panel.gameObject.SetActive(false);
            }
        }
    }

    public void StartScan()
    {
        if (currentState != ScanState.NotStarted) return;
        
        // Store and replace coffin materials
        MeshRenderer[] coffinRenderers = coffinRoot.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer renderer in coffinRenderers)
        {
            originalCoffinMaterials[renderer] = renderer.material;
            renderer.material = hologramMaterial;
        }
        
        isScanning = true;
        currentState = ScanState.InitialScan;
        Debug.Log("Initial scan started");
    }

    private void Update()
    {
        if (currentState == ScanState.Sculpting)
        {
            CheckStylusProximity();
        }
    }

    private void CheckStylusProximity()
    {
        foreach (Transform redMark in redMarkObjects)
        {
            if (redMark == null) continue;
            
            float distance = Vector3.Distance(stylusTip.position, redMark.position);
            Transform hologramChild = redMark.GetChild(0);
            
            if (hologramChild != null)
            {
                hologramChild.gameObject.SetActive(distance > stylusProximityDistance);
            }

            // Check if this redMark has associated glyphs and handle their visibility
            // This is a placeholder for the gradual reveal logic you'll implement
            HandleGlyphVisibility(redMark, distance);
        }
    }

    private void HandleGlyphVisibility(Transform redMark, float distance)
    {
        // This method will be implemented later for gradual glyph reveal
        // Based on stylus proximity and movement
    }

    private void OnButtonBPressed(InputAction.CallbackContext context)
    {
        switch (currentState)
        {
            case ScanState.InitialScan:
                EndInitialScan();
                break;
            case ScanState.Sculpting:
                CompleteSculpting();
                break;
            case ScanState.SculptingComplete:
                ShowScanComplete();
                break;
            case ScanState.ScanComplete:
                ShowAIAnalysis();
                break;
        }
    }

    private void EndInitialScan()
    {
        // Restore original coffin materials
        foreach (var kvp in originalCoffinMaterials)
        {
            if (kvp.Key != null)
            {
                kvp.Key.material = kvp.Value;
            }
        }
        
        // Enable red marks and glyphs
        if (redMarksRoot != null) redMarksRoot.gameObject.SetActive(true);
        if (glyphsRoot != null) glyphsRoot.gameObject.SetActive(true);
        
        // Show instructions UI
        StartCoroutine(AnimateUIPanel(instructionsPanel, true));
        
        currentState = ScanState.Sculpting;
        Debug.Log("Entering sculpting phase");
    }

    private void CompleteSculpting()
    {
        // Change redmark materials to green
        foreach (Transform redMark in redMarkObjects)
        {
            MeshRenderer renderer = redMark.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.material = greenBoundaryMaterial;
            }
        }
        
        currentState = ScanState.SculptingComplete;
        Debug.Log("Sculpting complete");
    }

    private void ShowScanComplete()
    {
        // Hide instructions panel
        StartCoroutine(AnimateUIPanel(instructionsPanel, false));
        
        // Disable red marks and glyphs
        if (redMarksRoot != null) redMarksRoot.gameObject.SetActive(false);
        if (glyphsRoot != null) glyphsRoot.gameObject.SetActive(false);
        
        // Show scan complete UI
        StartCoroutine(AnimateUIPanel(scanCompletePanel, true));
        
        currentState = ScanState.ScanComplete;
        Debug.Log("Scan complete");
    }

    private void ShowAIAnalysis()
    {
        StartCoroutine(AnimateUIPanel(scanCompletePanel, false));
        StartCoroutine(AnimateUIPanel(aiAnalysisPanel, true));
        
        currentState = ScanState.AIAnalysis;
        Debug.Log("Showing AI analysis");
    }

    private IEnumerator AnimateUIPanel(CanvasGroup panel, bool show)
    {
        if (panel == null) yield break;

        RectTransform rectTransform = panel.GetComponent<RectTransform>();
        panel.gameObject.SetActive(true);
        
        float startTime = Time.time;
        Vector2 startPos = rectTransform.anchoredPosition;
        Vector2 targetPos = startPos;

        if (show)
        {
            startPos.y += uiStartYOffset;
            panel.alpha = 0f;
        }
        else
        {
            targetPos.y += uiStartYOffset;
        }

        while (Time.time < startTime + uiAnimationDuration)
        {
            float t = (Time.time - startTime) / uiAnimationDuration;
            t = show ? Mathf.Sin(t * Mathf.PI * 0.5f) : 1f - Mathf.Sin((1f - t) * Mathf.PI * 0.5f);
            
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            panel.alpha = show ? t : 1f - t;
            
            yield return null;
        }

        rectTransform.anchoredPosition = targetPos;
        panel.alpha = show ? 1f : 0f;
        
        if (!show)
        {
            panel.gameObject.SetActive(false);
        }
    }
}