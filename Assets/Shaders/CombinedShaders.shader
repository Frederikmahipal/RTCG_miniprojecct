Shader "Custom/URP_Combined_Rim_Glitch"
{
    Properties
    {
        _MainTex ("Vehicle Texture", 2D) = "white" {}
        [Header(Rim Lighting Effect)]
        _RimColor ("Rim Color", Color) = (0, 0.5, 1, 1)
        _RimPower ("Rim Power (Thickness)", Range(0.5, 8.0)) = 3.0
        
        [Header(Hologram Glitch Effect)]
        _HologramTint ("Hologram Tint", Color) = (0.2, 1, 0.8, 1)
        _GlitchIntensity ("Glitch Intensity", Range(0, 0.05)) = 0.02
        _ScanlineCount ("Scanline Count", Range(50, 500)) = 200.0
        _ScanlineSpeed ("Scanline Speed", Range(0, 10)) = 2.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _RimColor, _HologramTint;
            float _RimPower, _GlitchIntensity, _ScanlineCount, _ScanlineSpeed;

            Varyings vert (Attributes v)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = v.uv;

                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                float3 positionWS = TransformObjectToWorld(v.positionOS.xyz);
                o.viewDirWS = GetWorldSpaceViewDir(positionWS);

                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {

                float glitch = sin(_Time.y * 15.0 + i.uv.y * 50.0) * _GlitchIntensity;
                float2 glitchedUV = i.uv;
                glitchedUV.x += glitch;

                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, glitchedUV);

                float rim = 1.0 - saturate(dot(normalize(i.normalWS), normalize(i.viewDirWS)));
                rim = pow(rim, _RimPower);
                half3 rimColor = rim * _RimColor.rgb;


                float scanline = sin(i.uv.y * _ScanlineCount + _Time.y * _ScanlineSpeed) * 0.5 + 0.5;
                scanline = pow(scanline, 3.0); 


                half3 finalRGB = col.rgb * _HologramTint.rgb; 
                finalRGB *= (0.7 + 0.3 * scanline);           
                finalRGB += rimColor;                        

                return half4(finalRGB, col.a);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}