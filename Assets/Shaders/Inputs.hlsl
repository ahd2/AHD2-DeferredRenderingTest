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

#endif