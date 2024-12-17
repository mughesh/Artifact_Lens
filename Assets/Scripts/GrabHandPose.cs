using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR.Interaction.Toolkit;

public class GrabHandPose : MonoBehaviour
{
    public HandData rightHandPose;
    private Vector3 startingHandPosition;
    private Vector3 finalHandPosition;
    private Quaternion startingHandRotation;
    private Quaternion finalHandRotation;
    private Quaternion[] startingFingerRotations;
    private Quaternion[] finalFingerRotations;


    // Start is called before the first frame update
    void Start()
    {
        XRGrabInteractable grabInteractable = GetComponent<XRGrabInteractable>();
        grabInteractable.selectExited.AddListener(SetupPose);

        rightHandPose.gameObject.SetActive(false);

    }

    // Update is called once per frame

    public void SetupPose(BaseInteractionEventArgs args)
    {

        if (args.interactorObject is XRDirectInteractor)
        {

            HandData handData = args.interactorObject.transform.GetComponentInChildren<HandData>();
            //Debug.Log(handData.animator);
            handData.animator.enabled = false;
            SetHandDataValues(handData, rightHandPose);
            SetHandData(handData, finalHandPosition,finalHandRotation,finalFingerRotations);
        }

    }

    public void SetHandDataValues(HandData h1, HandData h2)
    {
        startingHandPosition = h1.root.transform.position;
        finalHandPosition = h2.root.transform.position;

        startingHandRotation = h1.root.transform.rotation;
        finalHandRotation = h2.root.transform.rotation;

        startingFingerRotations = new Quaternion[h1.fingerBones.Length];
        finalFingerRotations = new Quaternion[h2.fingerBones.Length];

        for(int i = 0; i < h1.fingerBones.Length; i++)
        {
            startingFingerRotations[i] = h1.fingerBones[i].localRotation;
            finalFingerRotations[i] = h2.fingerBones[i].localRotation;
        }
    }

    public void SetHandData(HandData h, Vector3 newPosition, Quaternion newRotation, Quaternion[] newBoneRotations)
    {
        h.root.localPosition = newPosition;
        h.root.localRotation = newRotation;
        for(int i = 0; i < newBoneRotations.Length; i++)
        {
            h.fingerBones[i].localRotation = newBoneRotations[i];
        }
    }

}
