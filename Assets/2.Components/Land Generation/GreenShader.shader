Shader "Custom/LitGreenShader"
{
    Properties
    {
        _Color ("Base Color", Color) = (0, 1, 0, 1) // Green Color
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 200

        Pass
        {
            Tags { "LightMode"="ForwardBase" }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldNormal : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
            };

            float4 _Color; // Custom Green Color

            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Normalize normal
                float3 normal = normalize(i.worldNormal);

                // Get light direction (main directional light)
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);

                // Diffuse (Lambert) lighting calculation
                float diff = max(dot(normal, lightDir), 0.0);

                // Get light color (uses _LightColor0 in Built-in RP, otherwise fallback to white)
                float3 lightColor = UNITY_LIGHTMODEL_AMBIENT.rgb; // Ambient light for fallback

                #ifdef UNITY_PASS_FORWARDADD
                    lightColor = _LightColor0.rgb;
                #endif

                // Apply lighting to the green color
                float3 finalColor = _Color.rgb * lightColor * diff;

                return fixed4(finalColor, 1.0);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
