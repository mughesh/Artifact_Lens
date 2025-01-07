using UnityEngine;

public class StylusContactGlow : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject glowPlanePrefab;  // Assign your plane with gradient texture
    [SerializeField] private Transform stylusTip;         // The actual tip transform
    
    [Header("Settings")]
    [SerializeField] private float glowSize = 0.02f;      // Size of the glow effect
    [SerializeField] private float offsetDistance = 0.001f; // Small offset to prevent z-fighting

    
    private GameObject currentGlowPlane;
    private Material glowMaterial;
    private bool isInContact;
    private Vector3 lastContactNormal;
    private Vector3 lastContactPoint;
    private static readonly int AlphaProperty = Shader.PropertyToID("_Alpha");
    
    private void Start()
    {
        Debug.Log($"StylusContactGlow initialized on {gameObject.name}");
        
        // Verify collider setup
        var collider = GetComponent<SphereCollider>();
        if (collider)
        {
            Debug.Log($"Collider found: IsTrigger={collider.isTrigger}, Radius={collider.radius}");
        }
        else
        {
            Debug.LogError("No SphereCollider found!");
        }
        
        // Create the glow plane
        currentGlowPlane = Instantiate(glowPlanePrefab, transform);
        currentGlowPlane.transform.localScale = Vector3.one * glowSize;
        
        // Get and store the material instance
        Renderer renderer = currentGlowPlane.GetComponent<Renderer>();
        glowMaterial = new Material(renderer.material);
        renderer.material = glowMaterial;
        
        // Initially hide the glow
        SetGlowAlpha(0f);
        currentGlowPlane.SetActive(false);
    }

    private void OnTriggerStay(Collider other)
    {
        // Get the contact point (use closest point if no direct contact)
        Vector3 contactPoint = other.ClosestPoint(transform.position);
        
        // Calculate normal - if we hit a mesh collider, try to get its normal
        Vector3 normal;
        if (other is MeshCollider meshCollider)
        {
            // Ray from slightly above the contact point
            Ray ray = new Ray(contactPoint + Vector3.up * 0.01f, Vector3.down);
            if (meshCollider.Raycast(ray, out RaycastHit hit, 0.02f))
            {
                normal = hit.normal;
            }
            else
            {
                // Fallback - use direction from collider to contact point
                normal = (contactPoint - other.transform.position).normalized;
            }
        }
        else
        {
            // For simple colliders, estimate normal
            normal = (contactPoint - other.bounds.center).normalized;
        }

        UpdateGlowPosition(contactPoint, normal);
        isInContact = true;
        
        Debug.Log($"Trigger Stay: Contact at {contactPoint}, Normal: {normal}"); // Debug log
    }

    private void OnTriggerExit(Collider other)
    {
        isInContact = false;
        Debug.Log("Trigger Exit"); // Debug log
    }

    private void UpdateGlowPosition(Vector3 contactPoint, Vector3 normal)
    {
        lastContactPoint = contactPoint;
        lastContactNormal = normal;

        // Position slightly above the surface to prevent z-fighting
        Vector3 position = contactPoint + normal * offsetDistance;
        
        // Orient the plane to align with the surface
        Quaternion rotation = Quaternion.LookRotation(normal);
        // Rotate 90 degrees around X to make plane parallel to surface
        rotation *= Quaternion.Euler(90f, 0f, 0f);
        
        currentGlowPlane.transform.SetPositionAndRotation(position, rotation);
    }

    private void Update()
    {
        if (!currentGlowPlane) return;

        // Simply show/hide based on contact state
        currentGlowPlane.SetActive(isInContact);
        if (isInContact)
        {
            SetGlowAlpha(1f);
            UpdateGlowPosition(lastContactPoint, lastContactNormal);
        }
        else
        {
            SetGlowAlpha(0f);
        }
    }

    private void SetGlowAlpha(float alpha)
    {
        if (glowMaterial)
        {
            glowMaterial.SetFloat(AlphaProperty, alpha);
        }
    }

    private void OnDestroy()
    {
        if (glowMaterial)
        {
            Destroy(glowMaterial);
        }
    }
}