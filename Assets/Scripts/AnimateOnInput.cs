using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AnimateOnInput : MonoBehaviour
{
    public InputActionProperty pinchInputAction;
    public InputActionProperty gripInputAction;
    public Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float triggerValue = pinchInputAction.action.ReadValue<float>();
        animator.SetFloat("Trigger", triggerValue);
        float gripValue = gripInputAction.action.ReadValue<float>();
        animator.SetFloat("Grip", gripValue);
    }
}
