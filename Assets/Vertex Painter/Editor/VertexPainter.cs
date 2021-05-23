using UnityEditor;
using UnityEngine;

public class VertexPainter : EditorWindow
{
    //Standart Editor Variables//
    private static VertexPainter window;

    [MenuItem("Tools/Vertex Painter")]
    private static void CreateWindow()
    {
        //Init window
        int x = 150, y = 150, w = 320, h = 150;
        window = (VertexPainter)GetWindow(typeof(VertexPainter), false);
        window.position = new Rect(x, y, w + x, h + y);
        window.minSize = new Vector2(w, h);
        window.maxSize = new Vector2(w + 2, h + 2);
        window.title = "Vertex Paint";
        window.Show();
    }

    //Variables//
    private GameObject go;
    private Collider collider;
    private MeshFilter mf;
    private Mesh mesh;
    private Renderer renderer;
    private Vector3[] vertices;
    private Color[] originalColors, debugColors;
    private Material originalMaterial;
    private static Material debugMaterial;

    //GUI Variables//
    private bool tgl_Paint;
    private string str_Paint;
    private bool tgl_ShowVertexColors;
    private string str_ShowVertexColors;
    private Color gui_BrushColor;
    private float gui_BrushSize;
    private float gui_BrushOpacity;
    private string gui_Notification;
    private bool canPaint;

    //Standart Mono Methods//
    void OnEnable()
    {
        if (SceneView.onSceneGUIDelegate != OnSceneGUI)
            SceneView.onSceneGUIDelegate += OnSceneGUI;
        //Create debug material
        if (debugMaterial == null)
        {
            debugMaterial = new Material(Shader.Find("VertexColor"));
        }
        ResetMe();
    }

    void OnDisable()
    {
        if (SceneView.onSceneGUIDelegate == OnSceneGUI)
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
        //Cleanup
        DestroyImmediate(debugMaterial);
        ResetMe();
    }

    void OnSelectionChange()
    {
        ResetMe();
        window.Repaint();
    }

    void OnProjectChange()
    {
        ResetMe();
        window.Repaint();
    }

    void OnInspectorUpdate()
    {
        this.Repaint();
    }

    void OnGUI()
    {
        //Warnings
        if (!canPaint)
        {
            EditorGUILayout.HelpBox(gui_Notification, MessageType.Warning);
            return;
        }
        //We can paint now
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button(str_Paint))
        {
            tgl_Paint = !tgl_Paint;
            if (tgl_Paint)
            {
                str_Paint = "STOP PAINTING";
                //Debug Material
                renderer.sharedMaterial = debugMaterial;
                //Other button
                tgl_ShowVertexColors = true;
                str_ShowVertexColors = "HIDE VERTEX COLORS";
            }
            else
            {
                str_Paint = "START PAINTING";
                ResetMe();
            }
        }

        if (GUILayout.Button(str_ShowVertexColors))
        {
            tgl_ShowVertexColors = !tgl_ShowVertexColors;
            if (tgl_ShowVertexColors)
            {
                str_ShowVertexColors = "HIDE VERTEX COLORS";
                //Debug Material
                renderer.sharedMaterial = debugMaterial;
            }
            else
            {
                str_ShowVertexColors = "SHOW VERTEX COLORS";
                renderer.sharedMaterial = originalMaterial;
            }
        }
        EditorGUILayout.EndHorizontal();

        if (tgl_Paint)
        {
            //Top
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical();
            gui_BrushSize = EditorGUILayout.Slider("Brush Size :", gui_BrushSize, 0.1f, 10.0f);
            gui_BrushOpacity = EditorGUILayout.Slider("Brush Opacity :", gui_BrushOpacity, 0.0f, 1.0f);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Brush Color :");
            if (GUILayout.Button("R", GUILayout.ExpandWidth(false)))
            {
                gui_BrushColor = new Color(1f, 0f, 0f, 0f);
            }
            if (GUILayout.Button("G", GUILayout.ExpandWidth(false)))
            {
                gui_BrushColor = new Color(0f, 1f, 0f, 0f);
            }
            if (GUILayout.Button("B", GUILayout.ExpandWidth(false)))
            {
                gui_BrushColor = new Color(0f, 0f, 1f, 0f);
            }
            if (GUILayout.Button("A", GUILayout.ExpandWidth(false)))
            {
                gui_BrushColor = new Color(0f, 0f, 0f, 1f);
            }
            gui_BrushColor = EditorGUILayout.ColorField(gui_BrushColor, GUILayout.Height(20));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            //Center
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Vertex Colors :");
            if (GUILayout.Button("R", GUILayout.ExpandWidth(false)))
            {
                for (int i = 0; i < vertices.Length; i++)
                {
                    debugColors[i] = new Color(1f, 0f, 0f, 0f);
                }
                mesh.colors = debugColors;
                EditorUtility.SetDirty(go);
            }
            if (GUILayout.Button("G", GUILayout.ExpandWidth(false)))
            {
                for (int i = 0; i < vertices.Length; i++)
                {
                    debugColors[i] = new Color(0f, 1f, 0f, 0f);
                }
                mesh.colors = debugColors;
                EditorUtility.SetDirty(go);
            }
            if (GUILayout.Button("B", GUILayout.ExpandWidth(false)))
            {
                for (int i = 0; i < vertices.Length; i++)
                {
                    debugColors[i] = new Color(0f, 0f, 1f, 0f);
                }
                mesh.colors = debugColors;
                EditorUtility.SetDirty(go);
            }
            if (GUILayout.Button("A", GUILayout.ExpandWidth(false)))
            {
                for (int i = 0; i < vertices.Length; i++)
                {
                    debugColors[i] = new Color(0f, 0f, 0f, 1f);
                }
                mesh.colors = debugColors;
                EditorUtility.SetDirty(go);
            }
            if (GUILayout.Button("RESET"))
            {
                mesh.colors = originalColors;
                EditorUtility.SetDirty(go);
            }
            EditorGUILayout.EndHorizontal();

            //Bottom
            EditorGUILayout.Space();
            if (GUILayout.Button("SAVE NEW MESH"))
            {
                //Create an instance
                Mesh data = (Mesh)Instantiate(mesh);
                string name = EditorUtility.SaveFilePanel("Save Mesh", "Asset/Vertex Painter", "newMesh.asset","asset");
                AssetDatabase.CreateAsset(data, name);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                //Revert original mesh colors
                mesh.colors = originalColors;
                Debug.LogWarning("Mesh is Saved as " + name + ".");
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = data;
                //window.Close();
            }
        }
        EditorGUILayout.EndVertical();
    }

    void OnSceneGUI(SceneView sceneView)
    {
        if (!tgl_Paint)
        {
            return;
        }

        Event current = Event.current;
        Ray ray = HandleUtility.GUIPointToWorldRay(current.mousePosition);
        RaycastHit hit;
        //Events
        int controlID = GUIUtility.GetControlID(sceneView.GetHashCode(), FocusType.Passive);
        switch (current.GetTypeForControl(controlID))
        {
            case EventType.Layout:
                {
                    if (!tgl_Paint)
                    {
                        return;
                    }
                    HandleUtility.AddDefaultControl(controlID);
                }
                break;
            case EventType.MouseDown:
            case EventType.MouseDrag:
                {
                    if (current.GetTypeForControl(controlID) == EventType.MouseDrag && GUIUtility.hotControl != controlID)
                    {
                        return;
                    }
                    if (current.alt || current.control)
                    {
                        return;
                    }
                    if (current.button != 0)
                    {
                        return;
                    }
                    if (!tgl_Paint)
                    {
                        return;
                    }
                    if (HandleUtility.nearestControl != controlID)
                    {
                        return;
                    }
                    if (current.type == EventType.MouseDown)
                    {
                        GUIUtility.hotControl = controlID;
                    }
                    //Do painting
                    if (Physics.Raycast(ray, out hit, float.MaxValue))
                    {
                        if (hit.transform == go.transform)
                        {
                            Vector3 hitPos = Vector3.Scale(go.transform.InverseTransformPoint(hit.point), go.transform.localScale);
                            for (int i = 0; i < vertices.Length; i++)
                            {
                                Vector3 vertPos = Vector3.Scale(vertices[i], go.transform.localScale);
                                float mag = (vertPos - hitPos).magnitude;
                                if (mag > gui_BrushSize)
                                    continue;
                                debugColors[i] = Color.Lerp(debugColors[i], gui_BrushColor, gui_BrushOpacity);
                            }
                            mesh.colors = debugColors;
                        }
                    }
                    current.Use();
                }
                break;
            case EventType.MouseUp:
                {
                    if (!tgl_Paint)
                    {
                        return;
                    }
                    if (GUIUtility.hotControl != controlID)
                    {
                        return;
                    }
                    GUIUtility.hotControl = 0;
                    current.Use();
                }
                break;
            case EventType.Repaint:
                {
                    //Draw paint brush
                    if (Physics.Raycast(ray, out hit, float.MaxValue))
                    {
                        if (hit.transform == go.transform)
                        {
                            Handles.color = new Color(gui_BrushColor.r, gui_BrushColor.g, gui_BrushColor.b, 1.0f);
                            Handles.DrawWireDisc(hit.point, hit.normal, gui_BrushSize);
                        }
                    }
                    HandleUtility.Repaint();
                }
                break;
        }
    }

    //Private Methods//
    private void ResetMe()
    {
        //Reset previously worked on object if any
        if (go && originalMaterial)
        {
            go.GetComponent<Renderer>().sharedMaterial = originalMaterial;
            mesh.colors = originalColors;
        }

        //Reset variables
        go = null;
        collider = null;
        mf = null;
        mesh = null;
        renderer = null;
        vertices = null;
        originalColors = null;
        debugColors = null;
        originalMaterial = null;

        //Reset gui variables
        tgl_Paint = false;
        str_Paint = "START PAINTING";
        tgl_ShowVertexColors = false;
        str_ShowVertexColors = "SHOW VERTEX COLORS";
        gui_BrushColor = new Color(1f, 0f, 0f, 0f);
        gui_BrushSize = 1.0f;
        gui_BrushOpacity = 0.5f;
        canPaint = false;

        //Reset Selection
        go = Selection.activeGameObject;
        if (go != null)
        {
            collider = go.GetComponent<Collider>();
            if (collider != null)
            {
                mf = go.GetComponent<MeshFilter>();
                if (mf != null)
                {
                    mesh = mf.sharedMesh;
                    if (mesh != null)
                    {
                        //Save originals
                        renderer = go.GetComponent<Renderer>();
                        originalMaterial = renderer.sharedMaterial;
                        originalColors = mesh.colors;
                        //Set Arrays
                        vertices = mesh.vertices;
                        if (mesh.colors.Length > 0)
                            debugColors = mesh.colors;
                        else
                        {
                            Debug.LogWarning("Mesh originally has no vertex color data!!");
                            debugColors = new Color[vertices.Length];
                        }
                        //All is okay, we can paint now
                        canPaint = true;
                    }
                    else
                        gui_Notification = "Object doesnt have a mesh!";
                }
                else
                    gui_Notification = "Object doesnt have a MeshFilter!";
            }
            else
                gui_Notification = "Object doesnt have a collider!";
        }
        else
            gui_Notification = "No object selected!";
    }
}
