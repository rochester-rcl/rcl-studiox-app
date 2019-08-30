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

        float HexDistance(float2 a, float2 b)
        {
            float2 p = abs(b-a);
	        float s = sin(deg30);
	        float c = cos(deg30);
	
	        float diagDist = s*p.x + c*p.y;
	        return max(diagDist, p.x)/c;
        }

        float NearestHex(float s, float2 st)
        {
            float h = sin(deg30) * s;
            float r = cos(deg30) * s;
            float b = s + 2.0 * h;
            float a = 2.0 * r;
            float m = h / r;

            float2 sect = st / float2(2.0 * r, h + s);
            float2 sectPixel = fmod(st, float2(2.0 * r, h+s));

            float aSection = fmod(floor(sect.y), 2.0);

            float2 coord = floor(sect);

            if (aSection > 0.0)
            {
                if (sectPixel.y < (h-sectPixel.x*m))
                {
                    coord -= 1.0;
                } else if (sectPixel.y < (-h + sectPixel.x * m))
                {
                    coord.y -= 1.0;
                }
            }
            else 
            {
               if (sectPixel.x > r)
               {
                   if (sectPixel.y < (2.0 * h - sectPixel.x * m))
                   {
                       coord.y -= 1.0;
                   }
               }
               else
               {
                   if (sectPixel.y < (sectPixel.x * m))
                   {
                       coord.y -= 1.0;
                   }
                   else 
                   {
                       coord.x -= 1.0;
                   }
               } 
            }
            float xOffset = fmod(coord.y, 2.0) * r;
            return float2(coord.x * 2.0 * r - xOffset, coord.y * (h + s)) + float2(r * 2.0, s);
        }

        float4 Frag(VaryingsDefault i): SV_Target
        {
            float hex = Hex(i.vertex.xy) * 0.5;
            float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
            return lerp(color, _HexColor, hex * _Blend);
        }

        float4 FragWithSampling(VaryingsDefault i): SV_Target
        {
            float s = _ScreenParams.x / 80.0;
            float2 nearest = NearestHex(s, i.texcoordStereo * _ScreenParams.xy);
            float4 texel = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, nearest / _ScreenParams.xy);
            float hex = Hex(i.vertex.xy) * 0.5;
            float dist = HexDistance(i.vertex.yx, nearest);
            float interior = 1.0 - smoothstep(s-1.0, s, hex);
            return texel;
            // return float4(dist, dist, dist, dist);
            //return float4(dist, dist, dist, 1.0);
            /*float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
            float4 blended = lerp(texel, _HexColor, hex);
            return lerp(color, blended, _Blend);*/
            
        }

    ENDHLSL
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            HLSLPROGRAM
                #pragma vertex VertDefault
                // #pragma fragment Frag
                #pragma fragment FragWithSampling
            ENDHLSL
        }
    }
}