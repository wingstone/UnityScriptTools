using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyFrameInterDemo : MonoBehaviour
{
    public AnimationCurve Curve;
    public float factor;

    void Start()
    {
        Keyframe[] keyframes = new Keyframe[2];
        keyframes[0] = new Keyframe(0, 0, 0, 0);
        keyframes[1] = new Keyframe(1, 1, 0, 0);
        Curve.keys = keyframes;
    }

    void Update()
    {
        float floor = Mathf.Floor(Time.time);
        factor = Time.time - floor;
        if (floor % 2 == 0)
            factor = 1 - factor;
        Keyframe keyframe0 = new Keyframe(0, 0, 0, 0);
        Keyframe keyframe1 = new Keyframe(1, 1, 0, 0);
        float y = KeyFrameInter.Evaluate(factor, keyframe0, keyframe1);
        Vector3 pos = transform.position;
        transform.position = new Vector3(pos.x, y, pos.z);
    }
}
