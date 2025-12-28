Shader "UI/ColorKeyUI"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        // === Color Key ===
        _KeyColor ("Key Color", Color) = (0,0,0,1)

        // 키 색과의 거리(색 차이)가 이 값보다 작으면 투명(0)
        _Threshold ("Threshold", Range(0, 1)) = 0.08

        // 경계 부드러움(0에 가까우면 딱딱, 크면 부드럽게 전이)
        _Softness ("Softness", Range(0.0001, 1)) = 0.05

        // 전체 알파를 더 강하게/약하게
        _AlphaBoost ("Alpha Boost", Range(0, 2)) = 1.0

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
            Name "Default"
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
                float2 texcoord : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            fixed4 _Color;
            float4 _ClipRect;

            fixed4 _KeyColor;
            float _Threshold;
            float _Softness;
            float _AlphaBoost;

            v2f vert(appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.worldPos = v.vertex;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 texCol = tex2D(_MainTex, i.texcoord);

                // --- Color key: 키 색과의 "RGB 거리"로 투명도 결정 ---
                // dist가 0에 가까울수록(키 색과 유사) 투명해짐
                float3 src = texCol.rgb;
                float3 key = _KeyColor.rgb;

                float dist = distance(src, key);

                // dist <= Threshold -> alpha 0
                // Threshold ~ Threshold+Softness -> 부드럽게 전이
                float keyA = smoothstep(_Threshold, _Threshold + _Softness, dist);
                keyA = saturate(keyA * _AlphaBoost);

                // UI Tint/VertexColor는 최종 색에만 적용 (키 판정은 원본 기준)
                fixed4 col = texCol * i.color;
                col.a *= keyA;

                // --- UGUI RectMask2D / Mask 호환 클리핑 ---
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
