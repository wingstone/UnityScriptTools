using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//单个自定义UI 支持圆形、矩形、圆角矩形
//TODO 添加UV坐标用以支持贴图
public class CustomUIMesh : MaskableGraphic
{
    [SerializeField]
    Texture m_Texture;
    public Texture MainTexture
    {
        get
        {
            return m_Texture;
        }
        set
        {
            if (m_Texture == value)
                return;
            m_Texture = value;
            SetAllDirty();
        }
    }

    public override Texture mainTexture
    {
        get
        {
            return m_Texture == null ? s_WhiteTexture : m_Texture;
        }
    }
    public enum UISHAPE
    {
        Circle, Rectangle, RoundRectangle
    }

    public UISHAPE uiShape = UISHAPE.Circle;

    //圆形细分数
    [Range(3, 100)]
    public int circleNum = 20;
    //矩形圆角细分数
    [Range(1, 100)]
    public int roundNum = 5;
    //矩形圆角半径
    [Range(0.0f, 100.0f)]
    public float roundRadius = 3.0f;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        if (uiShape == UISHAPE.Circle)
        {
            float radius = Mathf.Min(rectTransform.sizeDelta.x, rectTransform.sizeDelta.y) * 0.5f;

            vh.AddVert(Vector3.zero, color, new Vector2(0.5f, 0.5f));
            for (int i = 0; i <= circleNum; i++)
            {
                float cosx = Mathf.Cos(1.0f * i / circleNum * Mathf.PI * 2);
                float x = radius * cosx;
                float sinx = Mathf.Sin(1.0f * i / circleNum * Mathf.PI * 2);
                float y = radius * sinx;
                vh.AddVert(new Vector3(x, y), color, new Vector2(cosx * 0.5f + 0.5f, sinx * 0.5f + 0.5f));
            }

            for (int i = 1; i < circleNum + 1; i++)
            {
                vh.AddTriangle(0, i + 1, i);
            }
        }
        else if (uiShape == UISHAPE.Rectangle)
        {
            UIVertex[] vertices = new UIVertex[4];
            vertices[0].position = new Vector3(-rectTransform.sizeDelta.x * 0.5f, -rectTransform.sizeDelta.y * 0.5f);
            vertices[0].uv0 = new Vector2(0, 0);
            vertices[1].position = new Vector3(-rectTransform.sizeDelta.x * 0.5f, rectTransform.sizeDelta.y * 0.5f);
            vertices[1].uv0 = new Vector2(0, 1);
            vertices[2].position = new Vector3(rectTransform.sizeDelta.x * 0.5f, rectTransform.sizeDelta.y * 0.5f);
            vertices[2].uv0 = new Vector2(1, 1);
            vertices[3].position = new Vector3(rectTransform.sizeDelta.x * 0.5f, -rectTransform.sizeDelta.y * 0.5f);
            vertices[3].uv0 = new Vector2(1, 0);
            vertices[0].color = vertices[1].color = vertices[2].color = vertices[3].color = color;
            vh.AddUIVertexQuad(vertices);
        }
        else if (uiShape == UISHAPE.RoundRectangle)
        {
            float radius = Mathf.Min(roundRadius, rectTransform.sizeDelta.x * 0.5f);
            radius = Mathf.Min(radius, rectTransform.sizeDelta.y * 0.5f);
            float uround = radius / rectTransform.sizeDelta.x;
            float vround = radius / rectTransform.sizeDelta.y;
            int veRTNum = 0;    //存储已添加顶点，用于三角网格的索引

            //内部矩形
            UIVertex[] vertices = new UIVertex[4];
            vertices[0].position = new Vector3(-rectTransform.sizeDelta.x * 0.5f + radius, -rectTransform.sizeDelta.y * 0.5f + radius);
            vertices[0].uv0 = new Vector2(uround, vround);
            vertices[1].position = new Vector3(-rectTransform.sizeDelta.x * 0.5f + radius, rectTransform.sizeDelta.y * 0.5f - radius);
            vertices[1].uv0 = new Vector2(uround, 1 - vround);
            vertices[2].position = new Vector3(rectTransform.sizeDelta.x * 0.5f - radius, rectTransform.sizeDelta.y * 0.5f - radius);
            vertices[2].uv0 = new Vector2(1 - uround, 1 - vround);
            vertices[3].position = new Vector3(rectTransform.sizeDelta.x * 0.5f - radius, -rectTransform.sizeDelta.y * 0.5f + radius);
            vertices[3].uv0 = new Vector2(1 - uround, vround);
            vertices[0].color = vertices[1].color = vertices[2].color = vertices[3].color = color;
            vh.AddUIVertexQuad(vertices);
            veRTNum += 4;

            //round 1
            for (int i = 0; i <= roundNum; i++)
            {
                float cosx = Mathf.Cos(1.0f * i / roundNum * Mathf.PI * 0.5f);
                float x = -rectTransform.sizeDelta.x * 0.5f + radius - radius * cosx;
                float siny = Mathf.Sin(1.0f * i / roundNum * Mathf.PI * 0.5f);
                float y = -rectTransform.sizeDelta.y * 0.5f + radius - radius * siny;
                vh.AddVert(new Vector3(x, y), color, vertices[0].uv0 + new Vector2(-cosx * uround, -siny * vround));
            }
            for (int i = veRTNum; i < veRTNum + roundNum; i++)
            {
                vh.AddTriangle(0, i, i + 1);
            }
            veRTNum += roundNum + 1;

            //round 2
            for (int i = 0; i <= roundNum; i++)
            {
                float cosx = Mathf.Cos(1.0f * i / roundNum * Mathf.PI * 0.5f);
                float x = -rectTransform.sizeDelta.x * 0.5f + radius - radius * cosx;
                float siny = Mathf.Sin(1.0f * i / roundNum * Mathf.PI * 0.5f);
                float y = rectTransform.sizeDelta.y * 0.5f - radius + radius * siny;
                vh.AddVert(new Vector3(x, y), color, vertices[1].uv0 + new Vector2(-cosx * uround, siny * vround));
            }
            for (int i = veRTNum; i < veRTNum + roundNum; i++)
            {
                vh.AddTriangle(1, i, i + 1);
            }
            veRTNum += roundNum + 1;

            //round 3
            for (int i = 0; i <= roundNum; i++)
            {
                float cosx = Mathf.Cos(1.0f * i / roundNum * Mathf.PI * 0.5f);
                float x = rectTransform.sizeDelta.x * 0.5f - radius + radius * cosx;
                float siny = Mathf.Sin(1.0f * i / roundNum * Mathf.PI * 0.5f);
                float y = rectTransform.sizeDelta.y * 0.5f - radius + radius * siny;
                vh.AddVert(new Vector3(x, y), color, vertices[2].uv0 + new Vector2(cosx * uround, siny * vround));
            }
            for (int i = veRTNum; i < veRTNum + roundNum; i++)
            {
                vh.AddTriangle(2, i, i + 1);
            }
            veRTNum += roundNum + 1;

            //round 4
            for (int i = 0; i <= roundNum; i++)
            {
                float cosx = Mathf.Cos(1.0f * i / roundNum * Mathf.PI * 0.5f);
                float x = rectTransform.sizeDelta.x * 0.5f - radius + radius * cosx;
                float siny = Mathf.Sin(1.0f * i / roundNum * Mathf.PI * 0.5f);
                float y = -rectTransform.sizeDelta.y * 0.5f + radius - radius * siny;
                vh.AddVert(new Vector3(x, y), color, vertices[3].uv0 + new Vector2(cosx * uround, -siny * vround));
            }
            for (int i = veRTNum; i < veRTNum + roundNum; i++)
            {
                vh.AddTriangle(3, i, i + 1);
            }
            veRTNum += roundNum + 1;

            // rect1
            UIVertex[] vertices1 = new UIVertex[4];
            vertices1[0].position = new Vector3(-rectTransform.sizeDelta.x * 0.5f, -rectTransform.sizeDelta.y * 0.5f + radius);
            vertices1[0].uv0 = new Vector2(0, vround);
            vertices1[1].position = new Vector3(-rectTransform.sizeDelta.x * 0.5f, rectTransform.sizeDelta.y * 0.5f - radius);
            vertices1[1].uv0 = new Vector2(0, 1 - vround);
            vertices1[2].position = new Vector3(-rectTransform.sizeDelta.x * 0.5f + radius, rectTransform.sizeDelta.y * 0.5f - radius);
            vertices1[2].uv0 = new Vector2(uround, 1 - vround);
            vertices1[3].position = new Vector3(-rectTransform.sizeDelta.x * 0.5f + radius, -rectTransform.sizeDelta.y * 0.5f + radius);
            vertices1[3].uv0 = new Vector2(uround, vround);
            vertices1[0].color = vertices1[1].color = vertices1[2].color = vertices1[3].color = color;
            vh.AddUIVertexQuad(vertices1);
            veRTNum += 4;

            // rect2
            UIVertex[] vertices2 = new UIVertex[4];
            vertices2[0].position = new Vector3(-rectTransform.sizeDelta.x * 0.5f + radius, rectTransform.sizeDelta.y * 0.5f - radius);
            vertices2[0].uv0 = new Vector2(uround, 1 - vround);
            vertices2[1].position = new Vector3(-rectTransform.sizeDelta.x * 0.5f + radius, rectTransform.sizeDelta.y * 0.5f);
            vertices2[1].uv0 = new Vector2(uround, 1);
            vertices2[2].position = new Vector3(rectTransform.sizeDelta.x * 0.5f - radius, rectTransform.sizeDelta.y * 0.5f);
            vertices2[2].uv0 = new Vector2(1 - uround, 1);
            vertices2[3].position = new Vector3(rectTransform.sizeDelta.x * 0.5f - radius, rectTransform.sizeDelta.y * 0.5f - radius);
            vertices2[3].uv0 = new Vector2(1 - uround, 1 - vround);
            vertices2[0].color = vertices2[1].color = vertices2[2].color = vertices2[3].color = color;
            vh.AddUIVertexQuad(vertices2);
            veRTNum += 4;

            // rect3
            UIVertex[] vertices3 = new UIVertex[4];
            vertices3[0].position = new Vector3(rectTransform.sizeDelta.x * 0.5f - radius, -rectTransform.sizeDelta.y * 0.5f + radius);
            vertices3[0].uv0 = new Vector2(1 - uround, vround);
            vertices3[1].position = new Vector3(rectTransform.sizeDelta.x * 0.5f - radius, rectTransform.sizeDelta.y * 0.5f - radius);
            vertices3[1].uv0 = new Vector2(1 - uround, 1 - vround);
            vertices3[2].position = new Vector3(rectTransform.sizeDelta.x * 0.5f, rectTransform.sizeDelta.y * 0.5f - radius);
            vertices3[2].uv0 = new Vector2(1, 1 - vround);
            vertices3[3].position = new Vector3(rectTransform.sizeDelta.x * 0.5f, -rectTransform.sizeDelta.y * 0.5f + radius);
            vertices3[3].uv0 = new Vector2(1, vround);
            vertices3[0].color = vertices3[1].color = vertices3[2].color = vertices3[3].color = color;
            vh.AddUIVertexQuad(vertices3);
            veRTNum += 4;

            // rect4
            UIVertex[] vertices4 = new UIVertex[4];
            vertices4[0].position = new Vector3(-rectTransform.sizeDelta.x * 0.5f + radius, -rectTransform.sizeDelta.y * 0.5f);
            vertices4[0].uv0 = new Vector2(uround, 0);
            vertices4[1].position = new Vector3(-rectTransform.sizeDelta.x * 0.5f + radius, -rectTransform.sizeDelta.y * 0.5f + radius);
            vertices4[1].uv0 = new Vector2(uround, vround);
            vertices4[2].position = new Vector3(rectTransform.sizeDelta.x * 0.5f - radius, -rectTransform.sizeDelta.y * 0.5f + radius);
            vertices4[2].uv0 = new Vector2(1 - uround, vround);
            vertices4[3].position = new Vector3(rectTransform.sizeDelta.x * 0.5f - radius, -rectTransform.sizeDelta.y * 0.5f);
            vertices4[3].uv0 = new Vector2(1 - uround, 0);
            vertices4[0].color = vertices4[1].color = vertices4[2].color = vertices4[3].color = color;
            vh.AddUIVertexQuad(vertices4);
            veRTNum += 4;

        }

    }

    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();
        SetVerticesDirty();
        SetMaterialDirty();
    }
}
