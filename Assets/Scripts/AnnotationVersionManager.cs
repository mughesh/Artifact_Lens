using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using UnityEngine.UI;


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
    public Image buttonBackground; 
    [HideInInspector]
    public List<GameObject> activeLineRenderers = new List<GameObject>();
    public bool isActive = false;
}



public class AnnotationVersionManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform stylusTip;
   // [SerializeField] private Transform versionControlButton;
    [SerializeField] private GameObject versionListUI;
    [SerializeField] private GameObject lineRendererPrefab;
    
    [Header("Settings")]
    [SerializeField] private float interactionDistance = 0.05f;
    [SerializeField] private float buttonCooldown = 0.5f;
    
    [Header("Animation Settings")]
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float elementDelay = 0.1f;

        [Header("Positioning")]
    [SerializeField] private Transform spawnPoint; 
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Vector3 listOffset = new Vector3(0.5f, 0f, 0f);  // Offset from rig
    [SerializeField] private bool updatePositionEveryFrame = false;  // Option to continuously update position
    [SerializeField] private Vector3 offset = Vector3.zero;

    [Header("List Positioning Settings")]
    [SerializeField] private float distanceFromRig = 1f;
    [SerializeField] private float heightOffset = 0f;
    [SerializeField] private float rotationSmoothSpeed = 5f;

    [Header("Annotation Groups")]
    [SerializeField] private List<AnnotationGroup> annotationGroups = new List<AnnotationGroup>();
    
    [Header("UI Colors")]
    [SerializeField] private Color defaultButtonColor = Color.gray;
    [SerializeField] private Color selectedButtonColor = Color.blue;

    private Dictionary<string, AnnotationGroup> groupDictionary = new Dictionary<string, AnnotationGroup>();
    private bool isVersionListVisible = false;
    private float lastInteractionTime;

    private void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        InitializeGroups();
        if (versionListUI != null)
        {
            versionListUI.SetActive(false);
        }
    }

    private void InitializeGroups()
    {
        foreach (var group in annotationGroups)
        {
            groupDictionary[group.researcherName] = group;
            
            if (group.anchorGroup != null)
            {
                group.anchorGroup.SetActive(false);
            }
            if (group.annotationUIGroup != null)
            {
                group.annotationUIGroup.SetActive(false);
                group.annotationUIGroup.transform.localScale = Vector3.one;
            }
            if (group.buttonBackground != null)
            {
                group.buttonBackground.color = defaultButtonColor;
            }
        }
    }

    public void ToggleVersionList()
    {
        if (Time.time - lastInteractionTime < buttonCooldown) return;
        
        isVersionListVisible = !isVersionListVisible;
        lastInteractionTime = Time.time;

        if (isVersionListVisible)
        {
            ShowVersionList();
        }
        else
        {
            HideVersionList();
        }
    }

    private void ShowVersionList()
    {
        versionListUI.SetActive(true);
        PositionVersionList();
        //StartCoroutine(AnimateVersionList());
    }


   private void PositionVersionList()
    {
        if (spawnPoint == null || versionListUI == null) return;

        // Set position to spawn point plus any offset
        versionListUI.transform.position = spawnPoint.position + offset;
        
        // Initial facing towards camera
        UpdateUIFacing(versionListUI.transform);
    }


    private void HideVersionList()
    {
        StopAllCoroutines();
        versionListUI.SetActive(false);
        
        foreach (var group in annotationGroups)
        {
            if (group.isActive)
            {
                DisableAnnotationGroup(group);
            }
        }
    }

    private void Update()
    {
        if (isVersionListVisible)
        {
            CheckResearcherSelection();
            UpdateUIPositions();
        }
    }

    private void UpdateUIPositions()
    {
        // Update version list position to follow spawn point
        if (versionListUI != null && versionListUI.activeSelf && spawnPoint != null)
        {
            versionListUI.transform.position = spawnPoint.position + offset;
            UpdateUIFacing(versionListUI.transform);
        }

        // Update active annotation UIs facing
        foreach (var group in annotationGroups)
        {
            if (group.isActive && group.annotationUIGroup != null)
            {
                UpdateUIFacing(group.annotationUIGroup.transform);
            }
        }
    }

    private void UpdateUIFacing(Transform uiTransform)
    {
        if (mainCamera == null) return;

        Vector3 directionToCamera = mainCamera.transform.position - uiTransform.position;
        directionToCamera.y = 0; // Keep vertical orientation

        if (directionToCamera != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(-directionToCamera);
            uiTransform.rotation = Quaternion.Slerp(
                uiTransform.rotation,
                Quaternion.Euler(0, targetRotation.eulerAngles.y, 0),
                rotationSmoothSpeed * Time.deltaTime
            );
        }
    }

    private void CheckResearcherSelection()
    {
        if (Time.time - lastInteractionTime < buttonCooldown) return;

        foreach (var group in annotationGroups)
        {
            if (group.researcherUIButton != null)
            {
                float distance = Vector3.Distance(stylusTip.position, group.researcherUIButton.position);
                if (distance < interactionDistance)
                {
                    lastInteractionTime = Time.time;
                    ToggleAnnotationGroup(group.researcherName);
                    break;
                }
            }
        }
    }

    private void ToggleAnnotationGroup(string researcherName)
    {
        if (!groupDictionary.TryGetValue(researcherName, out AnnotationGroup group)) return;

        // Reset colors and disable other groups
        foreach (var otherGroup in annotationGroups)
        {
            if (otherGroup != group && otherGroup.isActive)
            {
                DisableAnnotationGroup(otherGroup);
            }
            // Reset color of other groups
            if (otherGroup.buttonBackground != null)
            {
                otherGroup.buttonBackground.color = defaultButtonColor;
            }
        }

        // Toggle selected group
        group.isActive = !group.isActive;
        if (group.isActive)
        {
            EnableAnnotationGroup(group);
            if (group.buttonBackground != null)
            {
                group.buttonBackground.color = selectedButtonColor;
            }
        }
        else
        {
            DisableAnnotationGroup(group);
            if (group.buttonBackground != null)
            {
                group.buttonBackground.color = defaultButtonColor;
            }
        }
    }

    private void EnableAnnotationGroup(AnnotationGroup group)
    {
        if (group.anchorGroup != null)
        {
            group.anchorGroup.SetActive(true);
        }

        if (group.annotationUIGroup != null)
        {
            group.annotationUIGroup.SetActive(true);
            group.annotationUIGroup.transform.localScale = Vector3.one;
            UpdateUIFacing(group.annotationUIGroup.transform);
            StartCoroutine(CreateLineRenderersWithDelay(group));
        }
    }
    private void DisableAnnotationGroup(AnnotationGroup group)
    {
        if (group.anchorGroup != null)
        {
            group.anchorGroup.SetActive(false);
        }
        if (group.annotationUIGroup != null)
        {
            group.annotationUIGroup.SetActive(false);
        }
        if (group.buttonBackground != null)
        {
            group.buttonBackground.color = defaultButtonColor;
        }

        foreach (var lineRenderer in group.activeLineRenderers)
        {
            if (lineRenderer != null)
            {
                Destroy(lineRenderer);
            }
        }
        group.activeLineRenderers.Clear();
        group.isActive = false;
    }

    private IEnumerator CreateLineRenderersWithDelay(AnnotationGroup group)
    {
        if (group.anchorGroup == null || group.annotationUIGroup == null) yield break;

        var anchorPoints = group.anchorGroup.GetComponentsInChildren<Transform>()
            .Where(t => t.name.Contains("AnchorPoint"))
            .ToArray();

        var lineRenderPoints = group.annotationUIGroup.GetComponentsInChildren<Transform>()
            .Where(t => t.name.Contains("LineRenderPoint"))
            .ToArray();

        int pairCount = Mathf.Min(anchorPoints.Length, lineRenderPoints.Length);

        for (int i = 0; i < pairCount; i++)
        {
            yield return new WaitForSeconds(elementDelay);

            GameObject lineObj = Instantiate(lineRendererPrefab);
            LineRenderer line = lineObj.GetComponent<LineRenderer>();
            
            line.positionCount = 2;
            line.SetPosition(0, anchorPoints[i].position);
            line.SetPosition(1, lineRenderPoints[i].position);
            
            group.activeLineRenderers.Add(lineObj);

            // Update line positions
            StartCoroutine(UpdateLineRenderer(line, anchorPoints[i], lineRenderPoints[i]));
        }
    }

    private IEnumerator UpdateLineRenderer(LineRenderer line, Transform start, Transform end)
    {
        while (line != null && start != null && end != null)
        {
            line.SetPosition(0, start.position);
            line.SetPosition(1, end.position);
            yield return null;
        }
    }
}