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
            TEXTURE2D(_DepthTex);   SAMPLER(sampler_DepthTex);
            //环境光贴图（默认是Skybox）
            TEXTURECUBE(unity_SpecCube0);   SAMPLER(samplerunity_SpecCube0);
            half3 _WorldSpaceLightPos0;
            half3 glossyEnvironmentColor;
            float4x4 unity_MatrixInvVP;
            
            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            CBUFFER_END

            v2f vert (appdata v)
            {
                v2f o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = v.uv;
                return o;
            }

            half4 frag (v2f i) : SV_TARGET
            {
                half4 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                
                //采样深度图
                #if UNITY_REVERSED_Z
                    real depth = SAMPLE_TEXTURE2D(_DepthTex, sampler_DepthTex, i.uv).r;
                #else
                    // 调整 z 以匹配 OpenGL 的 NDC
                    real depth = lerp(UNITY_NEAR_CLIP_VALUE, 1, SAMPLE_TEXTURE2D(_DepthTex, sampler_DepthTex, i.uv).r);
                #endif
                // 反投影重建世界坐标
                float4 ndcPos = float4(i.uv * 2 - 1, depth, 1);
                float4 worldPos = mul(unity_MatrixInvVP, ndcPos);
                worldPos /= worldPos.w;
                
                half3 normal = SAMPLE_TEXTURE2D(_GT1, sampler_MainTex, i.uv).xyz * 2 - 1;
                half3 lightDir = _WorldSpaceLightPos0;
                half NoL = max(0, dot(lightDir, normal));
                //return half4(depth,0, 0, 0);
                return half4(worldPos);
                return albedo * NoL;
            }
            ENDHLSL
        }
    }
}
