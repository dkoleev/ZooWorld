Shader "ZooWorld/Toon"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        _ShadeColor ("Shade Tint", Color) = (0.55, 0.62, 0.85, 1)
        _ShadeThreshold ("Shade Threshold", Range(-1, 1)) = 0.1
        _ShadeSoftness ("Shade Softness", Range(0.001, 0.5)) = 0.03
        _RimColor ("Rim Color", Color) = (1, 1, 1, 1)
        _RimThreshold ("Rim Threshold", Range(0, 1)) = 0.65
        _RimStrength ("Rim Strength", Range(0, 1)) = 0.3
        _OutlineColor ("Outline Color", Color) = (0.13, 0.1, 0.16, 1)
        _OutlineWidth ("Outline Width (world units)", Range(0, 0.1)) = 0.02
        _WiggleAmplitude ("Wiggle Amplitude", Range(0, 0.5)) = 0
        _WiggleFrequency ("Wiggle Frequency (rad per unit)", Range(0, 20)) = 4.65
        [HideInInspector] _WigglePhase ("Wiggle Phase (distance travelled)", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        // One buffer shared by every pass keeps the shader SRP Batcher compatible.
        CBUFFER_START(UnityPerMaterial)
            half4 _BaseColor;
            half4 _ShadeColor;
            half4 _RimColor;
            half4 _OutlineColor;
            half _ShadeThreshold;
            half _ShadeSoftness;
            half _RimThreshold;
            half _RimStrength;
            half _OutlineWidth;
            half _WiggleAmplitude;
            float _WiggleFrequency;
            float _WigglePhase;
        CBUFFER_END

        // Bends the mesh sideways along a sine. _WigglePhase is the distance travelled, set per renderer
        // by WigglePhase, so the body slides through the wave and freezes when the animal stops.
        void ApplyWiggle(inout float3 positionOS, inout float3 normalOS)
        {
            float s, c;
            sincos((_WigglePhase + positionOS.z) * _WiggleFrequency, s, c);
            positionOS.x += _WiggleAmplitude * s;
            // Inverse transpose of the shear x += f(z).
            normalOS.z -= _WiggleAmplitude * _WiggleFrequency * c * normalOS.x;
            normalOS = normalize(normalOS);
        }
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                half4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half3 normalWS : TEXCOORD1;
                half fogFactor : TEXCOORD2;
                half4 color : COLOR;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                ApplyWiggle(input.positionOS.xyz, input.normalOS);
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                output.color = input.color;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half3 normalWS = normalize(input.normalWS);
                Light light = GetMainLight(TransformWorldToShadowCoord(input.positionWS));

                // Two bands, lit or shaded, with a thin soft edge between them.
                half lit = smoothstep(_ShadeThreshold - _ShadeSoftness, _ShadeThreshold + _ShadeSoftness,
                    dot(normalWS, light.direction));
                lit *= smoothstep(0.5 - _ShadeSoftness, 0.5 + _ShadeSoftness, light.shadowAttenuation);

                half3 albedo = _BaseColor.rgb * input.color.rgb;
                half3 color = albedo * light.color * lerp(_ShadeColor.rgb, half3(1, 1, 1), lit);

                // A hard-edged highlight along the silhouette, on the lit side only.
                half rim = 1.0 - saturate(dot(normalWS, GetWorldSpaceNormalizeViewDir(input.positionWS)));
                color += _RimColor.rgb * (smoothstep(_RimThreshold, _RimThreshold + 0.05, rim) * lit * _RimStrength);

                return half4(MixFog(color, input.fogFactor), 1);
            }
            ENDHLSL
        }

        // Inverted hull: the back faces, pushed outward, show as a line around the object.
        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            Cull Front

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float3 smoothNormalOS : TEXCOORD3;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half fogFactor : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                // Generated meshes carry smoothed normals in UV3; any other mesh falls back to its own.
                float3 normalOS = dot(input.smoothNormalOS, input.smoothNormalOS) > 0.0001
                    ? input.smoothNormalOS
                    : input.normalOS;
                ApplyWiggle(input.positionOS.xyz, normalOS);
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz)
                    + TransformObjectToWorldNormal(normalOS) * _OutlineWidth;

                Varyings output;
                output.positionCS = TransformWorldToHClip(positionWS);
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                return half4(MixFog(_OutlineColor.rgb, input.fogFactor), 1);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            float3 _LightDirection;
            float3 _LightPosition;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            float4 Vert(Attributes input) : SV_POSITION
            {
                ApplyWiggle(input.positionOS.xyz, input.normalOS);
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
            #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                float3 lightDirectionWS = normalize(_LightPosition - positionWS);
            #else
                float3 lightDirectionWS = _LightDirection;
            #endif
                return ApplyShadowClamping(TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS)));
            }

            half4 Frag() : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On
            ColorMask R

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            float4 Vert(float4 positionOS : POSITION) : SV_POSITION
            {
                float3 unusedNormalOS = float3(0, 1, 0);
                ApplyWiggle(positionOS.xyz, unusedNormalOS);
                return TransformObjectToHClip(positionOS.xyz);
            }

            half Frag(float4 positionCS : SV_POSITION) : SV_Target
            {
                return positionCS.z;
            }
            ENDHLSL
        }

        // The renderer's ambient occlusion feature reads normals from this pass.
        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }
            ZWrite On

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half3 normalWS : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                ApplyWiggle(input.positionOS.xyz, input.normalOS);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                return half4(normalize(input.normalWS), 0);
            }
            ENDHLSL
        }
    }
}
