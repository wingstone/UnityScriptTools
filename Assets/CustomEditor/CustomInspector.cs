using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomInspector : MonoBehaviour
{
    [SerializeField]
    Vector3 lookAtPoint;

    [SerializeField]
    [ColorUsage(true, true)]
    public Color color = Color.white;

    [SerializeField]
    public float factor = 100;
    [SerializeField]
    public float factorPower = 2;
}
