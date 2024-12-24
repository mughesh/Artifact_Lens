using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RadialSegment : MonoBehaviour
{
    private Material material;
    private int segmentIndex;

    public void Initialize(float startAngle, float angleSize, int index)
    {
        segmentIndex = index;
        
        // Setup transform
        transform.localRotation = Quaternion.Euler(0, 0, startAngle);
        
        // Setup material
        material = new Material(Shader.Find("UI/RadialMenuSegment"));
        GetComponent<Image>().material = material;
        GetComponent<Image>().fillAmount = angleSize / 360f;
        
        SetHighlight(false);
    }

    public void SetHighlight(bool highlighted)
    {
        if (material != null)
        {
            material.SetFloat("_IsHighlighted", highlighted ? 1f : 0f);
        }
    }
}
