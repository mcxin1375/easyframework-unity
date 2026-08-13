Shader "EasyFrameworkURP/Unlit/OutlineStencilMask"
{
    Properties
    {
        _MainCol("Main Color", color) = (1,1,1,1)
        _OutlineWidth("Outline Width", range(0, 0.2)) = 0.005
        _StencilValue("StencilValue", Float) = 2.0
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType"="Opaque" "Queue" = "Geometry+1" }

        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode" = "UniversalForward" }
            Stencil{
                Ref [_StencilValue]
                Comp NotEqual
                Pass Keep
            }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };
            struct v2f
            {
                float4 vertex  : SV_POSITION;
            };

            CBUFFER_START(UnityPerMaterial)
            
            float4 _MainCol;
            float _OutlineWidth;
            
            CBUFFER_END

            v2f vert (appdata v)
            {
                v2f o;
                v.vertex.xyz += v.normal * _OutlineWidth;
                o.vertex = TransformObjectToHClip(v.vertex);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                clip(_OutlineWidth - 0.00001f);
                return _MainCol;
            }
            ENDHLSL
        }
        
    }
}
