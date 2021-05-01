using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ScreenPainter : MonoBehaviour
{
    public float radius = 0.01f;
    public float inten = 5f;

    RenderTexture rt;
    Material mat;

    Camera camera;
    int width, height;
    private void OnEnable()
    {
        camera = GetComponent<Camera>();
        width = camera.pixelWidth;
        height = camera.pixelHeight;

        rt = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.RFloat);
        Shader shader = Shader.Find("Wave");
        mat = new Material(shader);
        mat.hideFlags = HideFlags.HideAndDontSave;
    }

    private void OnDisable()
    {
        RenderTexture.ReleaseTemporary(rt);
    }

    void Update()
    {
        if (Input.GetMouseButton(0))    // 0: left 1: right 2: middle
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.x = mousePos.x / width;
            mousePos.y = mousePos.y / height;

            mat.SetVector("_Point", mousePos);
            mat.SetFloat("_Radius", radius);
            mat.SetFloat("_Inten", inten);
        }
        else
        {
            mat.SetFloat("_Inten", 0);
        }
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        RenderTexture tmp = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.RFloat);

        Graphics.Blit(rt, tmp, mat, 0);
        mat.SetFloat("_Inten", 0);
        Graphics.Blit(tmp, rt, mat, 0);
        Shader.SetGlobalTexture("_WaveTex", rt);
        Graphics.Blit(src, dest, mat, 1);

        RenderTexture.ReleaseTemporary(tmp);
    }

}
