Shader "VertexColorStandard" {
	Properties {
		_Splat0("Layer 0 (R)", 2D) = "red" {}
		_Splat1("Layer 1 (G)", 2D) = "green" {}
		_Splat2("Layer 2 (B)", 2D) = "blue" {}
		_Splat3("Layer 3 (A)", 2D) = "black" {}
		_Normal0("Normal 0 (R)", 2D) = "bump" {}
		_Normal1("Normal 1 (G)", 2D) = "bump" {}
		_Normal2("Normal 2 (B)", 2D) = "bump" {}
		_Normal3("Normal 3 (A)", 2D) = "bump" {}
		[Gamma]_Metallic0("Metallic 0", Range(0.0, 1.0)) = 0.0	
		[Gamma]_Metallic1("Metallic 1", Range(0.0, 1.0)) = 0.0	
		[Gamma]_Metallic2("Metallic 2", Range(0.0, 1.0)) = 0.0	
		[Gamma]_Metallic3("Metallic 3", Range(0.0, 1.0)) = 0.0
		_Glossiness("Smoothness", Range(0,1)) = 0.5
	}
	SubShader {
		Tags { "RenderType"="Opaque" "Queue" = "Geometry" }
		
		CGPROGRAM
		#pragma surface surf Standard vertex:vert fullforwardshadows
		#pragma target 3.0

		sampler2D _Splat0, _Splat1, _Splat2, _Splat3;
		sampler2D _Normal0, _Normal1, _Normal2, _Normal3;
		float4 _Splat0_ST, _Splat1_ST, _Splat2_ST, _Splat3_ST;
		half _Metallic0, _Metallic1, _Metallic2, _Metallic3;
		half _Glossiness;

		struct Input {
			fixed4 color : COLOR0;
			float4 coord0 : TEXCOORD0;
			float4 coord1 : TEXCOORD1;
		};

		void vert(inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.color = v.color;
			o.coord0.xy = v.texcoord.xy * _Splat0_ST.xy + _Splat0_ST.zw;
			o.coord0.zw = v.texcoord.xy * _Splat1_ST.xy + _Splat1_ST.zw;
			o.coord1.xy = v.texcoord.xy * _Splat2_ST.xy + _Splat2_ST.zw;
			o.coord1.zw = v.texcoord.xy * _Splat3_ST.xy + _Splat3_ST.zw;
		}

		void surf(Input IN, inout SurfaceOutputStandard o) {
			fixed4 col, nrm;
			col = IN.color.r * tex2D(_Splat0, IN.coord0.xy);
			col += IN.color.g * tex2D(_Splat1, IN.coord0.zw);
			col += IN.color.b * tex2D(_Splat2, IN.coord1.xy);
			col += IN.color.a * tex2D(_Splat3, IN.coord1.zw);
			nrm = IN.color.r * tex2D(_Normal0, IN.coord0.xy);
			nrm += IN.color.g * tex2D(_Normal1, IN.coord0.zw);
			nrm += IN.color.b * tex2D(_Normal2, IN.coord1.xy);
			nrm += IN.color.a * tex2D(_Normal3, IN.coord1.zw);
			o.Albedo = col.rgb;
			o.Alpha = col.a;
			o.Normal = UnpackNormal(nrm);
			o.Smoothness = _Glossiness;
			o.Metallic = dot(IN.color, half4(_Metallic0, _Metallic1, _Metallic2, _Metallic3));
		}
		ENDCG
	}
}