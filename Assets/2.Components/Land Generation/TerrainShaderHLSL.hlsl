#ifndef TERRAINSHADER
#define TERRAINSHADER

float inverseLerp(float a, float b, float value)
{
    return saturate((value - a) / (b - a));
}

static const float epsilon = 1e-4;
static const int maxColorCount = 3;

static const float3 baseColors[maxColorCount] = {float3(1, 0, 0), float3(0, 0, 1), float3(1, 1, 0)};
static const float baseStartHeights[maxColorCount] = {0, 0.29, 0.53};
static const float baseBlends[maxColorCount] = {0, 0.462, .47};

void TerrainShader_float(float3 worldPos, float minHeight, float maxHeight,/* Texture2D<float4> floatArrayTexture,*/ out float3 Out)
{
    SamplerState my_point_clamp_sampler;
    // float4 baseStartHeights[maxColorCount];
    
    // for (int i = 0; i < maxColorCount; i++)
    // {
    //     float4 cur = floatArrayTexture.SampleLevel(my_point_clamp_sampler, float2((i+.5) / maxColorCount, 0), 0);
    //     baseStartHeights[i] = cur.x;
    // }
    
    float heightPercent = inverseLerp(minHeight, maxHeight, worldPos.y);

    for (int i = 0; i < maxColorCount; i++)
    {
        float drawStrength = inverseLerp(-baseBlends[i] / 2 - epsilon, baseBlends[i] / 2, heightPercent - baseStartHeights[i]);
        Out = Out * (1 - drawStrength) + baseColors[i] * drawStrength;
    }
}

#endif