using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public float radius = 4;
    public Vector3 p1 = Vector3.forward;
    public Vector3 p2 = Vector3.right;
    public Vector3 p3 = Vector3.zero;
    public int num = 50;
    public GameObject go;
    void Start()
    {
        Vector3[] points = null;
        MathematicsHelper.UniformSampleInTriangle(num, p1, p2, p3, out points);

        for (int i = 0; i < num; i++)
        {
            GameObject newGo = GameObject.Instantiate(go, points[i], Quaternion.identity, transform);
            newGo.transform.localScale = Vector3.one * 0.1f;
        }
    }
}
