using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class AnnotationSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject markerPrefab;
    [SerializeField] private GameObject uiPanelPrefab;
    [SerializeField] private Transform stylusTip;
    [SerializeField] private InputActionProperty triggerAction;

    private bool isAnnotationMode = false;

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

    public void EnableAnnotationMode(bool enable)
    {
        isAnnotationMode = enable;
        Debug.Log($"Annotation mode {(enable ? "enabled" : "disabled")}");
    }

    private void OnTriggerPressed(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (!isAnnotationMode) return;

        // Place marker at stylus tip position
        Vector3 markerPosition = stylusTip.position;
        
        // For now, just log the position
        Debug.Log($"Annotation marker placed at: {markerPosition}");
        
        // Placeholder for future implementation
        // GameObject marker = Instantiate(markerPrefab, markerPosition, Quaternion.identity);
        // GameObject panel = Instantiate(uiPanelPrefab, markerPosition + offset, Quaternion.identity);
    }
}