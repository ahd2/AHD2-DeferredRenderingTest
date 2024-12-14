//计算光照的函数
#ifndef CUSTOM_LIGHTING_INCLUDED
#define CUSTOM_LIGHTING_INCLUDED


half3 AmbientLighting(half3 Irradiance, half3 prefilteredColor, half3 color, half4 MAODS, half NoV)
{
    //ibl漫反射
    half3 DiffuseColor = (1.0 - MAODS.x) * color; // Metallic surfaces have no diffuse reflections
    half envLightIntensity = 1/*3*/;//临时用，后面要调整下光照输入
    half3 DiffuseContribution = envLightIntensity * DiffuseColor * Irradiance;//不除以pi可能是IBL图已经除以过了。
    //ibl镜面反射
    float2 iblLUT = SAMPLE_TEXTURE2D(_iblBrdfLut, sampler_iblBrdfLut, float2(NoV, min((1 - MAODS.a), 0.95)));
    half3 SpecularContribution = prefilteredColor * EnvBRDF(MAODS.x, color, iblLUT);
    //return 0;
    return (DiffuseContribution + SpecularContribution) * MAODS.y;
}

#endif