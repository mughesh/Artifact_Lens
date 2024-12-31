using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public enum DemoState
{
    Idle,
    CreatingDomainBox,
    Scanning,
    ShowingRedMarks,
    Sculpting,
    ShowingAIAnalysis,
    SwitchingToGallery,
    GalleryIdle,
    ProcessingGlyphs,
    Annotating,
    Reconstructing
}

public class DemoController : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionProperty buttonBAction;
    [SerializeField] private InputActionProperty buttonYAction;

    [Header("Scene References")]
    [SerializeField] private GameObject scanningEffect;
    [SerializeField] private GameObject[] redMarkObjects;
    [SerializeField] private GameObject aiOrb;
    [SerializeField] private GameObject analysisPanelPrefab;
    [SerializeField] private Material redMarkFullMaterial;
    [SerializeField] private Material redMarkOutlineMaterial;
    
    [Header("Gallery Scene References")]
    [SerializeField] private GameObject reconstructedCoffin;
    [SerializeField] private GameObject brokenPot;
    [SerializeField] private GameObject assembledPot;

    private DemoState currentState = DemoState.Idle;
    private UIPanelManager uiManager;

    private void OnEnable()
    {
        buttonBAction.action.Enable();
        buttonYAction.action.Enable();
        buttonBAction.action.performed += OnButtonBPressed;
        buttonYAction.action.performed += OnButtonYPressed;
    }

    public void SetState(DemoState newState)
    {
        currentState = newState;
        HandleStateChange();
    }

    private void HandleStateChange()
    {
        switch (currentState)
        {
            case DemoState.Scanning:
                StartScanning();
                break;
            case DemoState.ShowingRedMarks:
                ShowRedMarks();
                break;
            // Add more state handlers
        }
    }

    private void OnButtonBPressed(InputAction.CallbackContext context)
    {
        // Progress demo based on current state
        switch (currentState)
        {
            case DemoState.Scanning:
                SetState(DemoState.ShowingRedMarks);
                break;
            case DemoState.ShowingRedMarks:
                SetState(DemoState.Sculpting);
                break;
            // Add more state progressions
        }
    }

    private void OnButtonYPressed(InputAction.CallbackContext context)
    {
        // Progress demo based on current state
        Debug.Log("Button Y pressed");
    }

    private void StartScanning()
    {
        if (scanningEffect != null)
        {
            scanningEffect.SetActive(true);
        }
    }

    private void ShowRedMarks()
    {
        if (scanningEffect != null)
        {
            scanningEffect.SetActive(false);
        }
        
        foreach (var redMark in redMarkObjects)
        {
            redMark.SetActive(true);
        }
        
        uiManager.ShowPanel("SculptingInstructions");
    }

    private void HandleStylusProximity(Vector3 stylusPosition)
    {
        if (currentState != DemoState.Sculpting) return;

        foreach (var redMark in redMarkObjects)
        {
            float distance = Vector3.Distance(stylusPosition, redMark.transform.position);
            MeshRenderer renderer = redMark.GetComponent<MeshRenderer>();
            
            if (distance < 0.1f) // Adjust threshold as needed
            {
                renderer.material = redMarkOutlineMaterial;
            }
            else
            {
                renderer.material = redMarkFullMaterial;
            }
        }
    }

    // Add more methods for other states...
}