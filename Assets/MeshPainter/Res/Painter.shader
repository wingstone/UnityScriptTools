Shader "Painter"
{
    Properties
    {
        SplatTex ("Texture", 2D) = "white" {}
        DiffTex1 ("DiffTex1", 2D) = "white" {}
        DiffTex2 ("DiffTex2", 2D) = "white" {}
        DiffTex3 ("DiffTex3", 2D) = "white" {}
        DiffTex4 ("DiffTex4", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D SplatTex;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 col = tex2D(SplatTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}
