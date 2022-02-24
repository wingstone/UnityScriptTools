Shader "Custom/Debug"
{
    Properties
    {
        [Toggle]_UseMeanCurve ("UseMeanCurve", float) = 0
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
            #include "Lighting.cginc"		//光源相关变量
            #include "AutoLight.cginc"		//光照，阴影相关宏，函数


            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                float4 vertex : SV_POSITION;

                float3 normal : TEXCOORD1;
                float3 tangent : TEXCOORD2;
                float3 binormal : TEXCOORD3;
            };

            float GSquare(float x)
            {
                return x*x;
            }
            
            float CurvatureFromLight(
            float3 tangent,
            float3 bitangent,
            float3 curvTensor,
            float3 lightDir)
            {
                // Project light vector into tangent plane
                
                float2 lightDirProj = float2(dot(lightDir, tangent), dot(lightDir, bitangent));
                
                // NOTE (jasminp) We should normalize lightDirProj here in order to correctly
                //    calculate curvature in the light direction projected to the tangent plane.
                //    However, it makes no perceptible difference, since the skin LUT does not vary
                //    much with curvature when N.L is large.
                
                float curvature = curvTensor.x * GSquare(lightDirProj.x) +
                2.0f * curvTensor.y * lightDirProj.x * lightDirProj.y +
                curvTensor.z * GSquare(lightDirProj.y);
                
                return curvature;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;

                o.normal = UnityObjectToWorldNormal(v.normal);
                o.tangent = UnityObjectToWorldDir(v.tangent);

                float3x3 tangentToWorld = CreateTangentToWorldPerVertex(o.normal, o.tangent, v.tangent.w);
                o.tangent = tangentToWorld[0];
                o.binormal = tangentToWorld[1];
                o.normal = tangentToWorld[2];

                return o;
            }

            float _UseMeanCurve;

            float4 frag (v2f i) : SV_Target
            {
                float meanCurv = i.color.a;

                i.tangent =  normalize(i.tangent);
                i.normal = normalize(i.normal);
                i.binormal = normalize(i.binormal);
                float3 lightDir = _WorldSpaceLightPos0.xyz;

                float dirCurv = CurvatureFromLight(i.tangent, i.binormal, i.color.rgb, lightDir);

                if(_UseMeanCurve > 0)
                    return meanCurv/255;
                else
                    return dirCurv/255;
            }
            ENDCG
        }
    }
}
