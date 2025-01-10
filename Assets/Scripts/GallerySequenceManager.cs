using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class GallerySequenceManager : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionProperty buttonBAction;

    [Header("UI References")]
    [SerializeField] private GameObject aiOutputUI;

    [Header("Coffin References")]
    [SerializeField] private GameObject oldCoffin;
    [SerializeField] private GameObject newCoffin;
    [SerializeField] private Material hologramMaterial;

    [Header("Pot References")]
    [SerializeField] private Animator potAnimator;
    [SerializeField] private float partProcessingDelay = 0.1f;
    [SerializeField] private float floatHeight = 0.5f;
    [SerializeField] private float floatDuration = 2f;
    [SerializeField] private float materialTransitionDuration = 1f;

    [Header("Effect Settings")]
    [SerializeField] private float hologramDuration = 2.5f;
    [SerializeField] private float dissolveDuration = 2.0f;
    
    private enum SequenceState
    {
        Initial,
        UIShown,
        ReadyForReconstruction,
        CoffinReconstructed,
        PotReconstructed
    }

    private SequenceState currentState = SequenceState.Initial;

    private void OnEnable()
    {
        buttonBAction.action.Enable();
        buttonBAction.action.performed += OnButtonBPressed;

        // Initialize scene state
        if (newCoffin != null)
        {
            newCoffin.SetActive(false);
        }
        if (aiOutputUI != null)
        {
            aiOutputUI.SetActive(false);
        }
    }

    private void OnDisable()
    {
        buttonBAction.action.Disable();
        buttonBAction.action.performed -= OnButtonBPressed;
    }

    private void OnButtonBPressed(InputAction.CallbackContext context)
    {
        Debug.Log($"B pressed in state: {currentState}");
        
        switch (currentState)
        {
            case SequenceState.Initial:
                ShowAIOutput();
                break;
                
            case SequenceState.UIShown:
                HideAIOutput();
                break;
                
            case SequenceState.ReadyForReconstruction:
                StartCoffinReconstruction();
                break;
                
            case SequenceState.CoffinReconstructed:
                StartPotReconstruction();
                break;
        }
    }

    private void ShowAIOutput()
    {
        if (aiOutputUI != null)
        {
            aiOutputUI.SetActive(true);
            currentState = SequenceState.UIShown;
            Debug.Log("AI Output UI shown");
        }
    }

    private void HideAIOutput()
    {
        if (aiOutputUI != null)
        {
            aiOutputUI.SetActive(false);
            currentState = SequenceState.ReadyForReconstruction;
            Debug.Log("AI Output UI hidden");
        }
    }

    private void StartCoffinReconstruction()
    {
        StartCoroutine(CoffinReconstructionSequence());
        Debug.Log("Starting coffin reconstruction");
    }

    private void StartPotReconstruction()
    {
        if (potAnimator != null)
        {
            StartCoroutine(PotReconstructionSequence());
            Debug.Log("Starting pot reconstruction");
        }
    }

    private IEnumerator CoffinReconstructionSequence()
    {
        // Apply hologram effect to old coffin
        if (oldCoffin != null && hologramMaterial != null)
        {
            Renderer[] renderers = oldCoffin.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                renderer.material = hologramMaterial;
            }
        }

        yield return new WaitForSeconds(hologramDuration);

        // Switch coffins
        oldCoffin.SetActive(false);
        newCoffin.SetActive(true);

        // Get renderers from new coffin
        Renderer[] newCoffinRenderers = newCoffin.GetComponentsInChildren<Renderer>();
        
        // Set initial dissolve value (invisible)
        foreach (Renderer renderer in newCoffinRenderers)
        {
            renderer.material.SetFloat("_Dissolve_value", 1f);
        }

        // Wait before starting dissolve
        yield return new WaitForSeconds(1f);

        // Animate dissolve effect
        float elapsedTime = 0f;
        while (elapsedTime < dissolveDuration)
        {
            float normalizedTime = elapsedTime / dissolveDuration;
            float dissolveValue = Mathf.Lerp(1f, 0f, normalizedTime);
            
            foreach (Renderer renderer in newCoffinRenderers)
            {
                renderer.material.SetFloat("_Dissolve_value", dissolveValue);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure final state
        foreach (Renderer renderer in newCoffinRenderers)
        {
            renderer.material.SetFloat("_Dissolve_value", 0f);
        }

        currentState = SequenceState.CoffinReconstructed;
        Debug.Log("Coffin reconstruction completed");
    }

    private IEnumerator PotReconstructionSequence()
    {
        if (potAnimator == null) yield break;

        Transform potRoot = potAnimator.transform;
        Vector3 startPosition = potRoot.position;
        Vector3 floatPosition = startPosition + Vector3.up * floatHeight;
        
        // Get all pot part renderers
        Renderer[] potPartRenderers = potRoot.GetComponentsInChildren<Renderer>();
        Material[][] originalMaterials = new Material[potPartRenderers.Length][];

        // Store original materials and initially set hologram
        for (int i = 0; i < potPartRenderers.Length; i++)
        {
            originalMaterials[i] = potPartRenderers[i].materials;
            yield return new WaitForSeconds(partProcessingDelay);
            
            Material[] hologramMaterials = new Material[potPartRenderers[i].materials.Length];
            for (int j = 0; j < hologramMaterials.Length; j++)
            {
                hologramMaterials[j] = hologramMaterial;
            }
            potPartRenderers[i].materials = hologramMaterials;
        }

        // Start reconstruction animation
        potAnimator.SetTrigger("Reconstruct");

        // Float up while reconstructing
        float elapsedTime = 0f;
        while (elapsedTime < floatDuration)
        {
            float t = elapsedTime / floatDuration;
            potRoot.position = Vector3.Lerp(startPosition, floatPosition, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Wait for reconstruction animation to complete
        AnimatorStateInfo stateInfo;
        do
        {
            stateInfo = potAnimator.GetCurrentAnimatorStateInfo(0);
            yield return null;
        } while (!stateInfo.IsName("Animate") || stateInfo.normalizedTime < 0.99f);

        // Trigger end state
        potAnimator.SetTrigger("End");
        currentState = SequenceState.PotReconstructed;
        Debug.Log("Pot reconstruction completed");

        // Transition back to original materials
        elapsedTime = 0f;
        while (elapsedTime < materialTransitionDuration)
        {
            float t = elapsedTime / materialTransitionDuration;
            
            // Update hologram material properties to fade out
            // Assuming your hologram shader has a "_Transparency" property
            hologramMaterial.SetFloat("_Transparency", 1 - t);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Restore original materials
        for (int i = 0; i < potPartRenderers.Length; i++)
        {
            potPartRenderers[i].materials = originalMaterials[i];
        }

        // Float back down
        elapsedTime = 0f;
        while (elapsedTime < floatDuration)
        {
            float t = elapsedTime / floatDuration;
            potRoot.position = Vector3.Lerp(floatPosition, startPosition, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure final position
        potRoot.position = startPosition;


    }
}