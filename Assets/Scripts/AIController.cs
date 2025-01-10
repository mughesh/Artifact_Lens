using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.VFX;

public class AIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject aiVFXPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private ParticleSystem[] vfxSystems;
    
    [Header("Input")]
    [SerializeField] private InputActionProperty buttonXAction;
    [SerializeField] private InputActionProperty buttonYAction;
    
    [Header("VFX Settings")]
    [SerializeField] private VisualEffect aiVFXGraph;

        [Header("Color Properties")]
    [ColorUsage(true, true)] // Enables HDR color picker
    [SerializeField] private Color defaultColor = new Color(0.5f, 1f, 1f, 1f);
    [ColorUsage(true, true)]
    [SerializeField] private Color activeColor = new Color(1f, 0.5f, 1f, 1f);
    [ColorUsage(true, true)]
    [SerializeField] private Color defaultTrailColor = new Color(1f, 0.5f, 0f, 1f);
    [ColorUsage(true, true)]
    [SerializeField] private Color activeTrailColor = new Color(1f, 0f, 0.5f, 1f);
    [ColorUsage(true, true)]
    [SerializeField] private Color defaultBeamColor = new Color(0f, 1f, 1f, 1f);
    [ColorUsage(true, true)]
    [SerializeField] private Color activeBeamColor = new Color(1f, 0f, 1f, 1f);
    
    private int currentSpawnIndex = -1;
    private bool isAIActive = false;
    private bool isListening = false;
    
    private void OnEnable()
    {
        buttonXAction.action.Enable();
        buttonYAction.action.Enable();
        
        buttonXAction.action.performed += OnButtonXPressed;
        buttonYAction.action.performed += OnButtonYPressed;
        buttonYAction.action.canceled += OnButtonYReleased;
    }
    
    private void OnDisable()
    {
        buttonXAction.action.Disable();
        buttonYAction.action.Disable();
        
        buttonXAction.action.performed -= OnButtonXPressed;
        buttonYAction.action.performed -= OnButtonYPressed;
        buttonYAction.action.canceled -= OnButtonYReleased;
    }
    
    private void OnButtonXPressed(InputAction.CallbackContext context)
    {
        Debug.Log("Button X Pressed");
        if (!isAIActive)
        {
            // First activation
            currentSpawnIndex = 0;
            SpawnAI();
        }
        else if (currentSpawnIndex < spawnPoints.Length - 1)
        {
            // Move to next position
            currentSpawnIndex++;
            MoveAIToCurrentPosition();
        }
        else
        {
            // Deactivate
            DeactivateAI();
        }
    }
    
    private void SpawnAI()
    {
        if (aiVFXPrefab != null && currentSpawnIndex >= 0 && currentSpawnIndex < spawnPoints.Length)
        {
            aiVFXPrefab.SetActive(true);
            aiVFXPrefab.transform.position = spawnPoints[currentSpawnIndex].position;
            isAIActive = true;
            UpdateVFXColor(isListening);
        }
    }
    
    private void MoveAIToCurrentPosition()
    {
        if (isAIActive && currentSpawnIndex < spawnPoints.Length)
        {
            StartCoroutine(MoveAISmooth(spawnPoints[currentSpawnIndex].position));
        }
    }
    
    private IEnumerator MoveAISmooth(Vector3 targetPosition)
    {
        float duration = 0.5f;
        float elapsed = 0f;
        Vector3 startPosition = aiVFXPrefab.transform.position;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            aiVFXPrefab.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }
        
        aiVFXPrefab.transform.position = targetPosition;
    }
    
    private void DeactivateAI()
    {
        if (aiVFXPrefab != null)
        {
            aiVFXPrefab.SetActive(false);
            isAIActive = false;
            currentSpawnIndex = -1;
        }
    }
    
    private void OnButtonYPressed(InputAction.CallbackContext context)
    {
        Debug.Log("Button Y Pressed");
        if (isAIActive)
        {
            isListening = true;
            UpdateVFXColor(isListening);
        }
    }
    
    private void OnButtonYReleased(InputAction.CallbackContext context)
    {
        if (isAIActive)
        {
            isListening = false;
            UpdateVFXColor(isListening);
        }
    }
    
    private void UpdateVFXColor(bool isActive)
    {
        if (aiVFXGraph != null)
        {
            // Update main color
            aiVFXGraph.SetVector4("Color", isActive ? activeColor : defaultColor);
            
            // Update trail color
            aiVFXGraph.SetVector4("TrailColor", isActive ? activeTrailColor : defaultTrailColor);
            
            // Update beam color
            aiVFXGraph.SetVector4("BeamColor", isActive ? activeBeamColor : defaultBeamColor);
        }
    }
}