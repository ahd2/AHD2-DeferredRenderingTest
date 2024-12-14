#ifndef CUSTOM_BRDF_INCLUDED
#define CUSTOM_BRDF_INCLUDED
TEXTURE2D(_iblBrdfLut);    SAMPLER(sampler_iblBrdfLut);

float3 EnvBRDF(float Metallic, float3 BaseColor, float2 LUT)
{
    float3 F0 = lerp(0.04f, BaseColor.rgb, Metallic); 
    
    return F0 * LUT.x + LUT.y;
}
#endif