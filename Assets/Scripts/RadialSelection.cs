using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using System.Collections.Generic;

[System.Serializable]
public class RadialMenuIcon
{
    public Sprite icon;
    public string label;
}

public class RadialSelection : MonoBehaviour
{
    [Header("References")]
    public GameObject radialPrefab;
    public Transform radialParent;
    public Transform stylusTip;
    public InputActionProperty buttonAAction;

    [Header("Visual Settings")]
    public Color defaultColor = Color.gray;
    public Color hoverColor = Color.blue;
    [Range(1, 10)]
    public float gapSizeInDegrees = 5f;

    [Header("Settings")]
    [Range(2, 10)]
    public int numberOfRadials = 4;
    public float selectionRadius = 0.1f;

    [Header("Icon Settings")]
    [SerializeField] private GameObject iconPrefab;
    [SerializeField] private RadialMenuIcon[] menuIcons = new RadialMenuIcon[4];
    [SerializeField] private float iconDistanceFromCenter = 70f;
    [SerializeField] private Vector2 iconSize = new Vector2(30f, 30f);

    [Header("Stylus Reference")]
    [SerializeField] private XRGrabInteractable stylusInteractable;
    [SerializeField] private DomainBoxCreator domainBoxCreator; // Reference to your DomainBoxCreator

    private GameObject[] radialSegments;
    private GameObject[] iconObjects;
    private bool isMenuActive = false;
    private int hoveredSegment = -1;
    private Vector3 menuSpawnPosition;
    private bool isStylusGrabbed = false;
    private int currentMode = -1; // -1: No mode, 0: Domain Box, 1: Spatial AI, 2: Sculpt, 3: Clear

    private void Start()
    {
        if (stylusInteractable != null)
        {
            stylusInteractable.selectEntered.AddListener(OnStylusGrabbed);
            stylusInteractable.selectExited.AddListener(OnStylusReleased);
        }
        
        // Ensure domain box creator starts disabled
        if (domainBoxCreator != null)
        {
            domainBoxCreator.enabled = false;
        }
    }

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

    private void OnStylusGrabbed(SelectEnterEventArgs args)
    {
        isStylusGrabbed = true;
    }

    private void OnStylusReleased(SelectExitEventArgs args)
    {
        isStylusGrabbed = false;
    }

    void CreateRadials()
    {
        radialSegments = new GameObject[numberOfRadials];
        iconObjects = new GameObject[numberOfRadials];
        float anglePerSegment = 360f / numberOfRadials;
        float angleBetweenParts = gapSizeInDegrees;

        for (int i = 0; i < numberOfRadials; i++)
        {
            // Create segment
            float angle = -i * anglePerSegment - angleBetweenParts / 2f;
            GameObject spawnedRadial = Instantiate(radialPrefab, radialParent);
            spawnedRadial.transform.localRotation = Quaternion.Euler(0, 0, angle);
            spawnedRadial.transform.localPosition = Vector3.zero;
            
            Image radialImage = spawnedRadial.GetComponent<Image>();
            radialImage.fillAmount = (1f / numberOfRadials) - (angleBetweenParts / 360f);
            radialImage.color = defaultColor;
            
            radialSegments[i] = spawnedRadial;

            // Create icon for this segment
            CreateIconForSegment(i, spawnedRadial.transform);
        }
    }

    private void CreateIconForSegment(int index, Transform segmentTransform)
    {
        if (iconPrefab == null || index >= menuIcons.Length || menuIcons[index].icon == null)
        {
            Debug.LogWarning($"Missing icon setup for segment {index}");
            return;
        }

        // Create icon GameObject
        GameObject icon = Instantiate(iconPrefab, segmentTransform);
        icon.name = $"Icon_{index}";
        
        // Position the icon
        RectTransform iconRect = icon.GetComponent<RectTransform>();
        iconRect.sizeDelta = iconSize;
        
        // Use fixed offset for all icons
        float offset = iconDistanceFromCenter;
        iconRect.anchoredPosition = new Vector2(offset, offset);
        
        // Set the icon sprite
        Image iconImage = icon.GetComponent<Image>();
        iconImage.sprite = menuIcons[index].icon;
        iconImage.color = Color.white;
        
        iconObjects[index] = icon;
    }

    private void Update()
    {
        if (isMenuActive)
        {
            CheckHover();
        }
    }

    private void CheckHover()
    {
        if (hoveredSegment >= 0)
        {
            radialSegments[hoveredSegment].GetComponent<Image>().color = defaultColor;
        }

        Vector3 centerToStylus = stylusTip.position - radialParent.position;
        Vector3 projectedVector = Vector3.ProjectOnPlane(centerToStylus, radialParent.forward);
        
        float distance = projectedVector.magnitude;

        if (distance < selectionRadius)
        {
            float angle = Vector3.SignedAngle(radialParent.up, projectedVector, radialParent.forward);
            
            if (angle < 0)
                angle += 360f;
                
            hoveredSegment = numberOfRadials - 1 - (int)(angle * numberOfRadials / 360f);
            hoveredSegment = (hoveredSegment + numberOfRadials) % numberOfRadials;

            if (hoveredSegment >= 0 && hoveredSegment < numberOfRadials)
            {
                radialSegments[hoveredSegment].GetComponent<Image>().color = hoverColor;
            }
        }
        else
        {
            hoveredSegment = -1;
        }
    }

    private void OnButtonAPressed(InputAction.CallbackContext context)
    {
        if (!isStylusGrabbed) return; // Only show menu if stylus is grabbed

        menuSpawnPosition = stylusTip.position;
        radialParent.position = menuSpawnPosition;
        radialParent.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
        
        SetMenuActive(true);
    }

    private void OnButtonAReleased(InputAction.CallbackContext context)
    {
        if (hoveredSegment >= 0)
        {
            HandleSelection(hoveredSegment);
        }
        SetMenuActive(false);
    }

    private void SetMenuActive(bool active)
    {
        isMenuActive = active;
        radialParent.gameObject.SetActive(active);
        
        if (active)
        {
            CreateRadials();
        }
        else
        {
            // Cleanup
            if (radialSegments != null)
            {
                foreach (var segment in radialSegments)
                {
                    if (segment != null)
                        Destroy(segment);
                }
            }
            hoveredSegment = -1;
        }
    }

    private void HandleSelection(int segmentIndex)
    {
        currentMode = segmentIndex;
        
        // Handle different modes
        switch (segmentIndex)
        {
            case 3: // Domain Box Creation Mode
                if (domainBoxCreator != null)
                {
                    domainBoxCreator.enabled = true;
                }
                Debug.Log("Domain Box Creation Mode Enabled");
                break;
                
            case 0: // Spatial AI Mode
                if (domainBoxCreator != null)
                {
                    domainBoxCreator.enabled = false;
                }
                Debug.Log("Spatial AI Mode Enabled");
                break;
                
            case 2: // Sculpt Mode
                if (domainBoxCreator != null)
                {
                    domainBoxCreator.enabled = false;
                }
                Debug.Log("Sculpt Mode Enabled");
                break;
                
            case 1: // Clear Scene Mode
                if (domainBoxCreator != null)
                {
                    domainBoxCreator.ResetDomain();
                    domainBoxCreator.enabled = false;
                }
                currentMode = -1; // Reset mode after clearing
                Debug.Log("Scene Cleared");
                break;
        }
    }

    private void OnDestroy()
    {
        if (stylusInteractable != null)
        {
            stylusInteractable.selectEntered.RemoveListener(OnStylusGrabbed);
            stylusInteractable.selectExited.RemoveListener(OnStylusReleased);
        }
        SetMenuActive(false);
    }
}