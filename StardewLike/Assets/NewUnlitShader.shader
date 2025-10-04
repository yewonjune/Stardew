Shader "Custom/MultiplyDarkenSprite"
{
Properties
    {
        _Color ("Tint (use alpha to control strength)", Color) = (0,0,0,0.5)
        _MainTex ("Sprite", 2D) = "white" {}
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "CanUseSpriteAtlas"="True"
        }
        Cull Off
        Lighting Off
        ZWrite Off

        // 핵심: Multiply 블렌딩 (Src * Dst)
        Blend DstColor Zero

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex   : POSITION;
                float2 texcoord : TEXCOORD0;
                float4 color    : COLOR;
            };

            struct v2f {
                float4 pos  : SV_POSITION;
                float2 uv   : TEXCOORD0;
                float4 color: COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.color = v.color * _Color; // SpriteRenderer/Tilemap 색 * 머티리얼 색
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // _Color.a = 어둡게 할 강도 (0=그대로, 1=완전검정)
                float f = 1.0 - saturate(_Color.a);   // 곱할 계수 (1~0)
                return fixed4(f, f, f, 1.0);          // 이 값이 DestColor와 곱해짐 (Blend DstColor Zero)
            }
            ENDCG
        }
    }
    
}
