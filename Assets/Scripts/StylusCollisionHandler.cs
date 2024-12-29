using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class StylusCollisionHandler : MonoBehaviour
{
    [SerializeField] private XRGrabInteractable grabInteractable;
    [SerializeField] private Rigidbody stylusRigidbody;
    [SerializeField] private Collider[] stylusColliders;
    
    private void Awake()
    {
        // Auto-find components if not set
        if (grabInteractable == null)
            grabInteractable = GetComponent<XRGrabInteractable>();
        if (stylusRigidbody == null)
            stylusRigidbody = GetComponent<Rigidbody>();
        if (stylusColliders == null || stylusColliders.Length == 0)
            stylusColliders = GetComponentsInChildren<Collider>();
            
        // Setup grab interactable events
        grabInteractable.selectEntered.AddListener(OnGrabbed);
        grabInteractable.selectExited.AddListener(OnReleased);
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        // Keep rigidbody kinematic while grabbed
        stylusRigidbody.isKinematic = true;
        
        // Ensure colliders stay active
        foreach (var collider in stylusColliders)
        {
            collider.isTrigger = true;  // Make colliders triggers while grabbed
        }
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        // Return to normal physics when released
        stylusRigidbody.isKinematic = false;
        
        foreach (var collider in stylusColliders)
        {
            collider.isTrigger = false;  // Return colliders to normal
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Handle collision while grabbed
        if (grabInteractable.isSelected)
        {
            Debug.Log($"Stylus collided with: {other.gameObject.name}");
            // Add your sculpting logic here
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // Handle continuous collision while grabbed
        if (grabInteractable.isSelected)
        {
            // Add your continuous sculpting logic here
        }
    }

    private void OnDestroy()
    {
        // Clean up event listeners
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrabbed);
            grabInteractable.selectExited.RemoveListener(OnReleased);
        }
    }
}