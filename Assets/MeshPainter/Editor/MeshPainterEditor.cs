using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(MeshPainter))]
public class MeshPainterEditor : Editor
{
    const int MaxBrushSize = 500;
    const int DefaultBrushSize = 50;
    const float DefaultBrushOpacity = 1f;
    const float WindowMinSizeX = 230f;
    const float BrushCellSize = 40f;
    const float DiffCellSize = 64f;
    const float LabelWidth = 60f;
    const float StartStopButtonHeight = 64f;
    const int StartStopButtonFontSize = 20;
    const float RayLength = 1000f;

    readonly Color32 ColorBlack = new Color32(0, 0, 0, 0);
    readonly Color32 ColorRed = new Color32(255, 0, 0, 0);
    readonly Color32 ColorGreen = new Color32(0, 255, 0, 0);
    readonly Color32 ColorBlue = new Color32(0, 0, 255, 0);
    readonly Color32 ColorAlpha = new Color32(0, 0, 0, 255);

    bool isPaint;


    // target
    MeshPainter meshPainter;

    // brush
    bool useDiffTex = false;
    private Texture2D[] BrushTextures;
    float brushSize = 16f;
    float brushStronger = 0.5f;

    private Texture2D[] DiffTextures;
    string[] layerStrings;
    private Texture2D splatTex;

    int selBrush = 0;
    int selTex = 0;

    int brushSizeInPourcent;

    // used mat
    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    Material material;
    Shader shader;

    #region  fix function
    void OnSceneGUI()
    {
        if (isPaint)
        {
            SceneView.RepaintAll();
            Painter();
        }
    }

    void OnEnable()
    {
        meshPainter = target as MeshPainter;

        // Load brush icons.
        BrushTextures = new Texture2D[20];
        for (int i = 0; i != BrushTextures.Length; ++i)
            BrushTextures[i] = (Texture2D)EditorGUIUtility.Load(string.Format("builtin_brush_{0}.png", i + 1));

        // initial layer strings.
        layerStrings = new string[4];
        layerStrings[0] = "R 通道";
        layerStrings[1] = "G 通道";
        layerStrings[2] = "B 通道";
        layerStrings[3] = "A 通道";

        // load shader
        shader = Shader.Find("Painter");

    }

    public override void OnInspectorGUI()
    {
        ShowBrushProperties();
        useDiffTex = EditorGUILayout.Toggle("Use Diff Tex?", useDiffTex);
        string helpMessage = UpdateObject();
        bool isValid = helpMessage.Equals("");

        // If no valid mesh selected.
        if (!isValid)
        {
            ShowHelp(helpMessage);
        }
        else
        {
            ShowStartStopButton();
            ShowTextures();
        }
    }

    #endregion

    void ShowBrushProperties()
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.BeginHorizontal("box", GUILayout.Width(318));
        selBrush = GUILayout.SelectionGrid(selBrush, BrushTextures, 10, "gridlist", GUILayout.Width(340), GUILayout.Height(70));
        GUILayout.EndHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        brushSize = EditorGUILayout.Slider("Size", brushSize, 1, MaxBrushSize);
        brushStronger = EditorGUILayout.Slider("Opacity", brushStronger, 0f, 1f);
    }

    string UpdateObject()
    {
        // check mesh filter
        meshFilter = meshPainter.GetComponent<MeshFilter>();

        if (meshFilter == null)
            return "GameObject must have a MeshFilter.";

        if (meshFilter.sharedMesh == null)
            return "MeshFilter must have a mesh.";

        // Check if GameObject has MeshRenderer with Material.
        meshRenderer = meshPainter.GetComponent<MeshRenderer>();

        if (meshRenderer == null)
            return "GameObject must have a MeshRenderer.";

        material = meshRenderer.sharedMaterial;
        if (material == null)
            return "MeshRenderer must have a material.";


        if (material.shader != shader)
            return "Material must use \"Painter Shader\".";

        string platProp = "SplatTex";
        string[] diffProps = new string[4] { "DiffTex1", "DiffTex2", "DiffTex3", "DiffTex4" };


        splatTex = material.GetTexture(platProp) as Texture2D;
        if (splatTex == null)
            return "Material must set SplatTex.";

        DiffTextures = new Texture2D[4];
        for (int i = 0; i != diffProps.Length; i++)
        {
            Texture2D tex = material.GetTexture(diffProps[i]) as Texture2D;
            if (tex != null)
                DiffTextures[i] = AssetPreview.GetAssetPreview(tex);
        }

        return string.Empty;
    }

    void ShowHelp(string p_helpMessage)
    {
        EditorGUILayout.HelpBox(p_helpMessage, MessageType.Info);
    }

    void ShowStartStopButton()
    {
        GUIStyle bigButtonStyle = new GUIStyle(GUI.skin.button);
        bigButtonStyle.fontSize = StartStopButtonFontSize;

        // Show button for drawing (Start or Stop).
        if (!isPaint)
        {
            GUI.color = Color.green;

            MeshCollider collider = meshPainter.GetComponent<MeshCollider>();
            if (collider == null)
            {
                if (GUILayout.Button("Add Mesh Collider", bigButtonStyle, GUILayout.Height(StartStopButtonHeight)))
                {
                    collider = meshPainter.gameObject.AddComponent<MeshCollider>();
                    collider.sharedMesh = meshFilter.sharedMesh;
                }
            }
            else
            {
                if (GUILayout.Button("Start Drawing", bigButtonStyle, GUILayout.Height(StartStopButtonHeight)))
                    StartDrawing();
            }


            GUI.color = Color.white;
        }
        else
        {
            GUI.color = Color.red;

            if (GUILayout.Button("Stop Drawing", bigButtonStyle, GUILayout.Height(StartStopButtonHeight)))
                StopDrawing();

            GUI.color = Color.white;
        }
    }

    void SetTextureImport()
    {
        string path = AssetDatabase.GetAssetPath(splatTex);
        TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;

        TextureImporterPlatformSettings androidTexFMT = textureImporter.GetPlatformTextureSettings("Android");
        androidTexFMT.overridden = true;
        androidTexFMT.format = TextureImporterFormat.RGBA32;
        textureImporter.SetPlatformTextureSettings(androidTexFMT);

        TextureImporterPlatformSettings iosTexFMT = textureImporter.GetPlatformTextureSettings("iPhone");
        iosTexFMT.overridden = true;
        iosTexFMT.format = TextureImporterFormat.RGBA32;
        textureImporter.SetPlatformTextureSettings(iosTexFMT);

        TextureImporterPlatformSettings pcTexFMT = textureImporter.GetPlatformTextureSettings("Standalone");
        pcTexFMT.overridden = true;
        pcTexFMT.format = TextureImporterFormat.RGBA32;
        textureImporter.SetPlatformTextureSettings(pcTexFMT);

        textureImporter.isReadable = true;
        textureImporter.SaveAndReimport();
        AssetDatabase.Refresh();
    }

    void RestoreTextureImport()
    {
        string path = AssetDatabase.GetAssetPath(splatTex);
        TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;

        TextureImporterPlatformSettings androidTexFMT = textureImporter.GetPlatformTextureSettings("Android");
        androidTexFMT.overridden = true;
        androidTexFMT.format = TextureImporterFormat.ETC2_RGBA8;
        textureImporter.SetPlatformTextureSettings(androidTexFMT);

        TextureImporterPlatformSettings iosTexFMT = textureImporter.GetPlatformTextureSettings("iPhone");
        iosTexFMT.overridden = true;
        iosTexFMT.format = TextureImporterFormat.ASTC_RGBA_4x4;
        textureImporter.SetPlatformTextureSettings(iosTexFMT);

        TextureImporterPlatformSettings pcTexFMT = textureImporter.GetPlatformTextureSettings("Standalone");
        pcTexFMT.overridden = true;
        pcTexFMT.format = TextureImporterFormat.DXT5;
        textureImporter.SetPlatformTextureSettings(pcTexFMT);

        textureImporter.isReadable = false;
        textureImporter.SaveAndReimport();
    }

    void StartDrawing()
    {
        isPaint = true;

        // set mesh readable
        string path = AssetDatabase.GetAssetPath(meshFilter.sharedMesh);
        ModelImporter modelImporter = AssetImporter.GetAtPath(path) as ModelImporter;
        modelImporter.isReadable = true;
        modelImporter.SaveAndReimport();

        // set texture readable
        SetTextureImport();

        EditorUtility.SetSelectedRenderState(meshRenderer, EditorSelectedRenderState.Hidden);
    }

    void StopDrawing()
    {
        if (isPaint)
        {
            SaveChange();
            isPaint = false;

            // set texture not readable
            RestoreTextureImport();

            AssetDatabase.Refresh();
        }

        // set mesh not readable
        string modelPath = AssetDatabase.GetAssetPath(meshFilter.sharedMesh);
        ModelImporter modelImporter = AssetImporter.GetAtPath(modelPath) as ModelImporter;
        modelImporter.isReadable = false;
        modelImporter.SaveAndReimport();

        MeshCollider collider = meshPainter.GetComponent<MeshCollider>();
        if (collider != null)
        {
            DestroyImmediate(collider);
        }

        EditorUtility.SetSelectedRenderState(meshRenderer, EditorSelectedRenderState.Highlight);
    }

    void ShowTextures()
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.BeginHorizontal("box", GUILayout.Width(340));
        if (!useDiffTex)
            selTex = GUILayout.SelectionGrid(selTex, layerStrings, 4, "gridlist", GUILayout.Width(340), GUILayout.Height(86));
        else
            selTex = GUILayout.SelectionGrid(selTex, DiffTextures, 4, "gridlist", GUILayout.Width(340), GUILayout.Height(86));
        GUILayout.EndHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    void Painter()
    {
        Transform trans = meshPainter.transform;
        float orthographicSize = (brushSize * trans.localScale.x) * (meshFilter.sharedMesh.bounds.size.x / 200);//笔刷在模型上的正交大小

        brushSizeInPourcent = (int)Mathf.Round((brushSize * splatTex.width) / 100);//笔刷在模型上的大小

        Event e = Event.current;
        HandleUtility.AddDefaultControl(0);

        RaycastHit hitInfo = new RaycastHit();
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        MeshCollider collider = meshPainter.GetComponent<MeshCollider>();

        if (collider.Raycast(ray, out hitInfo, Mathf.Infinity))
        {
            Handles.color = new Color(1f, 1f, 0f, 1f);//颜色
            Handles.DrawWireDisc(hitInfo.point, hitInfo.normal, orthographicSize);//根据笔刷大小在鼠标位置显示一个圆

            //鼠标点击或按下并拖动进行绘制
            if ((e.type == EventType.MouseDrag && e.alt == false && e.control == false && e.shift == false && e.button == 0) || (e.type == EventType.MouseDown && e.shift == false && e.alt == false && e.control == false && e.button == 0))
            {
                //选择绘制的通道
                Color targetColor = new Color(1f, 0f, 0f, 0f);
                switch (selTex)
                {
                    case 0:
                        targetColor = new Color(1f, 0f, 0f, 0f);
                        break;
                    case 1:
                        targetColor = new Color(0f, 1f, 0f, 0f);
                        break;
                    case 2:
                        targetColor = new Color(0f, 0f, 1f, 0f);
                        break;
                    case 3:
                        targetColor = new Color(0f, 0f, 0f, 1f);
                        break;

                }

                Vector2 pixelUV = hitInfo.textureCoord;

                //计算笔刷所覆盖的区域
                int PuX = Mathf.FloorToInt(pixelUV.x * splatTex.width);
                int PuY = Mathf.FloorToInt(pixelUV.y * splatTex.height);
                int x = Mathf.Clamp(PuX - brushSizeInPourcent / 2, 0, splatTex.width - 1);
                int y = Mathf.Clamp(PuY - brushSizeInPourcent / 2, 0, splatTex.height - 1);
                int width = Mathf.Clamp((PuX + brushSizeInPourcent / 2), 0, splatTex.width) - x;
                int height = Mathf.Clamp((PuY + brushSizeInPourcent / 2), 0, splatTex.height) - y;

                Color[] terrainBay = splatTex.GetPixels(x, y, width, height, 0);//获取Control贴图被笔刷所覆盖的区域的颜色

                Texture2D TBrush = BrushTextures[selBrush] as Texture2D;//获取笔刷性状贴图
                float[] brushAlpha = new float[brushSizeInPourcent * brushSizeInPourcent];//笔刷透明度

                //根据笔刷贴图计算笔刷的透明度
                for (int i = 0; i < brushSizeInPourcent; i++)
                {
                    for (int j = 0; j < brushSizeInPourcent; j++)
                    {
                        brushAlpha[j * brushSizeInPourcent + i] = TBrush.GetPixelBilinear(((float)i) / brushSizeInPourcent, ((float)j) / brushSizeInPourcent).a;
                    }
                }

                //计算绘制后的颜色
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        int index = (i * width) + j;
                        float Stronger = brushAlpha[Mathf.Clamp((y + i) - (PuY - brushSizeInPourcent / 2), 0, brushSizeInPourcent - 1) * brushSizeInPourcent + Mathf.Clamp((x + j) - (PuX - brushSizeInPourcent / 2), 0, brushSizeInPourcent - 1)] * brushStronger;

                        terrainBay[index] = Color.Lerp(terrainBay[index], targetColor, Stronger);
                    }
                }

                Undo.RegisterCompleteObjectUndo(splatTex, "meshPaint");//保存历史记录以便撤销

                splatTex.SetPixels(x, y, width, height, terrainBay, 0);//把绘制后的Control贴图保存起来
                splatTex.Apply();
            }
        }
    }

    public void SaveChange()
    {
        var path = AssetDatabase.GetAssetPath(splatTex);
        var bytes = splatTex.EncodeToTGA();
        File.WriteAllBytes(path, bytes);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);//刷新
    }

}

public static class MeshPainterUtility
{
    static void Create(int p_width, int p_height, Color32 p_color)
    {
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);

        if (path == "")
            path = "Assets";
        else if (Path.GetExtension(path) != "")
            path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");

        string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/SplatTex.tga");
        Texture2D tex = new Texture2D(p_width, p_height, TextureFormat.RGBA32, true);
        Color32[] colors = new Color32[p_width * p_height];

        for (int i = 0; i != colors.Length; ++i)
            colors[i] = p_color;

        tex.SetPixels32(colors);
        byte[] pngData = tex.EncodeToTGA();
        File.WriteAllBytes(assetPathAndName, pngData);
        Object.DestroyImmediate(tex);
        AssetDatabase.Refresh();
        TextureImporter ti = (TextureImporter)TextureImporter.GetAtPath(assetPathAndName);

        ti.wrapMode = TextureWrapMode.Clamp;
        AssetDatabase.ImportAsset(assetPathAndName);
        AssetDatabase.Refresh();
    }

    [MenuItem("Assets/Create/Splatmap/Red 32x32")]
    static void CreateRed32x32()
    {
        Create(32, 32, new Color32(255, 0, 0, 0));
    }

    [MenuItem("Assets/Create/Splatmap/Red 64x64")]
    static void CreateRed64x64()
    {
        Create(64, 64, new Color32(255, 0, 0, 0));
    }

    [MenuItem("Assets/Create/Splatmap/Red 128x128")]
    static void CreateRed128x128()
    {
        Create(128, 128, new Color32(255, 0, 0, 0));
    }

    [MenuItem("Assets/Create/Splatmap/Red 256x256")]
    static void CreateRed256x256()
    {
        Create(256, 256, new Color32(255, 0, 0, 0));
    }

    [MenuItem("Assets/Create/Splatmap/Red 512x512")]
    static void CreateRed512x512()
    {
        Create(512, 512, new Color32(255, 0, 0, 0));
    }

    [MenuItem("Assets/Create/Splatmap/Red 1024x1024")]
    static void CreateRed1024x1024()
    {
        Create(1024, 1024, new Color32(255, 0, 0, 0));
    }

    [MenuItem("Assets/Create/Splatmap/Red 2048x2048")]
    static void CreateRed2048x2048()
    {
        Create(2048, 2048, new Color32(255, 0, 0, 0));
    }
}