Shader "Hidden/EasyFrameworkURP/GaussianBlur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Radius("Radius", float) = 1
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
                
        Pass
        {
            Name "UniversalForward"
            Tags{"LightMode" = "UniversalForward"}  
            HLSLPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            CBUFFER_START(UnityPerMaterial)
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            float _Radius;
            
            CBUFFER_END

            struct appdata
            {
                float4 positionOS   : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 positionHCS  : POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float4 FragGaussianBlur(float2 uv): SV_Target
            {
                float offset_x = _MainTex_TexelSize.x * _Radius;
                float offset_y = _MainTex_TexelSize.y * _Radius;

                // half count = 9 + 5 * 4 + 3 * 4 + 2 * 4;                
                // 0.4
                const float v1 = 0.4;
                float4 col = tex2D(_MainTex, uv) * v1;

                // 0.3
                const float v2 = 0.075;
                col += tex2D(_MainTex, uv + float2(offset_x, 0)) * v2;
                col += tex2D(_MainTex, uv + float2(-offset_x, 0)) * v2;
                col += tex2D(_MainTex, uv + float2(0, offset_y)) * v2;
                col += tex2D(_MainTex, uv + float2(0, -offset_y)) * v2;

                // 0.2
                const float v3 = 0.05;
                col += tex2D(_MainTex, uv + float2(offset_x, offset_y)) * v3;
                col += tex2D(_MainTex, uv + float2(offset_x, -offset_y)) * v3;
                col += tex2D(_MainTex, uv + float2(-offset_x, offset_y)) * v3;
                col += tex2D(_MainTex, uv + float2(-offset_x, -offset_y)) * v3;

                // 0.1
                const float v4 = 0.025;
                col += tex2D(_MainTex, uv + float2(offset_x * 2, 0)) * v4;
                col += tex2D(_MainTex, uv + float2(-offset_x * 2, 0)) * v4;
                col += tex2D(_MainTex, uv + float2(0, offset_y * 2)) * v4;
                col += tex2D(_MainTex, uv + float2(0, -offset_y * 2)) * v4;
                
                col.a = 1;
                return col;
            }
            
            half4 frag (v2f i) : SV_Target
            {
                return FragGaussianBlur(i.uv);
            }
            
            ENDHLSL
        }
    }
}
