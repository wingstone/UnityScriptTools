using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AssetImportSetting : AssetPostprocessor
{
    // model
    void OnPreprocessModel()
    {
        ModelImporter modelImporter = assetImporter as ModelImporter;
        modelImporter.importMaterials = false;
        modelImporter.importLights = false;
        modelImporter.importCameras = false;
        modelImporter.importConstraints = false;
        // modelImporter.isReadable = false;
    }

    // texture
    void OnPreprocessTexture()
    {
        if (assetPath.Contains("Textures"))
        {
            TextureImporter textureImporter = (TextureImporter)assetImporter;
            textureImporter.textureType = TextureImporterType.Default;
            textureImporter.isReadable = false;

            TextureImporterPlatformSettings androidTexFMT = textureImporter.GetPlatformTextureSettings("Android");
            androidTexFMT.overridden = true;
            if (textureImporter.DoesSourceTextureHaveAlpha())
                androidTexFMT.format = TextureImporterFormat.ETC2_RGBA8;
            else
                androidTexFMT.format = TextureImporterFormat.ETC2_RGB4;
            textureImporter.SetPlatformTextureSettings(androidTexFMT);

            TextureImporterPlatformSettings iosTexFMT = textureImporter.GetPlatformTextureSettings("iPhone");
            iosTexFMT.overridden = true;
            if (textureImporter.DoesSourceTextureHaveAlpha())
                iosTexFMT.format = TextureImporterFormat.ASTC_RGBA_4x4;
            else
                iosTexFMT.format = TextureImporterFormat.ASTC_RGB_4x4;
            textureImporter.SetPlatformTextureSettings(iosTexFMT);

            TextureImporterPlatformSettings pcTexFMT = textureImporter.GetPlatformTextureSettings("Standalone");
            pcTexFMT.overridden = true;
            if (textureImporter.DoesSourceTextureHaveAlpha())
                pcTexFMT.format = TextureImporterFormat.DXT5;
            else
                pcTexFMT.format = TextureImporterFormat.DXT1;
            textureImporter.SetPlatformTextureSettings(pcTexFMT);
        }
    }
}
