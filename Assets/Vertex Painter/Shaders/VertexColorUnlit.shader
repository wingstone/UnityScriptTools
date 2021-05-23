Shader "VertexColorUnlit" {
	Properties {
		_Splat0("Layer 0 (R)", 2D) = "red" {}
		_Splat1("Layer 1 (G)", 2D) = "green" {}
		_Splat2("Layer 2 (B)", 2D) = "blue" {}
		_Splat3("Layer 3 (A)", 2D) = "black" {}
	}

	SubShader {
		Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }

		Pass {
			Name "BASE"
			Tags { "LightMode" = "Always" }

			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog

			sampler2D _Splat0, _Splat1, _Splat2, _Splat3;
			uniform float4 _Splat0_ST, _Splat1_ST, _Splat2_ST, _Splat3_ST;

			struct a2v {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float4 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 pos : SV_POSITION;
				fixed4 color : COLOR0;
				float4 coord0 : TEXCOORD0;
				float4 coord1 : TEXCOORD1;
				UNITY_FOG_COORDS(3)
			};

			v2f vert(a2v v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.color = v.color;
				o.coord0.xy = v.texcoord.xy * _Splat0_ST.xy + _Splat0_ST.zw;
				o.coord0.zw = v.texcoord.xy * _Splat1_ST.xy + _Splat1_ST.zw;
				o.coord1.xy = v.texcoord.xy * _Splat2_ST.xy + _Splat2_ST.zw;
				o.coord1.zw = v.texcoord.xy * _Splat3_ST.xy + _Splat3_ST.zw;
				UNITY_TRANSFER_FOG(o, o.pos);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target {
				fixed4 result;
				result = i.color.r * tex2D(_Splat0, i.coord0.xy);
				result += i.color.g * tex2D(_Splat1, i.coord0.zw);
				result += i.color.b * tex2D(_Splat2, i.coord1.xy);
				result += i.color.a * tex2D(_Splat3, i.coord1.zw);
				UNITY_APPLY_FOG(i.fogCoord, result);
				UNITY_OPAQUE_ALPHA(result.a);
				return result;
			}
			ENDCG 
		}
	}
}