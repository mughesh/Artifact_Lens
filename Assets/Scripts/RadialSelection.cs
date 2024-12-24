using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

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
    public float selectionRadius = 0.1f; // Radius for selection detection

    private GameObject[] radialSegments;
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

    void Start()
    {
        CreateRadials();
        SetMenuActive(false);
    }

    void Update()
    {
        if (isMenuActive)
        {
            CheckHover();
        }
    }

private void CreateRadials()
{
    radialSegments = new GameObject[numberOfRadials];
    float anglePerSegment = 360f / numberOfRadials;
    float angleBetweenParts = 5f; // Gap between segments

    for (int i = 0; i < numberOfRadials; i++)
    {
        float angle = -i * anglePerSegment - angleBetweenParts / 2f;
        GameObject spawnedRadial = Instantiate(radialPrefab, radialParent);
        spawnedRadial.transform.localRotation = Quaternion.Euler(0, 0, angle);
        spawnedRadial.transform.localPosition = Vector3.zero;
        
        Image radialImage = spawnedRadial.GetComponent<Image>();
        radialImage.fillAmount = (1f / numberOfRadials) - (angleBetweenParts / 360f);
        radialImage.color = defaultColor;
        
        radialSegments[i] = spawnedRadial;
    }
}

private void CheckHover()
{
    // Reset previous hover state
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
            
        // Reverse the segment index calculation
        hoveredSegment = numberOfRadials - 1 - (int)(angle * numberOfRadials / 360f);
        
        // Wrap around to ensure valid index
        hoveredSegment = (hoveredSegment + numberOfRadials) % numberOfRadials;
        
        Debug.Log($"Angle: {angle}, Hovering Segment: {hoveredSegment}");

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
        // Store spawn position and face menu towards camera
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
            Debug.Log($"Selected: {hoveredSegment} (Segment on release {hoveredSegment})");
        }
        SetMenuActive(false);
    }

    private void SetMenuActive(bool active)
    {
        isMenuActive = active;
        Debug.Log($"Menu active: {active}");
        radialParent.gameObject.SetActive(active);
        
        if (!active)
        {
            hoveredSegment = -1;
            // Reset all segments to default color
            foreach (var segment in radialSegments)
            {
                segment.GetComponent<Image>().color = defaultColor;
            }
        }
    }

    private void HandleSelection(int segmentIndex)
    {
        string selectedMode = segmentIndex switch
        {
            0 => "Domain Box Creation Mode",
            1 => "Spatial AI Mode",
            2 => "Sculpt Mode",
            3 => "Clear Scene Mode",
            _ => "Unknown Mode"
        };

        Debug.Log($"Selected: {selectedMode} (Segment {segmentIndex})");
    }

    
}