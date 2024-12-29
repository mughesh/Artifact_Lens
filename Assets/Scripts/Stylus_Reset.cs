using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Stylus_Reset : MonoBehaviour
{
    public float resetDistance = 1.0f;
    public XRSocketInteractor SocketInteractor;
    private XRGrabInteractable grabInteractable;
    private bool isGrabbed = false;
    // Start is called before the first frame update
    void Start()
    {
        transform.position = SocketInteractor.transform.position;
        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = SocketInteractor.transform.position;
        if (!isGrabbed)
        {
            ResetStylus();
        }

    }

    void OnGrab(SelectEnterEventArgs args)
    {
        isGrabbed = true;
    }

    void OnRelease(SelectExitEventArgs args)
    {
        isGrabbed = false;
    }

    public void ResetStylus()
    {
        Vector3 distance = transform.position - SocketInteractor.transform.position;
        if (distance.magnitude > resetDistance)
        {
            transform.position = SocketInteractor.transform.position;
        }
    }
}
