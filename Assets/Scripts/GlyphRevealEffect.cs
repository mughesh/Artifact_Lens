using UnityEngine;

public class GlyphRevealEffect : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float revealDuration = 2f;
    [SerializeField] private Color glowColor = Color.cyan;
    [SerializeField] private float glowIntensity = 2f;

    private Material instancedMaterial;
    private float currentRevealTime = 0f;
    private bool isRevealing = false;
    private bool isRevealed = false;
    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

    private void Start()
    {
        // Create a unique material instance for this glyph
        Renderer renderer = GetComponent<Renderer>();
        instancedMaterial = new Material(renderer.material);
        renderer.material = instancedMaterial;
        
        // Set initial transparency
        SetTransparency(0f);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("StylusTip") && !isRevealed)
        {
            Debug.Log("Stylus tip is inside the glyph");
            isRevealing = true;
            currentRevealTime += Time.deltaTime;

            // Calculate reveal progress
            float progress = Mathf.Clamp01(currentRevealTime / revealDuration);
            SetTransparency(progress);

            // Add glow effect as it reveals
            Color emissionColor = glowColor * (progress * glowIntensity);
            instancedMaterial.SetColor(EmissionColor, emissionColor);

            if (progress >= 1f)
            {
                isRevealed = true;
                isRevealing = false;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("StylusTip") && !isRevealed)
        {
            isRevealing = false;
            // Optional: Uncomment to make progress reset when stylus leaves
            // currentRevealTime = 0f;
            // SetTransparency(0f);
        }
    }

    private void SetTransparency(float alpha)
    {
        Color color = instancedMaterial.color;
        color.a = alpha;
        instancedMaterial.color = color;
    }

    private void OnDestroy()
    {
        // Clean up the instanced material
        if (instancedMaterial != null)
        {
            Destroy(instancedMaterial);
        }
    }
}