using UnityEngine;
using UnityEngine.UI;

public class RadialMenuIcons : MonoBehaviour
{
    [Header("Icon References")]
    [SerializeField] private GameObject iconPrefab; // Your icon prefab
    [SerializeField] private Sprite[] iconSprites; // Array of sprites for each segment
    
    [Header("Icon Settings")]
    [SerializeField] private float iconRadius = 0.7f; // Distance from center (normalized)
    [SerializeField] private float iconSize = 30f; // Size of icon in pixels
    
    private GameObject[] icons;
    private RectTransform menuRectTransform;
    
    private void Awake()
    {
        menuRectTransform = GetComponent<RectTransform>();
        icons = new GameObject[4]; // For 4 segments
    }

    public void InitializeIcons()
    {
        // Clear existing icons if any
        ClearIcons();
        
        // Create new icons
        for (int i = 0; i < 4; i++)
        {
            CreateIcon(i);
        }
    }

    private void CreateIcon(int index)
    {
        if (iconPrefab == null || iconSprites == null || index >= iconSprites.Length)
            return;

        // Calculate position
        float angle = -index * (360f / 4) - (360f / 8); // Center of each segment
        Vector2 position = CalculateIconPosition(angle);

        // Create icon
        GameObject icon = Instantiate(iconPrefab, menuRectTransform);
        RectTransform iconRect = icon.GetComponent<RectTransform>();
        
        // Set position and size
        iconRect.anchoredPosition = position;
        iconRect.sizeDelta = new Vector2(iconSize, iconSize);
        
        // Set sprite
        Image iconImage = icon.GetComponent<Image>();
        if (iconImage != null && index < iconSprites.Length)
        {
            iconImage.sprite = iconSprites[index];
        }

        icons[index] = icon;
    }

    private Vector2 CalculateIconPosition(float angleDegrees)
    {
        float angleRadians = angleDegrees * Mathf.Deg2Rad;
        float radius = menuRectTransform.rect.width * 0.5f * iconRadius;
        
        float x = Mathf.Sin(angleRadians) * radius;
        float y = Mathf.Cos(angleRadians) * radius;
        
        return new Vector2(x, y);
    }

    private void ClearIcons()
    {
        if (icons != null)
        {
            foreach (GameObject icon in icons)
            {
                if (icon != null)
                    Destroy(icon);
            }
        }
    }

    private void OnDisable()
    {
        ClearIcons();
    }
}