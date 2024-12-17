using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandData : MonoBehaviour
{
    public enum HandModelType{left,right};

    public HandModelType handModelType;
    public Transform root;
    public Animator animator;
    public Transform[] fingerBones;

}
