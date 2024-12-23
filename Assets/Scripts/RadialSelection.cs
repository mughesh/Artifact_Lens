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

        for (int i = 0; i < numberOfRadials; i++)
        {
            float angle = i * anglePerSegment;
            GameObject spawnedRadial = Instantiate(radialPrefab, radialParent);
            spawnedRadial.transform.localRotation = Quaternion.Euler(0, 0, angle);
            spawnedRadial.transform.localPosition = Vector3.zero;
            
            // Set fill amount for the radial segment
            Image radialImage = spawnedRadial.GetComponent<Image>();
            radialImage.fillAmount = 1f / numberOfRadials;
            
            radialSegments[i] = spawnedRadial;
        }
    }

    private void CheckHover()
    {
        // Convert stylus position to local space relative to menu
        Vector3 localStylusPos = radialParent.InverseTransformPoint(stylusTip.position);
        float angle = Mathf.Atan2(localStylusPos.y, localStylusPos.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;

        float distance = Vector2.Distance(Vector2.zero, new Vector2(localStylusPos.x, localStylusPos.y));
        
        // Reset previous hover state
        if (hoveredSegment >= 0)
        {
            radialSegments[hoveredSegment].GetComponent<Image>().color = Color.gray;
        }

        // Check if stylus is within selection radius
        if (distance < selectionRadius)
        {
            int newHoveredSegment = (int)(angle / (360f / numberOfRadials));
            if (newHoveredSegment >= 0 && newHoveredSegment < numberOfRadials)
            {
                hoveredSegment = newHoveredSegment;
                radialSegments[hoveredSegment].GetComponent<Image>().color = Color.blue;
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
        }
        SetMenuActive(false);
    }

    private void SetMenuActive(bool active)
    {
        isMenuActive = active;
        radialParent.gameObject.SetActive(active);
        
        if (!active)
        {
            hoveredSegment = -1;
            // Reset all segments to default color
            foreach (var segment in radialSegments)
            {
                segment.GetComponent<Image>().color = Color.gray;
            }
        }
    }

    private void HandleSelection(int segmentIndex)
    {
        switch (segmentIndex)
        {
            case 0: // Left (Domain Box)
                Debug.Log("Selected Domain Box Creation Mode");
                break;
            case 1: // Top (Spatial AI)
                Debug.Log("Selected Spatial AI Mode");
                break;
            case 2: // Right (Sculpt)
                Debug.Log("Selected Sculpt Mode");
                break;
            case 3: // Bottom (Clear)
                Debug.Log("Selected Clear Scene");
                break;
        }
    }
}