using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR.Interaction.Toolkit;

public class GrabHandPose : MonoBehaviour
{
    public HandData rightHandPose;
    // Start is called before the first frame update
    void Start()
    {
        XRGrabInteractable grabInteractable = GetComponent<XRGrabInteractable>();
        grabInteractable.selectExited.AddListener(SetupPose);
        Debug.Log("Grab Hand Pose Activated");   
        rightHandPose.gameObject.SetActive(false);

    }

    // Update is called once per frame

    public void SetupPose(BaseInteractionEventArgs args)
    {

        if (args.interactorObject is XRDirectInteractor)
        {
            Debug.Log("If block");
            HandData handData = args.interactorObject.transform.GetComponent<HandData>();
            //Debug.Log(handData.animator);
            handData.animator.enabled = false;

        }
        else
        {
            Debug.Log("Else block");
        }

    }
}
