using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CustomInspector))]
public class CustomInspectorEditor : Editor
{
    SerializedProperty lookAtPoint;
    SerializedProperty color;
    SerializedProperty factor;
    private void OnEnable()
    {
        lookAtPoint = serializedObject.FindProperty("lookAtPoint");
        color = serializedObject.FindProperty("color");
        factor = serializedObject.FindProperty("factor");
    }

    public override void OnInspectorGUI()
    {
        // base.OnInspectorGUI();

        serializedObject.Update();

        EditorGUILayout.PropertyField(lookAtPoint);
        EditorGUILayout.PropertyField(color);
        EditorGUILayout.PropertyField(factor);

        factor.floatValue = PowerSlider("Power Slider:", 0.1f, 1000f, 10f, factor.floatValue);

        serializedObject.ApplyModifiedProperties();

    }

    // pow must > 0
    float PowerSlider(string label, float min, float max, float pow, float value)
    {
        float powmin = Mathf.Log(min, pow);
        float powmax = Mathf.Log(max, pow);
        float powValue = Mathf.Log(value, pow);
        powValue = EditorGUILayout.Slider(label, powValue, powmin, powmax);
        return Mathf.Pow(pow, powValue);
    }
}