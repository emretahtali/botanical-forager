Shader "Custom/TerrainShader"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        float minHeight;
        float maxHeight;

        static const int maxLayerCount = 8;
        static const float epsilon = 1e-4;
        
        int layerCount;
        float3 baseColors[maxLayerCount];
        float baseStartHeights[maxLayerCount];
        float baseBlends[maxLayerCount];
        float baseColorStrength[maxLayerCount];
        float baseTextureScales[maxLayerCount];
        
        UNITY_DECLARE_TEX2DARRAY(baseTextures);
        
        struct Input
        {
            float3 worldPos;
            float3 worldNormal;
        };

        float inverseLerp(float a, float b, float value)
        {
            return saturate((value - a) / (b - a));
        }

        float3 triplanar(float3 worldPos, float scale, float3 blendAxes, int textureIndex)
        {
            float3 scaledWorldPos = worldPos / scale;
            float3 xProjection = UNITY_SAMPLE_TEX2DARRAY(baseTextures, float3(scaledWorldPos.y, scaledWorldPos.z, textureIndex)) * blendAxes.x;
            float3 yProjection = UNITY_SAMPLE_TEX2DARRAY(baseTextures, float3(scaledWorldPos.x, scaledWorldPos.z, textureIndex)) * blendAxes.y;
            float3 zProjection = UNITY_SAMPLE_TEX2DARRAY(baseTextures, float3(scaledWorldPos.x, scaledWorldPos.y, textureIndex)) * blendAxes.z;
            return xProjection + yProjection + zProjection;
        }
        
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float heightPercent = inverseLerp(minHeight, maxHeight, IN.worldPos.y);
            float3 blendAxes = abs(IN.worldNormal);
            blendAxes /= blendAxes.x + blendAxes.y + blendAxes.z;

            for (int i = 0; i < layerCount; i++)
            {
                float drawStrength = inverseLerp(-baseBlends[i] / 2 - epsilon, baseBlends[i] / 2, heightPercent - baseStartHeights[i]);
            
                float3 baseColor = baseColors[i] * baseColorStrength[i];
                float3 textureColor = triplanar(IN.worldPos, baseTextureScales[i], blendAxes, i) * (1 - baseColorStrength[i]);
                
                o.Albedo = o.Albedo * (1 - drawStrength) + (baseColor + textureColor) * drawStrength;
            }

            // o.Albedo = float3(0, 1, 0);
        }
        ENDCG
    }
    FallBack "Diffuse"
}
