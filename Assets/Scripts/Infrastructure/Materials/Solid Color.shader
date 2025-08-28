Shader "Custom/Sprite Solid"
{
    Properties
    {
        [PerRendererData]_MainTex("Sprite Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "CanUseSpriteAtlas"="True" "IgnoreProjector"="True" }
        Cull Off
        ZWrite Off
        // 프리멀티 알파 블렌딩
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex   : POSITION;
                float4 color    : COLOR;     // SpriteRenderer.color가 들어옵니다
                float2 texcoord : TEXCOORD0;
            };

            struct v2f {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                half2 texcoord  : TEXCOORD0;
            };

            sampler2D _MainTex;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex   = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                o.color    = v.color; // SR.color 전달 (RGB/Alpha 모두 사용)
            #ifdef PIXELSNAP_ON
                o.vertex = UnityPixelSnap(o.vertex);
            #endif
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 텍스처는 알파만 사용 (실루엣 유지), RGB는 완전히 무시
                fixed aTex = tex2D(_MainTex, i.texcoord).a;

                // 최종 알파 = 텍스처 알파 × SpriteRenderer.color 알파
                fixed a = aTex * i.color.a;

                // 프리멀티: RGB = (SpriteRenderer.color의 RGB) × 최종 알파
                fixed3 rgb = i.color.rgb * a;

                return fixed4(rgb, a);
            }
            ENDCG
        }
    }
}
