Shader "UI/VideoRGBPlusMaskUI"
{
    Properties
    {
        [PerRendererData] _MainTex ("RGB Video", 2D) = "white" {}
        _AlphaTex ("Mask Video", 2D) = "white" {}

        _Color ("Tint", Color) = (1,1,1,1)

        _AlphaBoost ("Alpha Boost", Range(0, 2)) = 1.0
        [Toggle] _InvertMask ("Invert Mask", Float) = 0

        // 마스크를 부드럽게/임계값 처리하고 싶을 때(압축 노이즈 대응)
        _Cutoff ("Mask Cutoff", Range(0, 1)) = 0.0
        _Softness ("Mask Softness", Range(0.0001, 1)) = 0.05

        // ===== UGUI Mask (Stencil) properties =====
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile __ UNITY_UI_CLIP_RECT
            #pragma multi_compile __ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 uv       : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            sampler2D _AlphaTex;
            float4 _MainTex_ST;

            fixed4 _Color;
            float4 _ClipRect;

            float _AlphaBoost;
            float _InvertMask;
            float _Cutoff;
            float _Softness;

            v2f vert(appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.worldPos = v.vertex;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 rgb = tex2D(_MainTex, i.uv);
                float  m   = tex2D(_AlphaTex, i.uv).r; // 마스크는 흑백 가정

                if (_InvertMask > 0.5) m = 1.0 - m;

                // (선택) 컷오프/부드러움으로 마스크 노이즈 정리
                // cutoff 이하 -> 0, cutoff~cutoff+softness -> 부드럽게
                float a = smoothstep(_Cutoff, _Cutoff + _Softness, m);
                a = saturate(a * _AlphaBoost);

                fixed4 col = rgb * i.color;
                col.a *= a;

                #ifdef UNITY_UI_CLIP_RECT
                col.a *= UnityGet2DClipping(i.worldPos.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(col.a - 0.001);
                #endif

                return col;
            }
            ENDCG
        }
    }
}
