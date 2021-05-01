using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(ScreenPainter))]
public class ScreenPainterEditor : Editor
{
    Texture2D[] BrushTextures;
    // target
    MeshPainter meshPainter;

    void OnEnable()
    {
        meshPainter = target as MeshPainter;

        // Load brush icons.
        BrushTextures = new Texture2D[20];
        for (int i = 0; i != BrushTextures.Length; ++i)
            BrushTextures[i] = (Texture2D)EditorGUIUtility.Load(string.Format("builtin_brush_{0}.png", i + 1));

    }
}