#ifndef MYFUNC
#define MYFUNC

float inverseLerp(float a, float b, float value)
{
    return saturate((value - a) / (b - a));
}

static const int baseColorCount = 3;
static const float3 baseColors[baseColorCount] = {float3(1, 0, 0), float3(0, 0, 1), float3(1, 1, 0)};
static const float baseStartHeights[baseColorCount] = {0, 0.29, 0.504};

void myFunc_float(float3 worldPos, float minHeight, float maxHeight, out float3 Out)
{
    float heightPercent = inverseLerp(minHeight, maxHeight, worldPos.y);

    for (int i = 0; i < baseColorCount; i++)
    {
        float drawStrength = saturate(sign(heightPercent - baseStartHeights[i]));
        Out = Out * (1 - drawStrength) + baseColors[i] * drawStrength;
    }
}

#endif