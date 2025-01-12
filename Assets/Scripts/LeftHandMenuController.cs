using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using System;

[System.Serializable]
public class MenuOption
{
    public string name;
    public Image buttonBackground;
    public TextMeshProUGUI text;
    public Action onSelect;  // Callback for when option is selected
}

public class LeftHandMenuController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AnnotationVersionManager versionManager;
    [SerializeField] private Transform menuAnchor;
    [SerializeField] private Transform stylusTip;
    [SerializeField] private GameObject menuRoot;
    [SerializeField] private InputActionProperty menuButton;
    
    [Header("Menu Options")]
    [SerializeField] private MenuOption[] menuOptions;
    
    [Header("Visual Settings")]
    [SerializeField] private Color defaultColor = Color.gray;
    [SerializeField] private Color hoverColor = Color.blue;
    [SerializeField] private float interactionDistance = 0.05f;
    
    private Camera mainCamera;
    private bool isMenuActive = false;
    private int hoveredIndex = -1;
    private bool shouldTriggerSelection = false;

    private void Start()
    {
        mainCamera = Camera.main;
        menuRoot.SetActive(false);

        // Initialize menu options
        foreach (var option in menuOptions)
        {
            if (option.buttonBackground != null)
            {
                option.buttonBackground.color = defaultColor;
            }
            if (option.text != null)
            {
                option.text.gameObject.SetActive(false);
            }
        }
    }

    private void OnEnable()
    {
        menuButton.action.Enable();
        menuButton.action.performed += OnMenuButtonPressed;
        menuButton.action.canceled += OnMenuButtonReleased;
    }

    private void OnDisable()
    {
        menuButton.action.Disable();
        menuButton.action.performed -= OnMenuButtonPressed;
        menuButton.action.canceled -= OnMenuButtonReleased;
    }

    private void Update()
    {
        if (isMenuActive)
        {
            UpdateMenuPosition();
            CheckHoverStates();
        }
    }

    private void UpdateMenuPosition()
    {
        if (menuAnchor != null && menuRoot != null)
        {
            menuRoot.transform.position = menuAnchor.position;
            
            // Make menu face the user (camera)
            Vector3 directionToCamera = mainCamera.transform.position - menuRoot.transform.position;
            directionToCamera.y = 0; // Keep vertical orientation
            if (directionToCamera != Vector3.zero)
            {
                menuRoot.transform.rotation = Quaternion.LookRotation(-directionToCamera);
            }
        }
    }

    private void CheckHoverStates()
    {
        // Reset previous hover state
        if (hoveredIndex >= 0 && hoveredIndex < menuOptions.Length)
        {
            menuOptions[hoveredIndex].buttonBackground.color = defaultColor;
            menuOptions[hoveredIndex].text.gameObject.SetActive(false);
        }

        hoveredIndex = -1;

        // Check each menu option
        for (int i = 0; i < menuOptions.Length; i++)
        {
            if (menuOptions[i].buttonBackground != null)
            {
                Vector3 buttonPosition = menuOptions[i].buttonBackground.transform.position;
                float distance = Vector3.Distance(stylusTip.position, buttonPosition);

                if (distance < interactionDistance)
                {
                    hoveredIndex = i;
                    menuOptions[i].buttonBackground.color = hoverColor;
                    menuOptions[i].text.gameObject.SetActive(true);

                    // Trigger callback if assigned
                    if (menuOptions[i].onSelect != null)
                    {
                        menuOptions[i].onSelect.Invoke();
                    }
                    break;
                }
            }
        }
    }

    private void OnMenuButtonPressed(InputAction.CallbackContext context)
    {
        ShowMenu();
    }

    private void OnMenuButtonReleased(InputAction.CallbackContext context)
    {
        shouldTriggerSelection = true;
        HideMenu();
    }

    private void ShowMenu()
    {
        isMenuActive = true;
        menuRoot.SetActive(true);
        UpdateMenuPosition();
    }

     private void HideMenu()
    {
        if (shouldTriggerSelection && hoveredIndex >= 0)
        {
            HandleMenuSelection(hoveredIndex);
        }
        
        isMenuActive = false;
        menuRoot.SetActive(false);
        
        // Reset states
        if (hoveredIndex >= 0 && hoveredIndex < menuOptions.Length)
        {
            menuOptions[hoveredIndex].buttonBackground.color = defaultColor;
            menuOptions[hoveredIndex].text.gameObject.SetActive(false);
        }
        hoveredIndex = -1;
        shouldTriggerSelection = false;
    }

    private void HandleMenuSelection(int index)
    {
        if (index >= 0 && index < menuOptions.Length)
        {
            string optionName = menuOptions[index].name;
            
            switch (optionName)
            {
                case "VersionHistory":
                    if (versionManager != null)
                    {
                        versionManager.ToggleVersionList();
                    }
                    break;
                case "AI":
                    // Handle AI menu selection
                    break;
                case "Collaborate":
                    // Handle collaborate menu selection
                    break;
            }
        }
    }

    // Public method to assign callbacks
    public void SetMenuOptionCallback(string optionName, Action callback)
    {
        var option = Array.Find(menuOptions, x => x.name == optionName);
        if (option != null)
        {
            option.onSelect = callback;
        }
    }
}