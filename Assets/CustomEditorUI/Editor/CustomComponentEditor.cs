using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine.Events;

public enum EnumTest
{
    Enum0,
    Enum1,
    Enum2,
    Enum3,
}

// https://docs.unity3d.com/ScriptReference/EditorGUILayout.html
[CustomEditor(typeof(CustomComponent))]
public class CustomComponentEditor : Editor
{
    SerializedProperty pointProperty;

    float tmp1 = 0;
    int tmp2 = 0;
    bool tmp3 = false;
    Color tmp4 = Color.white;

    string tmp5 = "";
    AnimBool m_ShowExtraFields;

    bool toggleGroup = false;
    string m_String = "";
    Color m_Color = Color.white;
    int m_Number = 0;

    Vector2 scrollPos = Vector2.zero;
    TextAsset textAsset = null;
    string text = "";

    bool foldout = false;
    AnimationCurve curveX = AnimationCurve.Linear(0, 0, 10, 10);
    Gradient gradient = new Gradient();

    bool fold = false;
    Vector3 position = Vector3.zero;

    static string tagStr = "";

    EnumTest enmPar = EnumTest.Enum0;
    string[] options = new string[] { "Cube", "Sphere", "Plane" };
    int index = 0;

    int newIndex = 0;

    private void OnEnable()
    {
        pointProperty = serializedObject.FindProperty("point");

        m_ShowExtraFields = new AnimBool(true);
        m_ShowExtraFields.valueChanged.AddListener(new UnityAction(base.Repaint));
    }

    public override void OnInspectorGUI()
    {
        // base.OnInspectorGUI();

        serializedObject.Update();
        EditorGUILayout.PropertyField(pointProperty);
        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.BeginVertical();

        EditorGUILayout.LabelField("label");
        tmp1 = EditorGUILayout.Slider("slider", tmp1, 0, 1);
        tmp2 = EditorGUILayout.IntSlider("int slider", tmp2, 0, 10);
        tmp3 = EditorGUILayout.Toggle("toggle", tmp3);
        tmp4 = EditorGUILayout.ColorField("color", tmp4);

        using (var posGroup = new EditorGUILayout.ToggleGroupScope("Align position", toggleGroup))
        {
            toggleGroup = posGroup.enabled;
            tmp5 = EditorGUILayout.TextField("text", tmp5);
            if (GUILayout.Button("button"))
            {
                Debug.Log("click button");
            }
        }

        m_ShowExtraFields.target = EditorGUILayout.ToggleLeft("Show extra fields", m_ShowExtraFields.target);
        using (var group = new EditorGUILayout.FadeGroupScope(m_ShowExtraFields.faded))
        {
            if (group.visible)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PrefixLabel("Color");
                m_Color = EditorGUILayout.ColorField(m_Color);
                EditorGUILayout.PrefixLabel("Text");
                m_String = EditorGUILayout.TextField(m_String);
                EditorGUILayout.PrefixLabel("Number");
                m_Number = EditorGUILayout.IntSlider(m_Number, 0, 10);
                EditorGUI.indentLevel--;
            }
        }


        textAsset = EditorGUILayout.ObjectField("text field", textAsset, typeof(TextAsset), true) as TextAsset;
        using (var h = new EditorGUILayout.HorizontalScope())
        {
            using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPos, GUILayout.Width(100), GUILayout.Height(100)))
            {
                scrollPos = scrollView.scrollPosition;
                text = EditorGUILayout.TextArea(text);
            }
            if (GUILayout.Button("Read Text", GUILayout.Width(100), GUILayout.Height(100)))
                text = textAsset ? textAsset.text : "Read Text Completed";
            if (GUILayout.Button("Clear", GUILayout.Width(100), GUILayout.Height(100)))
                text = "";
        }

        foldout = EditorGUILayout.Foldout(foldout, "Fold Out");
        if (foldout)
        {
            EditorGUILayout.HelpBox("message", MessageType.Info);
            curveX = EditorGUILayout.CurveField("curve X", curveX);
            gradient = EditorGUILayout.GradientField("gradient", gradient);
        }

        // Make an inspector-window-like titlebar.
        fold = EditorGUILayout.InspectorTitlebar(fold, target);
        if (fold)
        {
            EditorGUILayout.Space();
            position = EditorGUILayout.Vector3Field("Position", position);
        }

        tagStr = EditorGUILayout.TagField("Tag for Objects:", tagStr);

        enmPar = (EnumTest)EditorGUILayout.EnumFlagsField("Enum test", enmPar);
        index = EditorGUILayout.Popup(index, options);

        newIndex = ShaderKeywordRadioGeneric("Outline Normals", newIndex, new[]
                        {
                            new GUIContent("R", "Use regular vertex normals"),
                            new GUIContent("VC", "Use vertex colors as normals (with smoothed mesh)"),
                            new GUIContent("T", "Use tangents as normals (with smoothed mesh)"),
                            new GUIContent("UV2", "Use second texture coordinates as normals (with smoothed mesh)")
                        });

        EditorGUILayout.EndVertical();

    }


    public static int ShaderKeywordRadioGeneric(string header, int index, GUIContent[] labels)
    {
        //Header and rect calculations
        var hasHeader = (!string.IsNullOrEmpty(header));
        var controlRect = EditorGUILayout.GetControlRect();
        var headerRect = EditorGUI.IndentedRect(controlRect);
        var r = headerRect;
        if (hasHeader)
        {
            headerRect.width = EditorGUIUtility.labelWidth - (EditorGUI.indentLevel * 15f);
            GUI.Label(headerRect, header, EditorStyles.label);
            r.width -= headerRect.width;
            r.x += headerRect.width;
        }
        else
        {
            r.width = Screen.width / EditorGUIUtility.pixelsPerPoint - 20f;
        }

        for (var i = 0; i < labels.Length; i++)
        {
            var rI = r;
            rI.width /= labels.Length;
            rI.x += i * rI.width;
            if (GUI.Toggle(rI, index == i, labels[i], (i == 0) ? EditorStyles.miniButtonLeft : (i == labels.Length - 1) ? EditorStyles.miniButtonRight : EditorStyles.miniButtonMid))
            {
                index = i;
            }
        }

        return index;
    }
}