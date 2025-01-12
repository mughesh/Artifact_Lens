using UnityEngine;
using System.Collections.Generic;
using System.Linq;


[System.Serializable]
public class UIAnimationSettings
{
    public float fadeInDuration = 0.3f;
    public float scaleInDuration = 0.3f;
    public float elementDelay = 0.1f;
    public Vector3 startScale = new Vector3(0.8f, 0.8f, 0.8f);
    public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
}


[System.Serializable]
public class AnnotationGroup
{
    public string researcherName;
    public GameObject anchorGroup;
    public GameObject annotationUIGroup;
    public Transform researcherUIButton; // Reference to the researcher's UI button in the list
    [HideInInspector]
    public List<GameObject> activeLineRenderers = new List<GameObject>();
    public bool isActive = false;
}

public class AnnotationVersionManager : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private UIAnimationSettings animSettings;
    
    [Header("Camera Reference")]
    [SerializeField] private Camera mainCamera;

    [Header("References")]
    [SerializeField] private Transform stylusTip;
    [SerializeField] private Transform versionControlButton;
    [SerializeField] private GameObject versionListUI;
    [SerializeField] private GameObject lineRendererPrefab;
    
    [Header("Settings")]
    [SerializeField] private float interactionDistance = 0.05f;
    [SerializeField] private float buttonCooldown = 0.5f;
    
    [Header("Annotation Groups")]
    [SerializeField] private List<AnnotationGroup> annotationGroups = new List<AnnotationGroup>();
    
    private Dictionary<string, AnnotationGroup> groupDictionary = new Dictionary<string, AnnotationGroup>();
    private bool isVersionListVisible = false;
    private float lastButtonPressTime;

    private void Start()
    {
        InitializeGroups();
        if (versionListUI != null)
        {
            versionListUI.SetActive(false);
            // Set initial scale of UI elements to zero
            foreach (var group in annotationGroups)
            {
                if (group.researcherUIButton != null)
                {
                    group.researcherUIButton.localScale = Vector3.zero;
                }
                if (group.annotationUIGroup != null)
                {
                    group.annotationUIGroup.transform.localScale = Vector3.zero;
                }
            }
        }
        
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    private void InitializeGroups()
    {
        foreach (var group in annotationGroups)
        {
            groupDictionary[group.researcherName] = group;
            
            // Ensure groups start hidden
            if (group.anchorGroup != null)
                group.anchorGroup.SetActive(false);
            if (group.annotationUIGroup != null)
                group.annotationUIGroup.SetActive(false);
        }
    }

    private void Update()
    {
        CheckStylusInteractions();
        if (AnyGroupActive())
        {
            UpdateLineRenderers();
        }
        UpdateUIRotations();
    }



    private void CheckStylusInteractions()
    {
        // Check version control button interaction
        if (versionControlButton != null && 
            Vector3.Distance(stylusTip.position, versionControlButton.position) < interactionDistance)
        {
            if (Time.time - lastButtonPressTime > buttonCooldown)
            {
                ToggleVersionList();
                lastButtonPressTime = Time.time;
            }
        }

        // Check researcher button interactions when list is visible
        if (isVersionListVisible)
        {
            foreach (var group in annotationGroups)
            {
                if (group.researcherUIButton != null && 
                    Vector3.Distance(stylusTip.position, group.researcherUIButton.position) < interactionDistance)
                {
                    Debug.Log("Interacted with " + group.researcherName);
                    if (Time.time - lastButtonPressTime > buttonCooldown)
                    {
                        ToggleAnnotationGroup(group.researcherName);
                        lastButtonPressTime = Time.time;
                    }
                }
            }
        }
    }

    private void ToggleVersionList()
    {
        isVersionListVisible = !isVersionListVisible;
        versionListUI.SetActive(isVersionListVisible);

        if (isVersionListVisible)
        {
            StartCoroutine(AnimateVersionList());
        }
        else
        {
            // Disable all groups
            foreach (var group in annotationGroups)
            {
                if (group.isActive)
                {
                    DisableAnnotationGroup(group);
                }
            }
        }
    }

        private void UpdateUIRotations()
    {
        if (mainCamera == null) return;

        // Rotate version list UI
        if (versionListUI != null && versionListUI.activeSelf)
        {
            RotateTowardCamera(versionListUI.transform);
        }

        // Rotate active annotation UIs
        foreach (var group in annotationGroups)
        {
            if (group.isActive && group.annotationUIGroup != null)
            {
                RotateTowardCamera(group.annotationUIGroup.transform);
            }
        }
    }

        private void RotateTowardCamera(Transform target)
    {
        Vector3 directionToCamera = mainCamera.transform.position - target.position;
        directionToCamera.y = 0; // Keep vertical orientation
        
        if (directionToCamera != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(-directionToCamera);
            target.rotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0);
        }
    }

    private System.Collections.IEnumerator AnimateVersionList()
    {
        // Get all UI elements to animate
        var uiElements = versionListUI.GetComponentsInChildren<CanvasGroup>(true)
            .Where(cg => cg.transform != versionListUI.transform)
            .ToArray();

        // Set initial states
        foreach (var element in uiElements)
        {
            element.alpha = 0f;
            element.transform.localScale = animSettings.startScale;
        }

        // Animate each element
        foreach (var element in uiElements)
        {
            StartCoroutine(AnimateUIElement(element));
            yield return new WaitForSeconds(animSettings.elementDelay);
        }
    }

    private System.Collections.IEnumerator AnimateUIElement(CanvasGroup element)
    {
        float elapsed = 0f;
        Vector3 originalScale = Vector3.one;

        while (elapsed < animSettings.fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / animSettings.fadeInDuration;

            // Animate fade
            element.alpha = animSettings.fadeCurve.Evaluate(normalizedTime);

            // Animate scale
            float scaleProgress = animSettings.scaleCurve.Evaluate(normalizedTime);
            element.transform.localScale = Vector3.Lerp(animSettings.startScale, originalScale, scaleProgress);

            yield return null;
        }

        // Ensure final state
        element.alpha = 1f;
        element.transform.localScale = originalScale;
    }

    private void ToggleAnnotationGroup(string researcherName)
    {
        if (groupDictionary.TryGetValue(researcherName, out AnnotationGroup group))
        {
            // Disable other groups first
            foreach (var otherGroup in annotationGroups)
            {
                if (otherGroup != group && otherGroup.isActive)
                {
                    DisableAnnotationGroup(otherGroup);
                }
            }

            // Toggle selected group
            group.isActive = !group.isActive;
            if (group.isActive)
            {
                EnableAnnotationGroup(group);
            }
            else
            {
                DisableAnnotationGroup(group);
            }
        }
    }

    private void EnableAnnotationGroup(AnnotationGroup group)
    {
        group.anchorGroup.SetActive(true);
        group.annotationUIGroup.SetActive(true);
        StartCoroutine(AnimateAnnotationGroup(group));
        CreateLineRenderers(group);
    }

    private void DisableAnnotationGroup(AnnotationGroup group)
    {
        group.anchorGroup.SetActive(false);
        group.annotationUIGroup.SetActive(false);
        
        foreach (var lineRenderer in group.activeLineRenderers)
        {
            Destroy(lineRenderer);
        }
        group.activeLineRenderers.Clear();
        group.isActive = false;
    }

    private void CreateLineRenderers(AnnotationGroup group)
    {
        Transform[] anchorPoints = group.anchorGroup.GetComponentsInChildren<Transform>()
            .Where(t => t.name.Contains("AnchorPoint"))
            .ToArray();
            
        Transform[] lineRenderPoints = group.annotationUIGroup.GetComponentsInChildren<Transform>()
            .Where(t => t.name.Contains("LineRenderPoint"))
            .ToArray();

        int pairCount = Mathf.Min(anchorPoints.Length, lineRenderPoints.Length);

        for (int i = 0; i < pairCount; i++)
        {
            GameObject lineObj = Instantiate(lineRendererPrefab);
            LineRenderer line = lineObj.GetComponent<LineRenderer>();
            
            line.positionCount = 2;
            line.SetPosition(0, anchorPoints[i].position);
            line.SetPosition(1, lineRenderPoints[i].position);
            
            group.activeLineRenderers.Add(lineObj);
        }
    }

    private void UpdateLineRenderers()
    {
        foreach (var group in annotationGroups.Where(g => g.isActive))
        {
            Transform[] anchorPoints = group.anchorGroup.GetComponentsInChildren<Transform>()
                .Where(t => t.name.Contains("AnchorPoint"))
                .ToArray();
                
            Transform[] lineRenderPoints = group.annotationUIGroup.GetComponentsInChildren<Transform>()
                .Where(t => t.name.Contains("LineRenderPoint"))
                .ToArray();

            for (int i = 0; i < group.activeLineRenderers.Count; i++)
            {
                if (i < anchorPoints.Length && i < lineRenderPoints.Length)
                {
                    LineRenderer line = group.activeLineRenderers[i].GetComponent<LineRenderer>();
                    line.SetPosition(0, anchorPoints[i].position);
                    line.SetPosition(1, lineRenderPoints[i].position);
                }
            }
        }
    }

    private System.Collections.IEnumerator AnimateAnnotationGroup(AnnotationGroup group)
    {
        var uiElements = group.annotationUIGroup.GetComponentsInChildren<CanvasGroup>(true);
        
        foreach (var element in uiElements)
        {
            element.alpha = 0f;
            element.transform.localScale = animSettings.startScale;
            
            StartCoroutine(AnimateUIElement(element));
            yield return new WaitForSeconds(animSettings.elementDelay);
        }
    }

    private bool AnyGroupActive()
    {
        return annotationGroups.Any(g => g.isActive);
    }
}