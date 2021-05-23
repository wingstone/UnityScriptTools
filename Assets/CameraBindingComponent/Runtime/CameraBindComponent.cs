using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Camera))]
public class CameraBindComponent : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(Camera))]
class DisplayCameraEditor : CameraEditor
{
    CameraBindComponent bindComponent = null;
    public Camera camera { get { return target as Camera; } }
    public new void OnEnable()
    {
        // Additional Camera Data
        bindComponent = camera.gameObject.GetComponent<CameraBindComponent>();
        if (bindComponent == null)
        {
            bindComponent = camera.gameObject.AddComponent<CameraBindComponent>();
        }
    }
    public override void OnInspectorGUI()
    {
        // settings.Update();
        // settings.ApplyModifiedProperties();
        base.OnInspectorGUI();
    }
}
#endif