using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

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
    [SerializeField] private Color defaultColor = Color.cyan;
    [SerializeField] private Color activeColor = Color.magenta;
    [SerializeField] private float colorTransitionSpeed = 5f;
    
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
            UpdateVFXColor(defaultColor);
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
            UpdateVFXColor(activeColor);
        }
    }
    
    private void OnButtonYReleased(InputAction.CallbackContext context)
    {
        if (isAIActive)
        {
            isListening = false;
            UpdateVFXColor(defaultColor);
        }
    }
    
    private void UpdateVFXColor(Color targetColor)
    {
        if (vfxSystems != null)
        {
            foreach (var vfx in vfxSystems)
            {
                var main = vfx.main;
                main.startColor = targetColor;
            }
        }
    }
}