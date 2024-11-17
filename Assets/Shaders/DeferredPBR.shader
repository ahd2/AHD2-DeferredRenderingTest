Shader "DefferedrPBR"
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

            struct output
            {
                half4 GT0 : SV_Target0;
                half4 GT1 : SV_Target1;
                half4 GT2 : SV_Target2;
                half4 GT3 : SV_Target3;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_BumpMap);
            TEXTURE2D(_MetallicGlossMap);
            
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

            output frag (v2f i)
            {
                i.normalWS = normalize(i.normalWS);
                i.tangentWS = normalize(i.tangentWS);
                i.bitangentWS = normalize(i.bitangentWS);
                half4 Albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                //采样法线贴图(后续封装
                half4 NormalMap = SAMPLE_TEXTURE2D(_BumpMap, sampler_MainTex, i.uv);
                NormalMap.xyz = UnpackNormal(NormalMap);

                half3x3 TBN = half3x3(i.tangentWS, i.bitangentWS, i.normalWS);
                half3 normalWS = TransformTangentToWorld(NormalMap.xyz, TBN);
                
                half4 MAODSMap = SAMPLE_TEXTURE2D(_MetallicGlossMap, sampler_MainTex, i.uv);
                //Gbuffer输出
                output output;
                output.GT0 = Albedo;
                output.GT1 = half4(normalWS * 0.5 + 0.5, 1); //
                output.GT2 = MAODSMap; //Metallic/AO/DetailMap/Smothness
                output.GT3 = 0.1;
                return output;
            }
            ENDHLSL
        }
    }
}
