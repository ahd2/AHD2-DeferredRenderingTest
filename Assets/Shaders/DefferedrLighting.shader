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
        Tags { "RenderType"="Opaque" "LightMode" = "Gbuffer"}
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "..\ShaderLibrary\Common.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"

            struct appdata
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
                float3 tangentOS : TANGENT;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD1;
                float3 normalWS  : TEXCOORD2;
                float3 tangentWS  : TEXCOORD3;
                float3 bitangentWS  : TEXCOORD4;
            };

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            TEXTURE2D(_GT1);
            TEXTURE2D(_GT2);
            
            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            CBUFFER_END

            v2f vert (appdata v)
            {
                v2f o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.tangentWS = TransformObjectToWorldDir(v.tangentOS);
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);//向量记得在片元归一化
                o.bitangentWS = cross(o.normalWS, o.tangentWS);
                o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            half4 frag (v2f i) : SV_TARGET
            {
                i.normalWS = normalize(i.normalWS);
                i.tangentWS = normalize(i.tangentWS);
                i.bitangentWS = normalize(i.bitangentWS);
                half4 Albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                half4 a = SAMPLE_TEXTURE2D(_GT1, sampler_MainTex, i.uv);
                return Albedo * a;
            }
            ENDHLSL
        }
    }
}
