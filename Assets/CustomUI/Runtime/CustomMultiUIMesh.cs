using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//多行列UI 支持圆形、矩形、圆角矩形
public class CustomMultiUIMesh : MaskableGraphic
{
    //支持的集合图形
    public enum UISHAPE
    {
        Circle, Rectangle, RoundRectangle
    }

    public UISHAPE uiShape = UISHAPE.Circle;

    //行数
    [Range(1, 100)]
    public int Row = 1;
    //列数
    [Range(1, 100)]
    public int Column = 1;
    //行间距
    [Range(0.0f, 100.0f)]
    public float RowPadding = 3.0f;
    //列间距
    [Range(0.0f, 100.0f)]
    public float ColumnPadding = 3.0f;

    //圆形细分数
    [Range(3, 100)]
    public int circleNum = 20;
    //矩形圆角细分数
    [Range(1, 100)]
    public int roundNum = 5;
    //矩形圆角半径
    [Range(0.0f, 100.0f)]
    public float roundRadius = 3.0f;

    //重载纹理贴图
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

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        int haveVertNum = 0;    //存储已添加顶点，用于三角网格的索引
        for (int m = 0; m < Row; m++)
        {
            for (int n = 0; n < Column; n++)
            {
                Vector2 bl = new Vector2(rectTransform.rect.xMin, rectTransform.rect.yMin);
                Vector2 step = rectTransform.sizeDelta * new Vector2(1.0f / Column, 1.0f / Row);
                Vector2 center = bl + step * (new Vector2(n, m) + new Vector2(0.5f, 0.5f));

                if (uiShape == UISHAPE.Circle)
                {
                    float radius = Mathf.Min(rectTransform.sizeDelta.x / Row - RowPadding, rectTransform.sizeDelta.y / Column - ColumnPadding) * 0.5f;
                    radius = Mathf.Max(radius, 0);

                    vh.AddVert(new Vector3(center.x, center.y), color, new Vector2(0.5f, 0.5f));
                    for (int i = 0; i <= circleNum; i++)
                    {
                        float cosx = Mathf.Cos(1.0f * i / circleNum * Mathf.PI * 2);
                        float x = radius * cosx + center.x;
                        float sinx = Mathf.Sin(1.0f * i / circleNum * Mathf.PI * 2);
                        float y = radius * sinx + center.y;
                        vh.AddVert(new Vector3(x, y), color, new Vector2(cosx * 0.5f + 0.5f, sinx * 0.5f + 0.5f));
                    }

                    for (int i = 1; i < circleNum + 1; i++)
                    {
                        vh.AddTriangle(haveVertNum, haveVertNum + i + 1, haveVertNum + i);
                    }
                    haveVertNum += circleNum + 2;
                }
                else if (uiShape == UISHAPE.Rectangle)
                {
                    UIVertex[] vertices = new UIVertex[4];
                    RowPadding = Mathf.Min(step.y, RowPadding);
                    ColumnPadding = Mathf.Min(step.x, ColumnPadding);
                    float halfRowPadding = 0.5f * RowPadding;
                    float halfColumnPadding = 0.5f * ColumnPadding;

                    float xwidth = step.x * 0.5f - halfColumnPadding;
                    float yheight = step.y * 0.5f - halfRowPadding;
                    vertices[0].position = new Vector3(-xwidth, -yheight) + new Vector3(center.x, center.y);
                    vertices[0].uv0 = new Vector2(0, 0);
                    vertices[1].position = new Vector3(-xwidth, yheight) + new Vector3(center.x, center.y);
                    vertices[1].uv0 = new Vector2(0, 1);
                    vertices[2].position = new Vector3(xwidth, yheight) + new Vector3(center.x, center.y);
                    vertices[2].uv0 = new Vector2(1, 1);
                    vertices[3].position = new Vector3(xwidth, -yheight) + new Vector3(center.x, center.y);
                    vertices[3].uv0 = new Vector2(1, 0);
                    vertices[0].color = vertices[1].color = vertices[2].color = vertices[3].color = color;
                    vh.AddUIVertexQuad(vertices);
                    haveVertNum += 4;

                }
                else if (uiShape == UISHAPE.RoundRectangle)//padding 功能待做
                {
                    RowPadding = Mathf.Min(step.y, RowPadding);
                    ColumnPadding = Mathf.Min(step.x, ColumnPadding);
                    float halfRowPadding = 0.5f * RowPadding;
                    float halfColumnPadding = 0.5f * ColumnPadding;

                    int shapeVertNum = 0;   //存储已添加顶点，用于三角网格的索引
                    float xwidth = step.x * 0.5f - halfColumnPadding;
                    float yheight = step.y * 0.5f - halfRowPadding;

                    float radius = Mathf.Min(roundRadius, xwidth);
                    radius = Mathf.Min(radius, yheight);
                    float uround = radius / (step.x - ColumnPadding);
                    float vround = radius / (step.y - RowPadding);

                    //内部矩形
                    UIVertex[] vertices = new UIVertex[4];
                    vertices[0].position = new Vector3(-xwidth + radius, -yheight + radius) + new Vector3(center.x, center.y);
                    vertices[0].uv0 = new Vector2(uround, vround);
                    vertices[1].position = new Vector3(-xwidth + radius, yheight - radius) + new Vector3(center.x, center.y);
                    vertices[1].uv0 = new Vector2(uround, 1 - vround);
                    vertices[2].position = new Vector3(xwidth - radius, yheight - radius) + new Vector3(center.x, center.y);
                    vertices[2].uv0 = new Vector2(1 - uround, 1 - vround);
                    vertices[3].position = new Vector3(xwidth - radius, -yheight + radius) + new Vector3(center.x, center.y);
                    vertices[3].uv0 = new Vector2(1 - uround, vround);
                    vertices[0].color = vertices[1].color = vertices[2].color = vertices[3].color = color;
                    vh.AddUIVertexQuad(vertices);
                    shapeVertNum += 4;

                    //round 1
                    for (int i = 0; i <= roundNum; i++)
                    {
                        float cosx = Mathf.Cos(1.0f * i / roundNum * Mathf.PI * 0.5f);
                        float x = -xwidth + radius - radius * cosx + center.x;
                        float siny = Mathf.Sin(1.0f * i / roundNum * Mathf.PI * 0.5f);
                        float y = -yheight + radius - radius * siny + center.y;
                        vh.AddVert(new Vector3(x, y), color, vertices[0].uv0 + new Vector2(-cosx * uround, -siny * vround));
                    }
                    for (int i = haveVertNum + shapeVertNum; i < haveVertNum + shapeVertNum + roundNum; i++)
                    {
                        vh.AddTriangle(haveVertNum, i+1, i);
                    }
                    shapeVertNum += roundNum + 1;

                    //round 2
                    for (int i = 0; i <= roundNum; i++)
                    {
                        float cosx = Mathf.Cos(1.0f * i / roundNum * Mathf.PI * 0.5f);
                        float x = -xwidth + radius - radius * cosx + center.x;
                        float siny = Mathf.Sin(1.0f * i / roundNum * Mathf.PI * 0.5f);
                        float y = yheight - radius + radius * siny + center.y;
                        vh.AddVert(new Vector3(x, y), color, vertices[1].uv0 + new Vector2(-cosx * uround, siny * vround));
                    }
                    for (int i = haveVertNum + shapeVertNum; i < haveVertNum + shapeVertNum + roundNum; i++)
                    {
                        vh.AddTriangle(haveVertNum + 1, i, i + 1);
                    }
                    shapeVertNum += roundNum + 1;

                    //round 3
                    for (int i = 0; i <= roundNum; i++)
                    {
                        float cosx = Mathf.Cos(1.0f * i / roundNum * Mathf.PI * 0.5f);
                        float x = xwidth - radius + radius * cosx + center.x;
                        float siny = Mathf.Sin(1.0f * i / roundNum * Mathf.PI * 0.5f);
                        float y = yheight - radius + radius * siny + center.y;
                        vh.AddVert(new Vector3(x, y), color, vertices[2].uv0 + new Vector2(cosx * uround, siny * vround));
                    }
                    for (int i = haveVertNum + shapeVertNum; i < haveVertNum + shapeVertNum + roundNum; i++)
                    {
                        vh.AddTriangle(haveVertNum + 2, i+1, i);
                    }
                    shapeVertNum += roundNum + 1;

                    //round 4
                    for (int i = 0; i <= roundNum; i++)
                    {
                        float cosx = Mathf.Cos(1.0f * i / roundNum * Mathf.PI * 0.5f);
                        float x = xwidth - radius + radius * cosx + center.x;
                        float siny = Mathf.Sin(1.0f * i / roundNum * Mathf.PI * 0.5f);
                        float y = -yheight + radius - radius * siny + center.y;
                        vh.AddVert(new Vector3(x, y), color, vertices[3].uv0 + new Vector2(cosx * uround, -siny * vround));
                    }
                    for (int i = haveVertNum + shapeVertNum; i < haveVertNum + shapeVertNum + roundNum; i++)
                    {
                        vh.AddTriangle(haveVertNum + 3, i, i + 1);
                    }
                    shapeVertNum += roundNum + 1;

                    // rect1
                    UIVertex[] vertices1 = new UIVertex[4];
                    vertices1[0].position = new Vector3(-xwidth, -yheight + radius) + new Vector3(center.x, center.y);
                    vertices1[0].uv0 = new Vector2(0, vround);
                    vertices1[1].position = new Vector3(-xwidth, yheight - radius) + new Vector3(center.x, center.y);
                    vertices1[1].uv0 = new Vector2(0, 1 - vround);
                    vertices1[2].position = new Vector3(-xwidth + radius, yheight - radius) + new Vector3(center.x, center.y);
                    vertices1[2].uv0 = new Vector2(uround, 1 - vround);
                    vertices1[3].position = new Vector3(-xwidth + radius, -yheight + radius) + new Vector3(center.x, center.y);
                    vertices1[3].uv0 = new Vector2(uround, vround);
                    vertices1[0].color = vertices1[1].color = vertices1[2].color = vertices1[3].color = color;
                    vh.AddUIVertexQuad(vertices1);
                    shapeVertNum += 4;

                    // rect2
                    UIVertex[] vertices2 = new UIVertex[4];
                    vertices2[0].position = new Vector3(-xwidth + radius, yheight - radius) + new Vector3(center.x, center.y);
                    vertices2[0].uv0 = new Vector2(uround, 1 - vround);
                    vertices2[1].position = new Vector3(-xwidth + radius, yheight) + new Vector3(center.x, center.y);
                    vertices2[1].uv0 = new Vector2(uround, 1);
                    vertices2[2].position = new Vector3(xwidth - radius, yheight) + new Vector3(center.x, center.y);
                    vertices2[2].uv0 = new Vector2(1 - uround, 1);
                    vertices2[3].position = new Vector3(xwidth - radius, yheight - radius) + new Vector3(center.x, center.y);
                    vertices2[3].uv0 = new Vector2(1 - uround, 1 - vround);
                    vertices2[0].color = vertices2[1].color = vertices2[2].color = vertices2[3].color = color;
                    vh.AddUIVertexQuad(vertices2);
                    shapeVertNum += 4;

                    // rect3
                    UIVertex[] vertices3 = new UIVertex[4];
                    vertices3[0].position = new Vector3(xwidth - radius, -yheight + radius) + new Vector3(center.x, center.y);
                    vertices3[0].uv0 = new Vector2(1 - uround, vround);
                    vertices3[1].position = new Vector3(xwidth - radius, yheight - radius) + new Vector3(center.x, center.y);
                    vertices3[1].uv0 = new Vector2(1 - uround, 1 - vround);
                    vertices3[2].position = new Vector3(xwidth, yheight - radius) + new Vector3(center.x, center.y);
                    vertices3[2].uv0 = new Vector2(1, 1 - vround);
                    vertices3[3].position = new Vector3(xwidth, -yheight + radius) + new Vector3(center.x, center.y);
                    vertices3[3].uv0 = new Vector2(1, vround);
                    vertices3[0].color = vertices3[1].color = vertices3[2].color = vertices3[3].color = color;
                    vh.AddUIVertexQuad(vertices3);
                    shapeVertNum += 4;

                    // rect4
                    UIVertex[] vertices4 = new UIVertex[4];
                    vertices4[0].position = new Vector3(-xwidth + radius, -yheight) + new Vector3(center.x, center.y);
                    vertices4[0].uv0 = new Vector2(uround, 0);
                    vertices4[1].position = new Vector3(-xwidth + radius, -yheight + radius) + new Vector3(center.x, center.y);
                    vertices4[1].uv0 = new Vector2(uround, vround);
                    vertices4[2].position = new Vector3(xwidth - radius, -yheight + radius) + new Vector3(center.x, center.y);
                    vertices4[2].uv0 = new Vector2(1 - uround, vround);
                    vertices4[3].position = new Vector3(xwidth - radius, -yheight) + new Vector3(center.x, center.y);
                    vertices4[3].uv0 = new Vector2(1 - uround, 0);
                    vertices4[0].color = vertices4[1].color = vertices4[2].color = vertices4[3].color = color;
                    vh.AddUIVertexQuad(vertices4);
                    shapeVertNum += 4;
                    haveVertNum += shapeVertNum;
                }

            }
        }
    }

    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();
        SetVerticesDirty();
        SetMaterialDirty();
    }

}
