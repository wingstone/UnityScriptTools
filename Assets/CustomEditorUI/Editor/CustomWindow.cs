using System;
using UnityEditor;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

public class CustomWindow : EditorWindow
{
    string head = @"<!-- Use this to make custom html placed on head -->";
    string html = @"<h1>Hello World</h1>";
    string css = "body {\n  background-color: white;\n}";
    string js = "/* Javascript Goes Here */";
    int panel = 0;

    class Styles
    {
        public static string template = "<html>\n<head>\n{3}\n<style>\n{1}\n</style>\n<script>\n{2}\n</script>\n</head>\n<body>\n{0}\n</body>\n</html>";
        public static GUIContent[] heads = new GUIContent[] { new GUIContent("HTML"), new GUIContent("CSS"), new GUIContent("JS"), new GUIContent("Head") };
        public static GUIStyle[] headStyles = new GUIStyle[] { EditorStyles.miniButtonLeft, EditorStyles.miniButtonMid, EditorStyles.miniButtonMid, EditorStyles.miniButtonRight };
        public static GUIStyle textArea = new GUIStyle(EditorStyles.textArea)
        {
            font = Font.CreateDynamicFontFromOSFont("Courier New", 12),
            wordWrap = true
        };
    }

    public string this[int idx]
    {
        get
        {
            switch (idx)
            {
                case 0: return html;
                case 1: return css;
                case 2: return js;
                case 3: return head;
                default: return "";
            }
        }
        set
        {
            switch (idx)
            {
                case 0: html = value; break;
                case 1: css = value; break;
                case 2: js = value; break;
                case 3: head = value; break;
            }
        }
    }

    [MenuItem("Window/CustomWindow")]
    static public void ShowWindow()
    {
        CustomWindow window = GetWindow<CustomWindow>("Custom Window", true);
        window.Show();
    }

    PreviewScene previewScene = null;
    RenderTexture previwRT = null;
    private void Awake()
    {
        previewScene = new PreviewScene("preview scene");
        previewScene.camera.clearFlags = CameraClearFlags.SolidColor;
        previewScene.camera.transform.position = new Vector3(0, 0, -10);
        previewScene.camera.backgroundColor = Color.grey;

        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.position = Vector3.zero;
        previewScene.AddGameObject(go);

        GameObject light = new GameObject("light", typeof(Light));
        previewScene.AddGameObject(light);
        Light Light1 = light.GetComponent<Light>();
        Light1.transform.rotation = Quaternion.Euler(340, 218, 177);
        Light1.color = new Color(.4f, .4f, .45f, 0f) * .7f;

        previwRT = new RenderTexture((int)position.width, (int)position.height - 30, 24, RenderTextureFormat.ARGBHalf);

    }

    void OnDestroy()
    {
        previewScene.Dispose();
    }

    string Compose()
    {
        return string.Format(Styles.template, html, css, js, head);
    }

    bool showPreview = false;

    void OnGUI()
    {

        var half = position.width / 2;

        // head/body/css/js
        var rect = new Rect(half * 0.5f + 25, 0, (half - 50) / 4, 30);

        EditorGUI.BeginChangeCheck();
        for (int i = 0; i < 4; i++)
        {
            if (GUI.Toggle(rect, i == panel, Styles.heads[i], Styles.headStyles[i]))
                panel = i;
            rect.x += rect.width;
        }

        showPreview = GUI.Toggle(new Rect(half * 1.5f, 0, 100, 30), showPreview, "Show Preview");

        if (showPreview)
        {
            if (previwRT.width != (int)position.width || previwRT.height != (int)position.height - 30)
            {
                previwRT.Release();
                previwRT.width = (int)position.width;
                previwRT.height = (int)position.height - 30;
                previwRT.Create();
            }

            Texture defaultEnvTexture = ReflectionProbe.defaultTexture;

            if (Unsupported.SetOverrideLightingSettings(previewScene.scene))
            {
                RenderSettings.ambientMode = AmbientMode.Flat;
                RenderSettings.ambientLight = Color.grey;

                RenderSettings.defaultReflectionMode = UnityEngine.Rendering.DefaultReflectionMode.Custom;
                RenderSettings.customReflection = defaultEnvTexture as Cubemap;
            }

            previewScene.camera.Render();
            EditorGUI.DrawTextureTransparent(new Rect(0, 30, position.width, position.height - 30), previwRT);
        }
        else
        {
            this[panel] = EditorGUI.TextArea(new Rect(0, 30, position.width, position.height - 30), this[panel], Styles.textArea);
        }
    }
}
