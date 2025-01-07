using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

[System.Serializable]
public class MenuIconData
{
    public Sprite icon;
    public string label;
}

public class RadialSelection : MonoBehaviour
{
    [Header("References")]
    public GameObject radialPrefab;
    public GameObject iconPrefab;
    public Transform radialParent;
    public Transform stylusTip;
    public InputActionProperty buttonAAction;

    [Header("Mode References")]
    [SerializeField] private DomainBoxCreator domainBoxCreator;
    [SerializeField] private ScanController scanController;

    [Header("Menu Items")]
    public MenuIconData[] menuIcons = new MenuIconData[5];
    private MenuMode currentMode = (MenuMode)(-1);

    [Header("Visual Settings")]
    public Color defaultColor = Color.gray;
    public Color hoverColor = Color.blue;
    [Range(1, 10)]
    public float gapSizeInDegrees = 5f;
    public float iconOffset = 70f;
    public float menuRotationOffset = 0f; 
    public float iconAngleOffset = 0f; 

    [Header("Hover Effects")]
    public float hoverScaleMultiplier = 1.1f;
    public float scaleTransitionSpeed = 10f;

    [Header("Settings")]
    [Range(2, 10)]
    public int numberOfRadials = 5; // Changed to 5 for our menu options
    public float selectionRadius = 0.1f;

    public enum MenuMode
    {
        Annotate = 0,    // Top-right
        Passthrough = 1, // Top-left
        Clear = 2,       // Bottom-left
        Scan = 3,        // Bottom
        DomainBox = 4    // Bottom-right
    }

    private GameObject[] radialSegments;
    private GameObject[] iconObjects;
    private TextMeshProUGUI[] iconTexts;
    private Vector3[] originalScales;
    private bool isMenuActive = false;
    private int hoveredSegment = -1;
    private Vector3 menuSpawnPosition;

    private void OnEnable()
    {
        buttonAAction.action.Enable();
        buttonAAction.action.performed += OnButtonAPressed;
        buttonAAction.action.canceled += OnButtonAReleased;
    }

    private void OnDisable()
    {
        buttonAAction.action.Disable();
        buttonAAction.action.performed -= OnButtonAPressed;
        buttonAAction.action.canceled -= OnButtonAReleased;
    }

     private void CreateRadials()
    {
        radialSegments = new GameObject[numberOfRadials];
        iconObjects = new GameObject[numberOfRadials];
        iconTexts = new TextMeshProUGUI[numberOfRadials];
        originalScales = new Vector3[numberOfRadials];
        
        float anglePerSegment = 360f / numberOfRadials;
        float angleBetweenParts = gapSizeInDegrees;

        // Start from top-right and go clockwise
        float startAngle = -18f + menuRotationOffset;

        for (int i = 0; i < numberOfRadials; i++)
        {
            // Create segment
            float angle = startAngle - (i * anglePerSegment);
            GameObject spawnedRadial = Instantiate(radialPrefab, radialParent);
            spawnedRadial.transform.localRotation = Quaternion.Euler(0, 0, angle);
            spawnedRadial.transform.localPosition = Vector3.zero;
            
            Image radialImage = spawnedRadial.GetComponent<Image>();
            radialImage.fillAmount = (1f / numberOfRadials) - (angleBetweenParts / 360f);
            radialImage.color = defaultColor;
            
            radialSegments[i] = spawnedRadial;
            originalScales[i] = spawnedRadial.transform.localScale;

            // Create icon
            CreateIconForSegment(i, angle);
        }
    }

     private void CreateIconForSegment(int index, float segmentAngle)
    {
        if (iconPrefab == null || index >= menuIcons.Length || menuIcons[index] == null)
            return;

        // Calculate position with both menu rotation and icon position offsets
        float adjustedAngle = segmentAngle + 90f + iconAngleOffset;
        float angleRad = adjustedAngle * Mathf.Deg2Rad;
        Vector2 position = new Vector2(
            Mathf.Cos(angleRad) * iconOffset,
            Mathf.Sin(angleRad) * iconOffset
        );

        // Create icon
        GameObject icon = Instantiate(iconPrefab, radialParent);
        icon.transform.localPosition = position;
        icon.transform.localRotation = Quaternion.identity;

        // Set icon image
        Image iconImage = icon.GetComponentInChildren<Image>();
        if (iconImage != null && menuIcons[index].icon != null)
        {
            iconImage.sprite = menuIcons[index].icon;
        }

        // Set text and hide initially
        TextMeshProUGUI iconText = icon.GetComponentInChildren<TextMeshProUGUI>();
        if (iconText != null)
        {
            iconText.text = menuIcons[index].label;
            iconText.gameObject.SetActive(false); // Hide initially
            iconTexts[index] = iconText;
        }

        iconObjects[index] = icon;
    }

    private void Update()
    {
        if (isMenuActive)
        {
            CheckHover();
            UpdateHoverEffects();
        }
    }

        private void UpdateHoverEffects()
    {
        for (int i = 0; i < numberOfRadials; i++)
        {
            if (radialSegments[i] == null) continue;

            // Handle segment scaling
            Vector3 targetScale = originalScales[i];
            if (i == hoveredSegment)
            {
                targetScale *= hoverScaleMultiplier;
            }

            radialSegments[i].transform.localScale = Vector3.Lerp(
                radialSegments[i].transform.localScale,
                targetScale,
                Time.deltaTime * scaleTransitionSpeed
            );

            // Handle text visibility
            if (iconTexts[i] != null)
            {
                iconTexts[i].gameObject.SetActive(i == hoveredSegment);
            }
        }
    }

     private void CheckHover()
    {
        // Reset previous hover state
        if (hoveredSegment >= 0 && hoveredSegment < radialSegments.Length)
        {
            radialSegments[hoveredSegment].GetComponent<Image>().color = defaultColor;
        }

        Vector3 centerToStylus = stylusTip.position - radialParent.position;
        Vector3 projectedVector = Vector3.ProjectOnPlane(centerToStylus, radialParent.forward);
        
        float distance = projectedVector.magnitude;
        hoveredSegment = -1;

        if (distance < selectionRadius)
        {
            float angle = Vector3.SignedAngle(radialParent.up, projectedVector, radialParent.forward);
            angle = (angle - menuRotationOffset + 360f) % 360f;

            // Reverse the segment calculation to fix the hovering direction
            hoveredSegment = numberOfRadials - 1 - Mathf.FloorToInt(((angle + 18f) % 360f) / (360f / numberOfRadials));
            hoveredSegment = (hoveredSegment + numberOfRadials) % numberOfRadials;

            if (hoveredSegment >= 0 && hoveredSegment < radialSegments.Length)
            {
                radialSegments[hoveredSegment].GetComponent<Image>().color = hoverColor;
                //Debug.Log($"Hovering over segment: {hoveredSegment} ({(MenuMode)hoveredSegment})");
            }
        }
    }

    private void OnButtonAPressed(InputAction.CallbackContext context)
    {
        menuSpawnPosition = stylusTip.position;
        radialParent.position = menuSpawnPosition;
        radialParent.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
        
        SetMenuActive(true);
    }

    private void OnButtonAReleased(InputAction.CallbackContext context)
    {
        if (hoveredSegment >= 0 && hoveredSegment < numberOfRadials)
        {
            HandleSelection(hoveredSegment);
        }
        SetMenuActive(false);
    }

     private void SetMenuActive(bool active)
    {
        isMenuActive = active;
        Debug.Log($"Menu active: {active}");

        if (active)
        {
            CreateRadials();
        }
        else
        {
            CleanupMenu();
        }
        
        radialParent.gameObject.SetActive(active);
    }

    private void CleanupMenu()
    {
        if (radialSegments != null)
        {
            foreach (var segment in radialSegments)
            {
                if (segment != null)
                    Destroy(segment);
            }
            radialSegments = null;
        }

        if (iconObjects != null)
        {
            foreach (var icon in iconObjects)
            {
                if (icon != null)
                    Destroy(icon);
            }
            iconObjects = null;
        }

        iconTexts = null;
        originalScales = null;
        hoveredSegment = -1;
    }

private void HandleSelection(int segmentIndex)
{
    MenuMode selectedMode = (MenuMode)segmentIndex;
    Debug.Log($"Selected mode: {selectedMode}");

    // Disable previous mode
    //DisableCurrentMode();

    // Enable new mode
    switch (selectedMode)
    {
        case MenuMode.Annotate:
            Debug.Log("Annotation mode - Ready to place markers");
            currentMode = MenuMode.Annotate;
            break;

        case MenuMode.Passthrough:
            Debug.Log("Switching to Museum Scene");
            StartCoroutine(SwitchToMuseum());
            break;

        case MenuMode.Clear:
            if (domainBoxCreator != null)
            {
                domainBoxCreator.ResetDomain();
                Debug.Log("Scene cleared");
            }
            currentMode = (MenuMode)(-1); // Reset to no mode
            break;

        case MenuMode.Scan:
            if (scanController != null)
            {
                scanController.StartScan();
                Debug.Log("Starting scan sequence");
            }
            currentMode = MenuMode.Scan;
            break;

        case MenuMode.DomainBox:
            if (domainBoxCreator != null)
            {
                domainBoxCreator.enabled = true;
                Debug.Log("Domain Box Creation mode enabled");
            }
            currentMode = MenuMode.DomainBox;
            break;
    }
}

    private void OnDestroy()
    {
        SetMenuActive(false);
    }
    private IEnumerator SwitchToMuseum()
    {
        // Optional: Add fade effect or transition here
        yield return new WaitForSeconds(0.5f);
        UnityEngine.SceneManagement.SceneManager.LoadScene("Gallery");
    }

}