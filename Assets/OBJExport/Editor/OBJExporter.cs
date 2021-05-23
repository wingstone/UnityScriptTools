#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;

public class OBJExporter : ScriptableWizard
{
    public bool onlySelectedObjects = true;
    public bool applyPosition = true;
    public bool applyRotation = true;
    public bool applyScale = true;
    public bool splitObjects = true;
    public bool objNameAddIdNum = false;

    //public bool materialsUseTextureName = false;

    private string versionString = "v2.0";
    private string lastExportFolder;

    bool StaticBatchingEnabled()
    {
        PlayerSettings[] playerSettings = Resources.FindObjectsOfTypeAll<PlayerSettings>();
        if (playerSettings == null)
        {
            return false;
        }
        SerializedObject playerSettingsSerializedObject = new SerializedObject(playerSettings);
        SerializedProperty batchingSettings = playerSettingsSerializedObject.FindProperty("m_BuildTargetBatching");
        for (int i = 0; i < batchingSettings.arraySize; i++)
        {
            SerializedProperty batchingArrayValue = batchingSettings.GetArrayElementAtIndex(i);
            if (batchingArrayValue == null)
            {
                continue;
            }
            IEnumerator batchingEnumerator = batchingArrayValue.GetEnumerator();
            if (batchingEnumerator == null)
            {
                continue;
            }
            while (batchingEnumerator.MoveNext())
            {
                SerializedProperty property = (SerializedProperty)batchingEnumerator.Current;
                if (property != null && property.name == "m_StaticBatching")
                {
                    return property.boolValue;
                }
            }
        }
        return false;
    }

    void OnWizardUpdate()
    {
        helpString = "Aaro4130's OBJ Exporter " + versionString;
    }

    Vector3 RotateAroundPoint(Vector3 point, Vector3 pivot, Quaternion angle)
    {
        return angle * (point - pivot) + pivot;
    }
    Vector3 MultiplyVec3s(Vector3 v1, Vector3 v2)
    {
        return new Vector3(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z);
    }

    void OnWizardCreate()
    {
        if (StaticBatchingEnabled() && Application.isPlaying)
        {
            EditorUtility.DisplayDialog("Error", "Static batching is enabled. This will cause the export file to look like a mess, as well as be a large filesize. Disable this option, and restart the player, before continuing.", "OK");
            goto end;
        }
        string lastPath = EditorPrefs.GetString("a4_OBJExport_lastPath", "");
        string lastFileName = EditorPrefs.GetString("a4_OBJExport_lastFile", "unityexport.obj");
        string expFile = EditorUtility.SaveFilePanel("Export OBJ", lastPath, lastFileName, "obj");
        if (expFile.Length > 0)
        {
            var fi = new System.IO.FileInfo(expFile);
            EditorPrefs.SetString("a4_OBJExport_lastFile", fi.Name);
            EditorPrefs.SetString("a4_OBJExport_lastPath", fi.Directory.FullName);
            Export(expFile);
        }
    end:;
    }

    void Export(string exportPath)
    {
        //init stuff
        Dictionary<string, bool> materialCache = new Dictionary<string, bool>();
        var exportFileInfo = new System.IO.FileInfo(exportPath);
        lastExportFolder = exportFileInfo.Directory.FullName;
        string baseFileName = System.IO.Path.GetFileNameWithoutExtension(exportPath);
        EditorUtility.DisplayProgressBar("Exporting OBJ", "Please wait.. Starting export.", 0);

        //get list of required export things
        MeshFilter[] sceneMeshes;
        if (onlySelectedObjects)
        {
            List<MeshFilter> tempMFList = new List<MeshFilter>();
            foreach (GameObject g in Selection.gameObjects)
            {

                MeshFilter f = g.GetComponent<MeshFilter>();
                if (f != null)
                {
                    tempMFList.Add(f);
                }

            }
            sceneMeshes = tempMFList.ToArray();
        }
        else
        {
            sceneMeshes = FindObjectsOfType(typeof(MeshFilter)) as MeshFilter[];

        }

        if (Application.isPlaying)
        {
            foreach (MeshFilter mf in sceneMeshes)
            {
                MeshRenderer mr = mf.gameObject.GetComponent<MeshRenderer>();
                if (mr != null)
                {
                    if (mr.isPartOfStaticBatch)
                    {
                        EditorUtility.ClearProgressBar();
                        EditorUtility.DisplayDialog("Error", "Static batched object detected. Static batching is not compatible with this exporter. Please disable it before starting the player.", "OK");
                        return;
                    }
                }
            }
        }

        //work on export
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("# Export of " + Application.loadedLevelName);
        sb.AppendLine("# from Aaro4130 OBJ Exporter " + versionString);

        float maxExportProgress = (float)(sceneMeshes.Length + 1);
        int lastIndex_v = 0;
        int lastIndex_n = 0;
        int lastIndex_uv = 0;
        for (int i = 0; i < sceneMeshes.Length; i++)
        {
            string meshName = sceneMeshes[i].gameObject.name;
            float progress = (float)(i + 1) / maxExportProgress;
            EditorUtility.DisplayProgressBar("Exporting objects... (" + Mathf.Round(progress * 100) + "%)", "Exporting object " + meshName, progress);
            MeshFilter mf = sceneMeshes[i];
            MeshRenderer mr = sceneMeshes[i].gameObject.GetComponent<MeshRenderer>();

            if (mf == null)
                continue;

            if (splitObjects)
            {
                string exportName = meshName;
                if (objNameAddIdNum)
                {
                    exportName += "_" + i;
                }
                sb.AppendLine("g " + exportName);
            }

            //export the meshhh :3
            Mesh msh = mf.sharedMesh;
            int faceOrder = (int)Mathf.Clamp((mf.gameObject.transform.lossyScale.x * mf.gameObject.transform.lossyScale.z), -1, 1);

            //export vector data (FUN :D)!
            foreach (Vector3 vx in msh.vertices)
            {
                Vector3 v = vx;
                if (applyScale)
                {
                    v = MultiplyVec3s(v, mf.gameObject.transform.lossyScale);
                }

                if (applyRotation)
                {

                    v = RotateAroundPoint(v, Vector3.zero, mf.gameObject.transform.rotation);
                }

                if (applyPosition)
                {
                    v += mf.gameObject.transform.position;
                }
                v.x *= -1;
                sb.AppendLine("v " + v.x + " " + v.y + " " + v.z);
            }

            bool havenormal = msh.normals != null;
            if (havenormal)
            {
                foreach (Vector3 vx in msh.normals)
                {
                    Vector3 v = vx;

                    if (applyScale)
                    {
                        v = MultiplyVec3s(v, mf.gameObject.transform.lossyScale.normalized);
                    }
                    if (applyRotation)
                    {
                        v = RotateAroundPoint(v, Vector3.zero, mf.gameObject.transform.rotation);
                    }
                    v.x *= -1;
                    sb.AppendLine("vn " + v.x + " " + v.y + " " + v.z);

                }
            }

            bool haveuv = msh.uv != null;
            if (haveuv)
            {
                foreach (Vector2 v in msh.uv)
                {
                    sb.AppendLine("vt " + v.x + " " + v.y);
                }
            }

            for (int j = 0; j < msh.subMeshCount; j++)
            {
                if (mr != null && j < mr.sharedMaterials.Length)
                {
                    string matName = mr.sharedMaterials[j].name;
                    sb.AppendLine("usemtl " + matName);
                }
                else
                {
                    sb.AppendLine("usemtl " + meshName + "_sm" + j);
                }

                int[] tris = msh.GetTriangles(j);
                for (int t = 0; t < tris.Length; t += 3)
                {
                    int idx2_v = tris[t] + 1 + lastIndex_v;
                    int idx2_n = tris[t] + 1 + lastIndex_n;
                    int idx2_uv = tris[t] + 1 + lastIndex_uv;
                    int idx1_v = tris[t + 1] + 1 + lastIndex_v;
                    int idx1_n = tris[t + 1] + 1 + lastIndex_n;
                    int idx1_uv = tris[t + 1] + 1 + lastIndex_uv;
                    int idx0_v = tris[t + 2] + 1 + lastIndex_v;
                    int idx0_n = tris[t + 2] + 1 + lastIndex_n;
                    int idx0_uv = tris[t + 2] + 1 + lastIndex_uv;
                    if (faceOrder < 0)
                    {
                        sb.AppendLine("f " + ConstructOBJString(idx2_v, idx2_n, idx2_uv, havenormal, haveuv) + " " + ConstructOBJString(idx1_v, idx1_n, idx1_uv, havenormal, haveuv) + " " + ConstructOBJString(idx0_v, idx0_n, idx0_uv, havenormal, haveuv));
                    }
                    else
                    {
                        sb.AppendLine("f " + ConstructOBJString(idx0_v, idx0_n, idx0_uv, havenormal, haveuv) + " " + ConstructOBJString(idx1_v, idx1_n, idx1_uv, havenormal, haveuv) + " " + ConstructOBJString(idx2_v, idx2_n, idx2_uv, havenormal, haveuv));
                    }

                }
            }

            lastIndex_v += msh.vertices.Length;
            if (havenormal)
                lastIndex_n += msh.normals.Length;
            if (haveuv)
                lastIndex_uv += msh.uv.Length;
        }

        //write to disk
        System.IO.File.WriteAllText(exportPath, sb.ToString());

        //export complete, close progress dialog
        EditorUtility.ClearProgressBar();
        Process.Start(Path.GetDirectoryName(exportPath));
    }

    private string ConstructOBJString(int id_v, int id_n, int id_uv, bool havenormal, bool haveuv)
    {
        string idxString = id_v.ToString();
        if (havenormal && haveuv)
            return id_v + "/" + id_uv + "/" + id_n;
        if (havenormal && !haveuv)
            return id_v + "/" + "/" + id_n;
        if (!havenormal && !haveuv)
            return id_v.ToString();
        if (!havenormal && haveuv)
            return id_v + "/" + id_uv;

        return id_v.ToString();
    }

    [MenuItem("Tools/Export Wavefront OBJ")]
    static void CreateWizard()
    {
        ScriptableWizard.DisplayWizard("Export OBJ", typeof(OBJExporter), "Export");
    }
}
#endif