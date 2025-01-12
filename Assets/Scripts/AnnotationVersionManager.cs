using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;


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
    [Header("References")]
    [SerializeField] private Transform stylusTip;
    [SerializeField] private Transform versionControlButton;
    [SerializeField] private GameObject versionListUI;
    [SerializeField] private GameObject lineRendererPrefab;
    
    [Header("Settings")]
    [SerializeField] private float interactionDistance = 0.05f;
    [SerializeField] private float buttonCooldown = 0.5f;
    
    [Header("Animation Settings")]
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float elementDelay = 0.1f;

    [Header("Annotation Groups")]
    [SerializeField] private List<AnnotationGroup> annotationGroups = new List<AnnotationGroup>();
    
    private Dictionary<string, AnnotationGroup> groupDictionary = new Dictionary<string, AnnotationGroup>();
    private bool isVersionListVisible = false;
    private float lastInteractionTime;

    private void Start()
    {
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
                // Ensure proper scale initialization
                group.annotationUIGroup.transform.localScale = Vector3.one;
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
       // StartCoroutine(AnimateVersionList());
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