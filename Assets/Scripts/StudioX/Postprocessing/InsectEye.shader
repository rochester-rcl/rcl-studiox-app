// Ported from https://www.shadertoy.com/view/MlXyDl

Shader "Hidden/Custom/InsectEye"
{
    HLSLINCLUDE
        #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"
        #define deg30 TWO_PI/12.0
        TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
        float _Blend;
        float _UVTile = 10.0;
        float _HexSeparation = 0.05;
        float _HexAspect = sqrt(3.0);
        float4 _HexColor = float4(1.0, 1.0, 1.0, 1.0);
        
        float Hex(float2 uv)
        {
            float2 u = 8 * uv/_ScreenParams.x;
            float2 s = float2(1, 1.732);
            float2 a = fmod(u,s) * 2 - s;
            float2 b = fmod(u+s*0.5, s) * 2 - s;
            return 0.5 * min(dot(a,a),dot(b,b));
        }

        float4 Frag(VaryingsDefault i): SV_Target
        {
            float hex = Hex(i.vertex.xy) * 0.5;
            float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
            return lerp(color, _HexColor, hex * _Blend);
        }

    ENDHLSL
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            HLSLPROGRAM
                #pragma vertex VertDefault
                #pragma fragment Frag
                // #pragma fragment FragWithSampling
            ENDHLSL
        }
    }
}