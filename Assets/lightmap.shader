Shader "Unlit/lightmap"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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
            // make fog work
            #pragma multi_compile_fwdbase

            #include "UnityCG.cginc"
            #include "UnityCG.cginc"		//常用函数，宏，结构体
            #include "Lighting.cginc"		//光源相关变量
            #include "AutoLight.cginc"		//光照，阴影相关宏，函数


            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float2 uv1 : TEXCOORD1;	//for lightmap
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 ambientOrLightmapUV : TEXCOORD5;
                float4 pos : SV_POSITION;		//shadow宏要求此处必须为pos变量，shit。。。
            };


            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                half4 ambientOrLightmapUV = 0;
                ambientOrLightmapUV.xy = v.uv1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
                ambientOrLightmapUV.zw = 0;
                o.ambientOrLightmapUV = ambientOrLightmapUV;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                half2 lightmapUV = 0;

                half3 ambient = 0;
                lightmapUV = i.ambientOrLightmapUV.xy;
                // Baked lightmaps
                half4 bakedColorTex = UNITY_SAMPLE_TEX2D(unity_Lightmap, lightmapUV.xy);
                ambient = DecodeLightmap(bakedColorTex);
                // #endif

                #ifdef UNITY_COLORSPACE_GAMMA
                    ambient = GammaToLinearSpace(ambient);
                #endif

                return half4(ambient, 1);
            }
            ENDCG
        }
    }
}
