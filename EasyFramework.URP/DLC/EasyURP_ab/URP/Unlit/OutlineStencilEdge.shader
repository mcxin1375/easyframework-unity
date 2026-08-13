Shader "EasyFrameworkURP/Unlit/OutlineStencilEdge"
{
    Properties
    {
        _BaseMap ("Base Map", 2D) = "white" {}
        _MainCol("Main Color", color) = (1,1,1,1)
        _OutlineCol("Outline Color", color) = (1,1,1,1)
        _OutlineWidth("Outline Width", range(0, 0.2)) = 0.005
        _Stencil ("Stencil", float) = 2
        
		_EdgeWidth ("Edge Width", range(0, 5)) = 1
		_EdgeForce ("Edge Force", range(0, 10)) = 1
		_EdgeOnly ("Edge Only", range(0, 1)) = 0
		_EdgeColor ("Edge Color", Color) = (0, 0, 0, 1)
		_BackgroundColor ("Background Color", Color) = (1, 1, 1, 1)
    	
        _Cull("__cull", Float) = 2.0
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode" = "UniversalForward" "RenderType"="Opaque" }
            Stencil{
                Ref [_Stencil]
                Comp NotEqual
                Pass Keep
            }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                half4 vertex : POSITION;
                half3 normal : NORMAL;
            };
            struct v2f
            {
                half4 positionHCS  : SV_POSITION;
            };

            CBUFFER_START(UnityPerMaterial)

            half4 _OutlineCol;
            float _OutlineWidth;

            CBUFFER_END
            
            v2f vert (appdata v)
            {
                v2f o;
                // float4 clipPos = TransformObjectToHClip(v.vertex);
                // float3 normal = mul((float3x3)unity_ObjectToWorld, v.normal);
                // normal = normalize(normal) * _OutlineWidth;
                v.vertex.xyz += v.normal * _OutlineWidth;
                o.positionHCS = TransformObjectToHClip(v.vertex);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                return _OutlineCol;
            }
            ENDHLSL
        }
        
        Pass
        {
            Name "SRPDefaultUnlit"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            Stencil{
                Ref [_Stencil]
                Comp Always
                Pass Replace
            }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                half4 vertex : POSITION;
                half2 uv : TEXCOORD0;
            };
            struct v2f
            {
                half4 positionHCS  : SV_POSITION;
                half2 uv[5]  : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
            
			TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            half4 _BaseMap_ST;
            half4 _MainCol;
            
            half4 _BaseMap_TexelSize;
            half _EdgeWidth;
            half _EdgeForce;
            half _EdgeOnly;
            half4 _EdgeColor;
            half4 _BackgroundColor;
            
            CBUFFER_END
            
            half luminance(half4 color) 
            {
				return  0.2125 * color.r + 0.7154 * color.g + 0.0721 * color.b; 
            }

            half Sobel(v2f i) 
            {
				const half Gx[5] = {-1,  1,
		                            -1,  1,
		                            0};
				const half Gy[5] = {-1, -1,
		                             1,  1,
		                             0};
            	
				half texColor;
				half edgeX = 0;
				half edgeY = 0;
		        for (int it = 0; it < 5; it++) 
	            {
				    texColor = luminance(SAMPLE_TEXTURE2D(_BaseMap , sampler_BaseMap , i.uv[it]));
				    // texColor = intensity(SAMPLE_TEXTURE2D(_BaseMap , sampler_BaseMap , i.uv[it]));
				    edgeX += texColor * Gx[it] * _EdgeForce;
				    edgeY += texColor * Gy[it] * _EdgeForce;
				}
				
				return 1 - abs(edgeX) - abs(edgeY);	
            	// return 1 - pow(edgeX * edgeX + edgeY * edgeY, 0.5);
		    }

            v2f vert (appdata v)
            {
                v2f o;
                o.positionHCS = TransformObjectToHClip(v.vertex);
                half2 uv = v.uv;
                o.uv[0] = uv + _BaseMap_TexelSize.xy * half2(1, 1) * _EdgeWidth;
                o.uv[1] = uv + _BaseMap_TexelSize.xy * half2(1, -1) * _EdgeWidth;
				o.uv[2] = uv + _BaseMap_TexelSize.xy * half2(-1, 1) * _EdgeWidth;
				o.uv[3] = uv + _BaseMap_TexelSize.xy * half2(-1, -1) * _EdgeWidth;
				o.uv[4] = uv + _BaseMap_TexelSize.xy * half2(0, 0);
            	
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half edge = Sobel(i);		
				half4 withEdgeColor = lerp(_EdgeColor, SAMPLE_TEXTURE2D(_BaseMap , sampler_BaseMap , i.uv[4]) * _MainCol, edge);
				half4 onlyEdgeColor = lerp(_EdgeColor, _BackgroundColor, edge);
                return  lerp(withEdgeColor, onlyEdgeColor, _EdgeOnly);
            	
                // half4 col = SAMPLE_TEXTURE2D(_BaseMap , sampler_BaseMap , i.uv[4]);
                // return col * _MainCol;
            }
            ENDHLSL
        }
    	
    	Pass
        {
            Name "ShadowCaster"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }

            // -------------------------------------
            // Render State Commands
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 2.0

            // -------------------------------------
            // Shader Stages
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            // -------------------------------------
            // Universal Pipeline keywords

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile_fragment _ LOD_FADE_CROSSFADE

            // This is used during shadow map generation to differentiate between directional and punctual light shadows, as they use different formulas to apply Normal Bias
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            // -------------------------------------
            // Includes
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }
    	
        Pass
        {
            Name "DepthOnly"
            Tags
            {
                "LightMode" = "DepthOnly"
            }

            // -------------------------------------
            // Render State Commands
            ZWrite On
            ColorMask R
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 2.0

            // -------------------------------------
            // Shader Stages
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile_fragment _ LOD_FADE_CROSSFADE

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            // -------------------------------------
            // Includes
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }

        // This pass is used when drawing to a _CameraNormalsTexture texture
        Pass
        {
            Name "DepthNormals"
            Tags
            {
                "LightMode" = "DepthNormals"
            }

            // -------------------------------------
            // Render State Commands
            ZWrite On
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 2.0

            // -------------------------------------
            // Shader Stages
            #pragma vertex DepthNormalsVertex
            #pragma fragment DepthNormalsFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local _PARALLAXMAP
            #pragma shader_feature_local _ _DETAIL_MULX2 _DETAIL_SCALED
            #pragma shader_feature_local _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile_fragment _ LOD_FADE_CROSSFADE

            // -------------------------------------
            // Universal Pipeline keywords
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            // -------------------------------------
            // Includes
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitDepthNormalsPass.hlsl"
            ENDHLSL
        }

    	
    }
}
