using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class GallerySequenceManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject aiOutputUI;
    [SerializeField] private InputActionProperty buttonBAction;
    [SerializeField] private CanvasGroup[] contentPanels;
    
    [Header("Animation Settings")]
    [SerializeField] private float panelFadeInDuration = 0.5f;
    [SerializeField] private float contentDelay = 0.2f;
    
    private void OnEnable()
    {
        buttonBAction.action.Enable();
        buttonBAction.action.performed += OnButtonBPressed;
        
        // Ensure UI starts hidden
        if (aiOutputUI != null)
            aiOutputUI.SetActive(false);
    }
    
    private void OnDisable()
    {
        buttonBAction.action.Disable();
        buttonBAction.action.performed -= OnButtonBPressed;
    }
    
    private void OnButtonBPressed(InputAction.CallbackContext context)
    {
        ShowAIOutput();
    }
    
    private void ShowAIOutput()
    {
        if (aiOutputUI != null && !aiOutputUI.activeSelf)
        {
            aiOutputUI.SetActive(true);
            StartCoroutine(AnimateContentPanels());
        }
    }
    
    private IEnumerator AnimateContentPanels()
    {
        foreach (var panel in contentPanels)
        {
            if (panel != null)
            {
                panel.alpha = 0f;
                panel.gameObject.SetActive(true);
                
                float elapsed = 0f;
                while (elapsed < panelFadeInDuration)
                {
                    elapsed += Time.deltaTime;
                    panel.alpha = Mathf.Lerp(0f, 1f, elapsed / panelFadeInDuration);
                    yield return null;
                }
                
                panel.alpha = 1f;
                yield return new WaitForSeconds(contentDelay);
            }
        }
    }
}