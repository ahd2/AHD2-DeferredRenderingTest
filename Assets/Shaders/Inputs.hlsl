#ifndef INPUT_INCLUDED
#define INPUT_INCLUDED
#include "..\ShaderLibrary\Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"

TEXTURE2D(_MainTex);    SAMPLER(sampler_MainTex);
TEXTURE2D(_BumpMap);
TEXTURE2D(_MetallicGlossMap);
//环境光贴图（默认是Skybox）
TEXTURECUBE(unity_SpecCube0);   SAMPLER(samplerunity_SpecCube0);

CBUFFER_START(UnityPerMaterial)
float4 _MainTex_ST;
CBUFFER_END

CBUFFER_START(UnityLighting)
// SH lighting environment
half4 unity_SHAr;
half4 unity_SHAg;
half4 unity_SHAb;
half4 unity_SHBr;
half4 unity_SHBg;
half4 unity_SHBb;
half4 unity_SHC;
CBUFFER_END
#endif