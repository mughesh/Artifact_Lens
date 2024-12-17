using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class StylusPoseManager : MonoBehaviour
{
    [Header("Hand Models")]
    [SerializeField] private GameObject defaultRightHand; // Reference to Right Hand Model under Right Controller
    [SerializeField] private GameObject penHoldingHand;   // Reference to Right Hand Model pose
    
    [Header("Stylus Reference")]
    [SerializeField] private XRGrabInteractable stylusInteractable;

    private void Start()
    {
        if (stylusInteractable != null)
        {
            stylusInteractable.selectEntered.AddListener(OnStylusGrabbed);
            stylusInteractable.selectExited.AddListener(OnStylusReleased);
        }

        // Ensure default state
        defaultRightHand.SetActive(true);
        penHoldingHand.SetActive(false);
    }

    private void OnStylusGrabbed(SelectEnterEventArgs args)
    {
        // Check if it's the right hand controller that grabbed
        if (args.interactorObject is XRDirectInteractor directInteractor)
        {   
            Debug.Log("Stylus Grabbed");
            if (directInteractor.CompareTag("RightHand")) // Make sure to tag your right controller
            {
                
                defaultRightHand.SetActive(false);
                penHoldingHand.SetActive(true);
            }
        }
    }

    private void OnStylusReleased(SelectExitEventArgs args)
    {
        // Check if it's the right hand controller that released
        if (args.interactorObject is XRDirectInteractor directInteractor)
        {
            if (directInteractor.CompareTag("RightHand"))
            {
                defaultRightHand.SetActive(true);
                penHoldingHand.SetActive(false);
            }
        }
    }

    private void OnDestroy()
    {
        if (stylusInteractable != null)
        {
            stylusInteractable.selectEntered.RemoveListener(OnStylusGrabbed);
            stylusInteractable.selectExited.RemoveListener(OnStylusReleased);
        }
    }
}