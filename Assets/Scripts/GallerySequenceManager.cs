using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class GallerySequenceManager : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionProperty buttonBAction;

    [Header("UI References")]
    [SerializeField] private GameObject aiOutputUI;

    [Header("Artifact References")]
    [SerializeField] private GameObject oldCoffin;
    [SerializeField] private GameObject newCoffin;
    [SerializeField] private Material hologramMaterial;

    [Header("Effect Settings")]
    [SerializeField] private float hologramDuration = 2.5f;
    [SerializeField] private float dissolveDuration = 2.0f;
    
    private enum SequenceState
    {
        Initial,
        UIShown,
        ReadyForReconstruction,
        Completed
    }

    private SequenceState currentState = SequenceState.Initial;
    private Material[] originalMaterials;

    private void OnEnable()
    {
        buttonBAction.action.Enable();
        buttonBAction.action.performed += OnButtonBPressed;

        // Store original materials for later restoration if needed
        if (oldCoffin != null)
        {
            Renderer[] renderers = oldCoffin.GetComponentsInChildren<Renderer>();
            originalMaterials = new Material[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                originalMaterials[i] = renderers[i].material;
            }
        }

        // Ensure new coffin starts hidden
        if (newCoffin != null)
        {
            newCoffin.SetActive(false);
        }
    }

    private void OnDisable()
    {
        buttonBAction.action.Disable();
        buttonBAction.action.performed -= OnButtonBPressed;
    }

    private void OnButtonBPressed(InputAction.CallbackContext context)
    {
        switch (currentState)
        {
            case SequenceState.Initial:
                ShowUI();
                break;
            case SequenceState.UIShown:
                HideUI();
                break;
            case SequenceState.ReadyForReconstruction:
                StartReconstruction();
                break;
        }
    }

    private void ShowUI()
    {
        if (aiOutputUI != null)
        {
            aiOutputUI.SetActive(true);
            currentState = SequenceState.UIShown;
        }
    }

    private void HideUI()
    {
        if (aiOutputUI != null)
        {
            aiOutputUI.SetActive(false);
            currentState = SequenceState.ReadyForReconstruction;
        }
    }

    private void StartReconstruction()
    {
        StartCoroutine(ReconstructionSequence());
        currentState = SequenceState.Completed;
    }

    private IEnumerator ReconstructionSequence()
    {
        // Apply hologram effect
        if (oldCoffin != null && hologramMaterial != null)
        {
            Renderer[] renderers = oldCoffin.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                renderer.material = hologramMaterial;
            }
        }

        yield return new WaitForSeconds(hologramDuration);

        // Hide old coffin and prepare new one
        oldCoffin.SetActive(false);
        newCoffin.SetActive(true);

        // Get the renderers from the new coffin parts
        Renderer[] newCoffinRenderers = newCoffin.GetComponentsInChildren<Renderer>();
        
        // Set initial dissolve value (1 = invisible)
        foreach (Renderer renderer in newCoffinRenderers)
        {
            renderer.material.SetFloat("_Dissolve_value", 1f);
        }

        // Wait before starting dissolve effect
        yield return new WaitForSeconds(1f);

        // Animate dissolve effect
        float elapsedTime = 0f;
        
        while (elapsedTime < dissolveDuration)
        {
            float normalizedTime = elapsedTime / dissolveDuration;
            // Lerp from 1 to 0 (invisible to visible)
            float dissolveValue = Mathf.Lerp(1f, 0f, normalizedTime);
            
            foreach (Renderer renderer in newCoffinRenderers)
            {
                renderer.material.SetFloat("_Dissolve_value", dissolveValue);
                Debug.Log($"Setting dissolve value: {dissolveValue}");
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure final value is set
        foreach (Renderer renderer in newCoffinRenderers)
        {
            renderer.material.SetFloat("_Dissolve_value", 0f);
        }
    }
}