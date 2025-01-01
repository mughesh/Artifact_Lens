using UnityEngine;
using System.Collections;

public class ScanController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject targetObject; // The coffin
    [SerializeField] private Material hologramShader;
    [SerializeField] private DomainBoxCreator domainBoxCreator;
    
    private Material originalMaterial;
    private MeshRenderer meshRenderer;
    private bool isScanning = false;

    private void Start()
    {
        if (targetObject != null)
        {
            meshRenderer = targetObject.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                originalMaterial = meshRenderer.material;
            }
        }
    }

    public void StartScan()
    {
        if (domainBoxCreator != null)
        {
            domainBoxCreator.ResetDomain();
        }
        Debug.Log("Starting scan");
        
        StartCoroutine(ScanSequence());
    }

    private IEnumerator ScanSequence()
    {
        Debug.Log("Starting scan coroutine");
        if (isScanning || meshRenderer == null) yield break;
        
        isScanning = true;
        
        // Apply hologram shader
        meshRenderer.material = hologramShader;
        
        // Wait for scan duration
        yield return new WaitForSeconds(5f); // Adjust time as needed
        
        // Restore original material
        meshRenderer.material = originalMaterial;
        
        isScanning = false;
        
        Debug.Log("Scan complete - Ready for next phase");
    }

    public void StopScan()
    {
        if (meshRenderer != null)
        {
            meshRenderer.material = originalMaterial;
        }
        isScanning = false;
    }
}