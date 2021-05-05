using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ref: https://threejs.org/examples/#webgl_gpgpu_water
// ref: https://github.com/mrdoob/three.js/blob/dev/examples/webgl_gpgpu_water.html
[RequireComponent(typeof(Camera))]
public class ScreenWaterPainter : MonoBehaviour
{
    [Range(0.001f, 0.1f)]
    public float radius = 0.01f;
    [Range(0.1f, 5f)]
    public float inten = 1f;
    [Range(0.9f, 1f)]
    public float fade = 0.98f;

    RenderTexture rt;
    Material mat;

    Camera camera;
    int width, height;
    bool useSmooth = false;
    bool stop = false;

    private void OnEnable()
    {
        camera = GetComponent<Camera>();
        width = camera.pixelWidth;
        height = camera.pixelHeight;

        rt = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.RGB111110Float);
        Shader shader = Shader.Find("Wave");
        mat = new Material(shader);
        mat.hideFlags = HideFlags.HideAndDontSave;
    }

    private void OnDisable()
    {
        RenderTexture.ReleaseTemporary(rt);
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 150, 100), "Press for Smooth"))
        {
            useSmooth = true;
        }

        if (GUI.Button(new Rect(200, 10, 150, 50), "Stop"))
        {
            stop = true;
        }
        if (GUI.Button(new Rect(200, 60, 150, 50), "Continue"))
        {
            stop = false;
        }
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        RenderTexture tmp = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.RGB111110Float);

        if (Input.GetMouseButton(0))    // 0: left 1: right 2: middle
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.x = mousePos.x / width;
            mousePos.y = mousePos.y / height;

            mat.SetVector("_Point", mousePos);
            mat.SetFloat("_Radius", radius);
            mat.SetFloat("_Inten", inten);
            mat.SetFloat("_Fade", fade);
        }
        else
        {
            mat.SetFloat("_Inten", 0);
        }

        // stop
        if (stop)
            Graphics.Blit(rt, tmp);
        else
            Graphics.Blit(rt, tmp, mat, 0);

        // smooth
        if (useSmooth)
        {
            for (int i = 0; i < 5; i++)
            {
                Graphics.Blit(tmp, rt, mat, 1);
                Graphics.Blit(rt, tmp, mat, 1);
            }
            useSmooth = false;
        }

        Graphics.Blit(tmp, rt);

        Shader.SetGlobalTexture("_WaveTex", rt);
        Graphics.Blit(src, dest, mat, 2);

        RenderTexture.ReleaseTemporary(tmp);
    }

}
