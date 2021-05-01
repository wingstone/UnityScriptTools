Shader "Wave"
{
    Properties
    {
        _MainTex("Main tex", 2D) = "black"{}
        _Radius("redius", float) = 0.01
        _Point("point", Vector) = (0.5, 0.5, 0,0)
        _Inten("inten", float) = 5
    }
    SubShader
    {

        CGINCLUDE
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

        sampler2D _MainTex;
        float4 _MainTex_TexelSize;
        sampler2D _WaveTex;

        float2 _Point;
        float _Radius;
        float _Inten;

        v2f vert (appdata v)
        {
            v2f o;
            o.vertex = UnityObjectToClipPos(v.vertex);
            o.uv = v.uv;
            return o;
        }
        ENDCG

        Pass    // 0
        {
            CGPROGRAM
            float4 frag (v2f i) : SV_Target
            {
                float4 up = tex2D(_MainTex, i.uv + float2(0, _MainTex_TexelSize.y));
                float4 bottom = tex2D(_MainTex, i.uv + float2(0, -_MainTex_TexelSize.y));
                float4 left = tex2D(_MainTex, i.uv + float2(-_MainTex_TexelSize.x, 0));
                float4 right = tex2D(_MainTex, i.uv + float2(_MainTex_TexelSize.x, 0));
                float4 center = tex2D(_MainTex, i.uv);
                float4 col = center;

                col = (up + bottom + right + left)*0.5 - center;

                col *= 15.0/32;
                col += _Inten*(1 - smoothstep(0, _Radius, distance(i.uv, _Point)));

                return saturate(col);
            }
            ENDCG
        }

        Pass    // 1
        {
            CGPROGRAM
            float4 frag (v2f i) : SV_Target
            {
                float4 wave = tex2D(_WaveTex, i.uv);
                float4 col = tex2D(_MainTex, i.uv + wave.xx);
                return col;
            }
            ENDCG
        }
    }
}
