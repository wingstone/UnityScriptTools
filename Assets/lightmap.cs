using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.SceneManagement;

public class lightmap : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [MenuItem("Assets/处理lightmap")]
    static void processlightmap()
    {
        string lightmappath = "Assets/11/Lightmap-0_comp_light.exr";
        string lightmappath1 = "Assets/11/Lightmap-0_comp_light1.exr";
        string shadowmaskpath = "Assets/11/Lightmap-0_comp_shadowmask.png";

        Texture2D lightmap = AssetDatabase.LoadAssetAtPath(lightmappath, typeof(Texture2D)) as Texture2D;
        Texture2D shadowmask = AssetDatabase.LoadAssetAtPath(shadowmaskpath, typeof(Texture2D)) as Texture2D;

        Color[] lightcolor = lightmap.GetPixels();
        Color[] shadowcolor = shadowmask.GetPixels();

        for (int i = 0; i < shadowcolor.Length; i++)
        {
            lightcolor[i].r = lightcolor[i].r*2;
            lightcolor[i].g = lightcolor[i].g*2;
            lightcolor[i].b = lightcolor[i].b*2;
            lightcolor[i].a = shadowcolor[i].r;
        }

        Texture2D lightmapnew = new Texture2D(lightmap.width, lightmap.height, TextureFormat.RGBAHalf, true, true);
        lightmapnew.SetPixels(lightcolor);

        File.WriteAllBytes(lightmappath1, lightmapnew.EncodeToEXR());

        AssetDatabase.Refresh();
        // SceneAsset scene =  EditorSceneManager.GetActiveScene();
        LightmapData[] lightmapdatas = new LightmapData[1];
        lightmapdatas[0] = new LightmapData();
        lightmapdatas[0].lightmapColor = AssetDatabase.LoadAssetAtPath(lightmappath1,typeof(Texture2D)) as Texture2D;
        LightmapSettings.lightmaps = lightmapdatas;
    }
}
