Shader "DefferedrLighting"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BumpMap("Normap Map", 2D) = "bump" {}
        _MetallicGlossMap("Metallic/AO/D/Smothness", 2D) = "" { }
    }
    SubShader
    {
        Tags { }
        ZWrite Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "..\ShaderLibrary\Common.hlsl"

            struct appdata
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
            };

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);//等于GT0
            TEXTURE2D(_GT1);
            TEXTURE2D(_GT2);
            TEXTURE2D(_GT3);
            half3 _WorldSpaceLightPos0;
            half3 glossyEnvironmentColor;
            
            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            CBUFFER_END

            v2f vert (appdata v)
            {
                v2f o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            half4 frag (v2f i) : SV_TARGET
            {
                half4 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                half3 normal = SAMPLE_TEXTURE2D(_GT1, sampler_MainTex, i.uv).xyz * 2 - 1;
                half3 lightDir = _WorldSpaceLightPos0;
                half NoL = max(0, dot(lightDir, normal));
                return albedo * NoL;
            }
            ENDHLSL
        }
    }
}
