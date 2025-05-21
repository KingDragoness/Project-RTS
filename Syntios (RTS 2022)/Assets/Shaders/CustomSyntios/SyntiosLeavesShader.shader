Shader "Syntios/SyntiosLeavesShader"
{
    Properties
    {
        [HDR] _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,2)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Cutoff("Alpha cutoff", Range(0,1)) = 0.5
        _BumpMap("Bumpmap", 2D) = "bump" {}

        //leaves
        _Direction("Direction", Vector) = (1.0,0.0,0.0,1.0)
        _Steepness ("Steepness", Range(0.1, 1.0)) = 0.5
        _Freq ("Frequency", Range (0.1, 10.0)) = 1.0

    }
    SubShader
    {
            Tags {"Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="TransparentCutout"}



        CGPROGRAM
        // Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
        #pragma exclude_renderers gles
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard vertex:vert fullforwardshadows alphatest:_Cutoff



        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0
        #include "UnityCG.cginc"

        //Wave Properties
        float _Steepness, _Freq; 
        float4 _Direction;

        sampler2D _MainTex;
        sampler2D _BumpMap;
        uniform sampler2D _FOWMap;
        SamplerState sampler_SplatMap;
        uniform sampler2D _CloudFog;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_BumpMap;
            float3 worldPos;
        };

        half _Glossiness;
        half _Metallic;
        uniform half _FOWSampleRadiusBlur;
        fixed4 _Color;
        uniform fixed _SplatmapScale;
        uniform fixed _FOWmapScale;
        uniform float2 _MapSize;
        uniform float _BorderMap;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        float2 hash2D2D(float2 s)
        {
            //magic numbers
            return frac(sin(fmod(float2(dot(s, float2(127.1, 311.7)), dot(s, float2(269.5, 183.3))), 3.14159)) * 43758.5453);
        }

        float4 tex2DStochastic(sampler2D tex, float2 UV)
        {
            //triangle vertices and blend weights
            //BW_vx[0...2].xyz = triangle verts
            //BW_vx[3].xy = blend weights (z is unused)
            float4x3 BW_vx;

            //uv transformed into triangular grid space with UV scaled by approximation of 2*sqrt(3)
            float2 skewUV = mul(float2x2 (1.0, 0.0, -0.57735027, 1.15470054), UV * 3.464);

            //vertex IDs and barycentric coords
            float2 vxID = float2 (floor(skewUV));
            float3 barry = float3 (frac(skewUV), 0);
            barry.z = 1.0 - barry.x - barry.y;

            BW_vx = ((barry.z > 0) ?
                float4x3(float3(vxID, 0), float3(vxID + float2(0, 1), 0), float3(vxID + float2(1, 0), 0), barry.zyx) :
                float4x3(float3(vxID + float2 (1, 1), 0), float3(vxID + float2 (1, 0), 0), float3(vxID + float2 (0, 1), 0), float3(-barry.z, 1.0 - barry.y, 1.0 - barry.x)));

            //calculate derivatives to avoid triangular grid artifacts
            float2 dx = ddx(UV);
            float2 dy = ddy(UV);

            //blend samples with calculated weights
            return mul(tex2D(tex, UV + hash2D2D(BW_vx[0].xy), dx, dy), BW_vx[3].x) +
                mul(tex2D(tex, UV + hash2D2D(BW_vx[1].xy), dx, dy), BW_vx[3].y) +
                mul(tex2D(tex, UV + hash2D2D(BW_vx[2].xy), dx, dy), BW_vx[3].z);
        }
        void vert(inout appdata_full v, out Input o) 
        {
            //Wave displacement
            {
                //To begin, obtain the position of each vertex and normalize the wave direction

                float3 pos = v.vertex.xyz;
                float4 dir = normalize(_Direction);

                //The full length of a sine wave is 2PI
                float defaultWavelength = 2 * UNITY_PI;

                float wL = defaultWavelength / _Freq;
                float phase = sqrt(9.8 / wL);
                float disp = wL * (dot(dir, pos) - (phase * _Time.y));
                float peak = _Steepness/wL;

                pos.x += dir.x * (peak * cos(disp));
                pos.y += peak * sin(disp);
                pos.z += dir.y * (peak * cos(disp));
                v.vertex.xyz = pos;
            }
            
            

            UNITY_INITIALIZE_OUTPUT(Input, o);
            //o.uvFOW = o.worldPos.xz;// 
        }


        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            float4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color * 2;
            float fowPow = 1;

            //FOG OF WAR
            {
                float2 _fowUV = IN.worldPos.xz * _FOWmapScale * _SplatmapScale * 0.01;
                fixed3 sum = fixed3(0, 0, 0);
                sum = tex2D(_FOWMap, _fowUV).rgb;

                int totalLength = 3;
                int totalSample = (totalLength * totalLength) * (totalLength * totalLength);
                for (int x = -(totalSample / totalLength / totalLength); x < (totalSample / totalLength / totalLength); x++)
                {
                    sum += tex2D(_FOWMap, half2(_fowUV.x - x * _FOWSampleRadiusBlur, _fowUV.y + x * _FOWSampleRadiusBlur));
                }
                for (int x = -(totalSample / totalLength / totalLength); x < (totalSample / totalLength / totalLength); x++)
                {
                    sum += tex2D(_FOWMap, half2(_fowUV.x - x * _FOWSampleRadiusBlur, _fowUV.y - x * _FOWSampleRadiusBlur));
                }

                sum += tex2D(_FOWMap, half2(_fowUV.x, _fowUV.y - _FOWSampleRadiusBlur));
                sum += tex2D(_FOWMap, half2(_fowUV.x, _fowUV.y + _FOWSampleRadiusBlur));

                sum /= (totalSample + 3);

                float2 fogFOW = _fowUV * 11;
                float2 fogFOW_1 = _fowUV * 4.44;
                fogFOW.x += _Time * 1;
                fogFOW.y += _Time * 0.8;
                fogFOW_1.x += _Time * 1.3;
                fogFOW_1.y += _Time * -0.9;

                sum.r += tex2DStochastic(_CloudFog, fogFOW).r;
                //sum.r += tex2DStochastic(_CloudFog, fogFOW_1).r;

                c.rgb *= sum.r * 1.1;
                //fowPow = sum.r * 1.2;

            }

            //WORLD BORDER
            {
                float x_border_col = (_BorderMap - IN.worldPos.r / 2) / _BorderMap;
                float x_border_end = (_MapSize.r - IN.worldPos.r / 2) / _BorderMap;
                float y_border_col = (_BorderMap - IN.worldPos.b / 2) / _BorderMap;
                float y_border_end = (_MapSize.g - IN.worldPos.b / 2) / _BorderMap;

                x_border_col = 1 - x_border_col;
                y_border_col = 1 - y_border_col;
                x_border_end = x_border_end;
                y_border_end = y_border_end;

                x_border_col *= x_border_col;
                y_border_col *= y_border_col;
                x_border_end *= x_border_end;
                y_border_end *= y_border_end;

                x_border_col = clamp(x_border_col, 0, 1);
                y_border_col = clamp(y_border_col, 0, 1);
                x_border_end = clamp(x_border_end, 0, 1);
                y_border_end = clamp(y_border_end, 0, 1);

                if (IN.worldPos.r / 2 < _BorderMap)
                {
                    c.r *= x_border_col;
                    c.g *= x_border_col;
                    c.b *= x_border_col;
                }
                if (IN.worldPos.b / 2 < _BorderMap)
                {
                    c.r *= y_border_col;
                    c.g *= y_border_col;
                    c.b *= y_border_col;
                }
                if (IN.worldPos.r / 2 > _MapSize.r - _BorderMap)
                {
                    c.r *= x_border_end;
                    c.g *= x_border_end;
                    c.b *= x_border_end;

                    if (IN.worldPos.r / 2 > _MapSize.r)
                    {
                        c.r = 0;
                        c.g = 0;
                        c.b = 0;
                    }
                }
                if (IN.worldPos.b / 2 > _MapSize.g - _BorderMap)
                {
                    c.r *= y_border_end;
                    c.g *= y_border_end;
                    c.b *= y_border_end;

                    if (IN.worldPos.b / 2 > _MapSize.g)
                    {
                        c.r = 0;
                        c.g = 0;
                        c.b = 0;
                    }
                }
            }


            {
                
            }

            c.rgb *= 0 + (2) + c;   

            o.Albedo = c.rgb;
            o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));

            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness * fowPow;
            o.Alpha = tex2D (_MainTex, IN.uv_MainTex).a;


        }
        ENDCG
    }
    Fallback "Legacy Shaders/Transparent/Cutout/VertexLit"
}
