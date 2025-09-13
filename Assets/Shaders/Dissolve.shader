Shader "Custom/UnlitDissolve"
{
    Properties
    {
        _MainTex ("Main Tex", 2D) = "white" {}
        _NoiseTex ("Noise (grayscale)", 2D) = "white" {}
        _DissolveAmount ("Dissolve Amount", Range(0,1)) = 0.0
        _NoiseScale ("Noise Scale", Float) = 1.0
        _EdgeWidth ("Edge Width", Range(0.001,0.2)) = 0.02
        _UseAlphaClip ("Use Alpha Clip", Float) = 1.0
        _Cutoff ("Alpha Cutoff Threshold", Range(0,1)) = 0.01
    }

    SubShader
    {
        Tags { "RenderType"="TransparentCutout" "Queue"="AlphaTest" }
        LOD 100

        Pass
        {
            Cull Off
            ZWrite On
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _NoiseTex;
            float _DissolveAmount;
            float _NoiseScale;
            float _EdgeWidth;
            float _UseAlphaClip;
            float _Cutoff;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 uvNoise : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                // noise uv use world pos jitter potential: here use uv scaled
                o.uvNoise = v.uv * _NoiseScale;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                // sample textures
                float4 col = tex2D(_MainTex, i.uv);
                float noise = tex2D(_NoiseTex, i.uvNoise).r; // assume grayscale

                // compute dissolve: if noise < dissolveAmount -> removed
                // but we want a soft edge: compute a smooth edge mask
                float d = noise;
                float alpha = col.a;

                if (_UseAlphaClip > 0.5)
                {
                    // Hard/soft clip: if noise < dissolve -> discard
                    if (d < _DissolveAmount - _Cutoff)
                    {
                        discard;
                    }
                    // else keep full color (optionally modulate alpha slightly)
                }
                else
                {
                    // Fade out smoothly
                    float reveal = smoothstep(_DissolveAmount - _EdgeWidth, _DissolveAmount + _EdgeWidth, d);
                    alpha *= reveal;
                    if (alpha <= 0.001) discard;
                }

                // edge color add (emission-like)
                float3 finalRGB = col.rgb;

                return float4(finalRGB, alpha);
            }
            ENDCG
        }
    }

    FallBack "Diffuse"
}
