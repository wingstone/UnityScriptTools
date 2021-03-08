using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

// 以android平台为例
// 合并原因：
// unity中在移动平台使用LDR形式存储lightmap，占用三个通道，在Shader中运算时，动态解码为HDR数据；
// shadowmask在unity中，出于使用效率考虑，一般只使用R通道作为平行光的shadowmask；
// 但是lightmap在unity中TextureType为Lightmap；只能使用RGB三个通道；
// 因此，将lightmap与shadowmask进行合并是上上之选，即满足功能又满足性能；使用一张图的rgba四个通道即可；
public class MergeLightmap
{

    [MenuItem("Assets/合并当前Lightmap与Shadowmask")]
    static void processlightmap()
    {
        Scene scene = SceneManager.GetActiveScene();

        LightmapData[] lightmapDatas = LightmapSettings.lightmaps;

        for (int i = 0; i < lightmapDatas.Length; i++)
        {
            string lightmappath = AssetDatabase.GetAssetPath(lightmapDatas[i].lightmapColor);
            string newlightmappath = lightmappath.Replace(".exr", ".tga");
            string shadowmaskpath = AssetDatabase.GetAssetPath(lightmapDatas[i].shadowMask);

            //reset importer
            TextureImporter lightmapImporter = TextureImporter.GetAtPath(lightmappath) as TextureImporter;
            lightmapImporter.isReadable = true;
            TextureImporterPlatformSettings androidLMTexFMT = lightmapImporter.GetPlatformTextureSettings("Android");
            androidLMTexFMT.overridden = true;
            androidLMTexFMT.format = TextureImporterFormat.RGBA32;
            lightmapImporter.SetPlatformTextureSettings(androidLMTexFMT);
            lightmapImporter.SaveAndReimport();

            TextureImporter shadowmaskImporter = TextureImporter.GetAtPath(shadowmaskpath) as TextureImporter;
            shadowmaskImporter.isReadable = true;
            TextureImporterPlatformSettings androidSMTexFMT = shadowmaskImporter.GetPlatformTextureSettings("Android");
            androidSMTexFMT.overridden = true;
            androidSMTexFMT.format = TextureImporterFormat.RGBA32;
            shadowmaskImporter.SetPlatformTextureSettings(androidSMTexFMT);
            shadowmaskImporter.SaveAndReimport();

            Texture2D lightmap = AssetDatabase.LoadAssetAtPath(lightmappath, typeof(Texture2D)) as Texture2D;
            Texture2D shadowmask = AssetDatabase.LoadAssetAtPath(shadowmaskpath, typeof(Texture2D)) as Texture2D;

            Color[] lightcolor = lightmap.GetPixels();
            Color[] shadowcolor = shadowmask.GetPixels();

            for (int j = 0; j < shadowcolor.Length; j++)
            {
                lightcolor[j].r = lightcolor[j].r;
                lightcolor[j].g = lightcolor[j].g;
                lightcolor[j].b = lightcolor[j].b;
                lightcolor[j].a = shadowcolor[j].r;
            }

            //save merge lightmap
            Texture2D lightmapnew = new Texture2D(lightmap.width, lightmap.height, TextureFormat.RGBA32, true, true);
            lightmapnew.SetPixels(lightcolor);
            File.WriteAllBytes(newlightmappath, lightmapnew.EncodeToTGA());
            AssetDatabase.Refresh();

            lightmapDatas[i].lightmapColor = AssetDatabase.LoadAssetAtPath(newlightmappath, typeof(Texture2D)) as Texture2D;

            //reset old lightmap
            lightmapImporter.isReadable = false;
            lightmapImporter.SaveAndReimport();
            
            shadowmaskImporter.isReadable = false;
            shadowmaskImporter.SaveAndReimport();
        }


        AssetDatabase.Refresh();
        LightmapSettings.lightmaps = lightmapDatas;
    }
}
