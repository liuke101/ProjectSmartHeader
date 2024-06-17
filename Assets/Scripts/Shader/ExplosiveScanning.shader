Shader "Custom/ExplosiveScanning"
{
    Properties
    {
        _MainTex ("MainTex", 2D) = "white" {}
        _BaseColor("BaseColor", Color) = (1,1,1,1)
        [Normal] _NormalMap("NormalMap", 2D) = "bump" {}
        _NormalScale("NormalScale", Range(0, 10)) = 1

        [Header(Specular)]
        _SpecularExp("SpecularExp", Range(1, 100)) = 32
        _SpecularStrength("SpecularStrength", Range(0, 10)) = 1
        _SpecularColor("SpecularColor", Color) = (1,1,1,1)
        
        [Header(Setting)]
        _ScanCenter("ScanCenter", Vector) = (0.0, 0.0, 0.0, 0.0)
        _ScanWidth("ScanWidth", Range(0, 100)) = 1
        _ScanRange("ScanRange", Range(0, 100)) = 5
        _MaxRange("MaxRange", Range(0, 100)) = 10
    }
    
    HLSLINCLUDE

    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/lighting.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
    
    CBUFFER_START(UnityPerMaterial)
    float4 _MainTex_ST;
    float4 _BaseColor;
    float _NormalScale;
    float _SpecularExp;
    float _SpecularStrength;
    float4 _SpecularColor;
    float4 _ScanCenter;
    float _ScanWidth;
    float _ScanRange;
    float _MaxRange;
    CBUFFER_END

    TEXTURE2D(_MainTex);
    SAMPLER(sampler_MainTex);
    TEXTURE2D(_NormalMap);
    SAMPLER(sampler_NormalMap);

    
    struct Attributes
    {
        float4 positionOS : POSITION;
        float4 color : COLOR;
        float3 normalOS : NORMAL;
        float4 tangentOS : TANGENT;
        float2 uv : TEXCOORD0;
    };

    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float4 color : COLOR0;
        float2 uv : TEXCOORD0;
        float3 positionWS: TEXCOORD1;
        float3 normalWS : TEXCOORD2;
        float4 tangentWS : TEXCOORD3;
        float3 bitangentWS : TEXCOORD4;
        float3 viewDirWS : TEXCOORD5;
        float3 lightDirWS : TEXCOORD6;
    };
    ENDHLSL
    
    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType"="Opaque" 
        }

        Pass
        {
            Tags
            {
                "LightMode"="UniversalForward"
            }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            Varyings vert(Attributes i)
            {
                Varyings o = (Varyings)0;
        
                VertexPositionInputs vertexInput = GetVertexPositionInputs(i.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(i.normalOS, i.tangentOS);

                o.uv = TRANSFORM_TEX(i.uv, _MainTex);
                o.positionCS = vertexInput.positionCS;
                o.positionWS = vertexInput.positionWS;

                //TBN
                o.normalWS = normalInput.normalWS;
                real sign = i.tangentOS.w * GetOddNegativeScale();
                o.tangentWS = float4(normalInput.tangentWS.xyz, sign);
                o.bitangentWS = normalInput.bitangentWS;
                
                o.viewDirWS = GetWorldSpaceNormalizeViewDir(o.positionWS);

                return o;
            }

            float4 frag(Varyings i) : SV_Target
            {
                //纹理采样
                float4 MainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                float3 normalMap = UnpackNormalScale(
                    SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, i.uv), _NormalScale);

                //用深度纹理和屏幕空间uv重建像素的世界空间位置
                //屏幕空间uv
                float2 ScreenUV = GetNormalizedScreenSpaceUV(i.positionCS);
                
                //从深度纹理中采样深度
                #if UNITY_REVERSED_Z
                    // 具有 REVERSED_Z 的平台（如 D3D）的情况。
                    float depth = SampleSceneDepth(ScreenUV);
                #else
                    // 没有 REVERSED_Z 的平台（如 OpenGL）的情况。
                    // 调整 Z 以匹配 OpenGL 的 NDC ([-1, 1])
                    float depth = lerp(UNITY_NEAR_CLIP_VALUE, 1, SampleSceneDepth(ScreenUV));
                #endif
                // 重建世界空间位置
               float3 rebuildPosWS = ComputeWorldSpacePosition(ScreenUV, depth, UNITY_MATRIX_I_VP);
                
                //转换成线性深度
                float linearDepth = Linear01Depth(depth,_ZBufferParams);
                
                float pixelDistance = distance(rebuildPosWS, _ScanCenter);
                if (_ScanRange - pixelDistance > 0 && linearDepth < 1)
                {
                real scanPercent = 1 - (_ScanRange - pixelDistance) / _ScanWidth;
                real maxPercent = 1 - (_MaxRange - pixelDistance) / _ScanWidth;
                real percent = lerp(1, 0, saturate(scanPercent / maxPercent));
                return percent + var_Screen;
                }
                
                return float4(rebuildPosWS, 1);
                
                //向量计算
                float3x3 TBN = float3x3(i.tangentWS.xyz, i.bitangentWS.xyz, i.normalWS.xyz);
                float3 N = TransformTangentToWorld(normalMap, TBN, true);
                float3 L = normalize(_MainLightPosition.xyz);
                float3 V = normalize(i.viewDirWS);
                float3 H = normalize(L + V);
                float NdotL = dot(N, L);
                float NdotH = dot(N, H);
                //颜色计算
                float3 diffuse = (0.5 * NdotL + 0.5) * _BaseColor.rgb * _MainLightColor;
                float3 specular = pow(max(0, NdotH), _SpecularExp) * _SpecularStrength * _SpecularColor.rgb * _MainLightColor;

                float4 finalColor = MainTex * float4((diffuse + _GlossyEnvironmentColor.rgb) + specular, 1);
                
                return finalColor;
            }
            ENDHLSL
        }
    }
    FallBack "Packages/com.unity.render-pipelines.universal/FallbackError"
}