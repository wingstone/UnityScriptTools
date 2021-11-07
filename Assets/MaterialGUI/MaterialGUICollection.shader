// https://docs.unity3d.com/Manual/SL-Properties.html
// https://docs.unity3d.com/ScriptReference/MaterialPropertyDrawer.html
Shader "Unlit/MaterialGUICollection"
{
    Properties
    {
        [Header(Color)]
        _ColorPerm("Color Perm", Color) = (1,1,1,1)
        [MainColor]_ColorMainPerm("Color Main Perm", Color) = (1,1,1,1)
        [HDR]_ColorHDRPerm("Color HDR Perm", Color) = (1,1,1,1)
        [HideInInspector]_ColorPerm("Color Perm", Color) = (1,1,1,1)

        [Header(Texture)]
        [Space]
        _Tex2DPerm ("Tex2D Perm", 2D) = "white" {}
        [MainTexture]_Tex2DMainPerm ("Tex2D Main Perm", 2D) = "white" {}
        [Normal]_Tex2DNormalPerm ("Tex2D Normal Perm", 2D) = "white" {}
        [NoScaleOffset]_Tex2DNoScaleOffsetPerm ("Tex2D NoScaleOffset Perm", 2D) = "white" {}
        [Gamma]_Tex2DGammaPerm ("Tex2D Gamma Perm", 2D) = "white" {}
        _Tex2DGrayPerm ("Tex2D Perm", 2D) = "" {}     // default is gray
        _Tex2DArrayPerm ("Tex2D Array Perm", 2DArray) = "" {}
        _Tex3DPerm("Tex3D Perm", 3D) = "" {}
        _CubemapPerm ("Cubemap Perm", Cube) = "" {}
        _CubemapArrayPerm ("Cubemap Array Perm", CubeArray) = "" {}
        
        
        [Header(Data)]
        // Note: In spite of the name, this type is actually backed by a float.
        _IntPerm("Int Perm", Int) = 1
        [IntRange]_IntRangePerm("Int Range Perm", Range(0, 10)) = 1
        [Space(10)]
        _FloatPerm ("Float Perm", Float) = 0.5
        _FloatRangePerm ("Float Range Perm", Range(0.0, 1.0)) = 0.5
        [PowerSlider(3.0)]_FloatPowerRangePerm ("Float Power Range Perm", Range(0.0, 100.0)) = 0.5
        _VectorPerm ("Vector Perm", Vector) = (.25, .5, .5, 1)
        
        [Header(Toggle)]
        // When this toggle is enabled, Unity enables a shader keyword with the name "_ANOTHER_FEATURE_ON".
        // When this toggle is disabled, Unity disables a shader keyword with the name "_ANOTHER_FEATURE_ON".
        [Toggle] _Another_Feature ("Toggle another feature", Float) = 0
        // When the toggle is enabled, Unity enables a shader keyword with the name "ENABLE_EXAMPLE_FEATURE".
        // When the toggle is disabled, Unity disables a shader keyword with the name "ENABLE_EXAMPLE_FEATURE".
        [Toggle(ENABLE_EXAMPLE_FEATURE)] _ExampleFeatureEnabled ("Toggle example feature", Float) = 0
        // When this toggle is enabled, Unity disables a shader keyword with the name "_ANOTHER_FEATURE_OFF".
        // When this toggle is disabled, Unity enables a shader keyword with the name "_ANOTHER_FEATURE_OFF".
        [ToggleOff] _Another_Feature ("ToggleOff another feature", Float) = 0
        // When the toggle is enabled, Unity disables a shader keyword with the name "DISABLE_EXAMPLE_FEATURE".
        // When the toggle is disabled, Unity enables a shader keyword with the name "DISABLE_EXAMPLE_FEATURE".
        [ToggleOff(DISABLE_EXAMPLE_FEATURE)] _ExampleFeatureEnabled ("ToggleOff example feature", Float) = 0

        [Header(Enum)]
        // Each option will set _OVERLAY_NONE, _OVERLAY_ADD, _OVERLAY_MULTIPLY shader keywords.
        [KeywordEnum(None, Add, Multiply)] _Overlay ("KeywordEnum Overlay mode", Float) = 0
        // A subset of blend mode values, just "One" (value 1) and "SrcAlpha" (value 5).
        [Enum(One,1,SrcAlpha,5)] _ValueEnum ("Value Enum", Float) = 1

        [Header(Option)]
        [Enum(UnityEngine.Rendering.BlendOp)]  _BlendOp  ("BlendOp", Float) = 0
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("SrcBlend", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("DstBlend", Float) = 0
        [Enum(Off, 0, On, 1)]_ZWriteMode ("ZWriteMode", float) = 1
        [Enum(UnityEngine.Rendering.CullMode)]_CullMode ("CullMode", float) = 2
        [Enum(UnityEngine.Rendering.CompareFunction)]_ZTestMode ("ZTestMode", Float) = 4
        [Enum(UnityEngine.Rendering.ColorWriteMask)]_ColorMask ("ColorMask", Float) = 15

        [Header(Stencil)]
        [Enum(UnityEngine.Rendering.CompareFunction)]_StencilComp ("Stencil Comparison", Float) = 8
        [IntRange]_StencilWriteMask ("Stencil Write Mask", Range(0,255)) = 255
        [IntRange]_StencilReadMask ("Stencil Read Mask", Range(0,255)) = 255
        [IntRange]_Stencil ("Stencil ID", Range(0,255)) = 0
        [Enum(UnityEngine.Rendering.StencilOp)]_StencilPass ("Stencil Pass", Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)]_StencilFail ("Stencil Fail", Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)]_StencilZFail ("Stencil ZFail", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            BlendOp [_BlendOp]
            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWriteMode]
            ZTest [_ZTestMode]
            Cull [_CullMode]
            ColorMask [_ColorMask]

            Stencil
            {
                Ref [_Stencil]
                Comp [_StencilComp]
                ReadMask [_StencilReadMask]
                WriteMask [_StencilWriteMask]
                Pass [_StencilPass]
                Fail [_StencilFail]
                ZFail [_StencilZFail]
            }

            CGPROGRAM
            
            #pragma multi_compile __ ENABLE_EXAMPLE_FEATURE
            #pragma multi_compile __ _ANOTHER_FEATURE_ON

            #pragma multi_compile __ DISABLE_EXAMPLE_FEATURE
            #pragma multi_compile __ _ANOTHER_FEATURE_OFF

            #pragma multi_compile _OVERLAY_NONE _OVERLAY_ADD _OVERLAY_MULTIPLY

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

            sampler2D _TexPerm;
            float4 _ColorPerm;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 col = tex2D(_TexPerm, i.uv)*_ColorPerm;
                return col;
            }
            ENDCG
        }
    }
}
