// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Syntios/SyntiosTerrain"
{
    Properties
    {
        _SplatMap("Splatmap (g,1,2,3,4)", 2D) = "black" {}
        _SplatMap2("Splatmap (5,6,7,8)", 2D) = "black" {}

        //_FOWMap("Fog of War (Ever Explored)", 2D) = "black" {}
        _GroundTexture("Ground Texture", 2D) = "white" {}
        _CloudFog("Cloud Fog Pattern", 2D) = "black" {}
        _UnexploredFog("Unexplored Fog Color", Color) = (0.1,0.1,0.1,1)
        _BumpDepth("Bump Depth", Range(-2.0, 2.0)) = 1

        _TextureA("Layer 1", 2D) = "white" {}
        _NormalMapA("NormalMap 1", 2D) = "white" {}

        _TextureB("Layer 2", 2D) = "white" {}
        _NormalMapB("NormalMap 2", 2D) = "white" {}

        _TextureC("Layer 3", 2D) = "white" {}
        _NormalMapC("NormalMap 3", 2D) = "white" {}

        _TextureD("Layer 4", 2D) = "white" {}
        _NormalMapD("NormalMap 4", 2D) = "white" {}

        _TextureE("Layer 5", 2D) = "white" {}
        _NormalMapE("NormalMap 5", 2D) = "white" {}

        _TextureF("Layer 6", 2D) = "white" {}
        _NormalMapF("NormalMap 6", 2D) = "white" {}

        _TextureG("Layer 7", 2D) = "white" {}
        _NormalMapG("NormalMap 7", 2D) = "white" {}

        _TextureH("Layer 8", 2D) = "white" {}
        _NormalMapH("NormalMap 8", 2D) = "white" {}

        _TextureScale("TextureScale", Range(0.01,10)) = 0.25
        _SplatmapScale("SplatmapScale", Range(0.01,2)) = 0.5
        _FOWmapScale("FOWScale", Range(0.1,4)) = 1
        _FOWSampleRadiusBlur("FOW Blur Radius", Range(0.0,0.05)) = 0.005
        _PrioGround("Prio Ground", Range(0.01, 2.0)) = 1
        _PrioA("Prio layer1", Range(0.01, 2.0)) = 1
        _PrioB("Prio layer2", Range(0.01, 2.0)) = 1
        _PrioC("Prio layer3", Range(0.01, 2.0)) = 1
        _PrioD("Prio layer4", Range(0.01, 2.0)) = 1
        _PrioE("Prio layer1", Range(0.01, 2.0)) = 1
        _PrioF("Prio layer2", Range(0.01, 2.0)) = 1
        _PrioG("Prio layer3", Range(0.01, 2.0)) = 1
        _PrioH("Prio layer4", Range(0.01, 2.0)) = 1
        _ContrastCloud("Cloud Contrast", Range(0.001, 1.0)) = 0.01
        _Depth("Depth", Range(0.01,1.0)) = 0.2

        _SpecColor("Specular Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _Shininess("Shininess", float) = 10
        _RimColor("Rim Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _RimPower("Rim Power", Range(0.1, 10.0)) = 3.0
        _AlbedoPower("Albedo Power", Range(1, 10.0)) = 1
        [ShowAsVector2]_MapSize("MapSize", Vector) = (.25, .5, .5, 1)
        _BorderMap("Map Border", Range(4, 32)) = 4.0
    }

    SubShader
    {
        // Set Queue to AlphaTest+2 to render the terrain after all other solid geometry.
        // We do this because the terrain shader is expensive and this way we ensure most pixels
        // are already discarded before the fragment shader is executed:
        Tags{ "Queue" = "AlphaTest+2"}
        Pass
        {
            Tags{ "LightMode" = "ForwardBase" }


            CGPROGRAM
             #pragma target 3.0

        // Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
        #pragma exclude_renderers gles
        #pragma vertex vert
        #pragma fragment frag

        // Make realtime shadows work
        #pragma multi_compile_fwdbase
        // Skip unnessesary shader variants
        #pragma skip_variants DIRLIGHTMAP_COMBINED LIGHTPROBE_SH POINT SPOT SHADOWS_DEPTH SHADOWS_CUBE VERTEXLIGHT_ON


        #include "UnityCG.cginc"
        #include "UnityLightingCommon.cginc"
        #include "AutoLight.cginc"

        Texture2D _SplatMap;
        Texture2D _SplatMap2;
        Texture2D _CloudFog;
        uniform Texture2D _FOWMap;
        SamplerState sampler_SplatMap;

        sampler2D _GroundTexture;
        sampler2D _TextureA;
        sampler2D _TextureB;
        sampler2D _TextureC;
        sampler2D _TextureD;
        sampler2D _TextureE;
        sampler2D _TextureF;
        sampler2D _TextureG;
        sampler2D _TextureH;

        Texture2D _NormalMapA;
        Texture2D _NormalMapB;
        Texture2D _NormalMapC;
        Texture2D _NormalMapD;
        Texture2D _NormalMapE;
        Texture2D _NormalMapF;
        Texture2D _NormalMapG;
        Texture2D _NormalMapH;
        SamplerState sampler_NormalMapA;


        fixed _TextureScale;
        fixed _SplatmapScale;
        fixed _FOWmapScale;
        half _FOWSampleRadiusBlur;
        uniform float4 _UnexploredFog;

        uniform float _BumpDepth;
        uniform float _Shininess;
        uniform float4 _RimColor;
        uniform float _RimPower;
        uniform float _AlbedoPower;
        uniform float2 _MapSize;
        uniform float _BorderMap;

        fixed _PrioGround;
        fixed _PrioA;
        fixed _PrioB;
        fixed _PrioC;
        fixed _PrioD;
        fixed _PrioE;
        fixed _PrioF;
        fixed _PrioG;
        fixed _PrioH;
        fixed _ContrastCloud;

        fixed _Depth;

        struct a2v
        {
            float4 vertex : POSITION;
            fixed3 normal : NORMAL;
            float3 tangent : TANGENT;
            fixed4 color : COLOR;
            float3 uv : TEXCOORD0;
        };

        struct v2f
        {
            float4 pos : SV_POSITION;
            float2 uvSplat : TEXCOORD0;
            float2 uvMaterial : TEXCOORD1;
            fixed4 materialPrios : TEXCOORD2;
            float2 uvFOW : TEXCOORD4;

            float4 posWorld: TEXCOORD5;
            float3 normalWorld: TEXCOORD6;
            float3 tangentWorld: TEXCOORD7;
            float3 binormalWorld: TEXCOORD8;
            fixed4 materialPrios2 : TEXCOORD9;

            // put shadows data into TEXCOORD3
            SHADOW_COORDS(3)
            fixed4 color : COLOR0;
            fixed3 diff : COLOR1;
            fixed3 ambient : COLOR2;
        };

        v2f vert(a2v v)
        {
            v2f OUT;
            OUT.pos = UnityObjectToClipPos(v.vertex);
            //OUT.uvSplat = v.uv.xy;
            OUT.uvSplat = mul(unity_ObjectToWorld, v.vertex).xz * _SplatmapScale * 0.01; //replaced with global
            OUT.uvFOW = mul(unity_ObjectToWorld, v.vertex).xz * _FOWmapScale * _SplatmapScale * 0.01; //replaced with global

            //for normalmap
            {
                OUT.normalWorld = normalize(mul(float4(v.normal, 0.0), unity_WorldToObject).xyz);
                OUT.tangentWorld = normalize(mul(unity_ObjectToWorld, v.tangent).xyz);
                OUT.binormalWorld = normalize(cross(OUT.normalWorld, OUT.tangentWorld));
                OUT.posWorld = mul(unity_ObjectToWorld, v.vertex);
            }

            // uvs of the rendered materials are based on world position
            OUT.uvMaterial = mul(unity_ObjectToWorld, v.vertex).xz * _TextureScale;
            OUT.materialPrios = fixed4(_PrioA, _PrioB, _PrioC, _PrioD);
            OUT.materialPrios2 = fixed4(_PrioE, _PrioF, _PrioG, _PrioH);
            OUT.color = v.color;

            // calculate light
            half3 worldNormal = UnityObjectToWorldNormal(v.normal);
            half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
            OUT.diff = nl * _LightColor0.rgb;
            OUT.ambient = ShadeSH9(half4(worldNormal,1));

            // Transfer shadow coordinates:
            TRANSFER_SHADOW(OUT);

            return OUT;
        }

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

        float4 tex2DStochastic(Texture2D<float4> tex, float2 UV, SamplerState ss)
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
            return mul(tex.Sample(ss, UV + hash2D2D(BW_vx[0].xy)), BW_vx[3].x) +
                mul(tex.Sample(ss, UV + hash2D2D(BW_vx[1].xy)), BW_vx[3].y) +
                mul(tex.Sample(ss, UV + hash2D2D(BW_vx[2].xy)), BW_vx[3].z);
        }

        fixed4 frag(v2f IN) : SV_Target
        {



            fixed4 groundColor = tex2DStochastic(_GroundTexture, IN.uvMaterial);
            fixed4 materialAColor = tex2DStochastic(_TextureA, IN.uvMaterial);
            fixed4 materialBColor = tex2DStochastic(_TextureB, IN.uvMaterial);
            fixed4 materialCColor = tex2DStochastic(_TextureC, IN.uvMaterial);
            fixed4 materialDColor = tex2DStochastic(_TextureD, IN.uvMaterial);
            fixed4 materialEColor = tex2DStochastic(_TextureE, IN.uvMaterial);
            fixed4 materialFColor = tex2DStochastic(_TextureF, IN.uvMaterial);
            fixed4 materialGColor = tex2DStochastic(_TextureG, IN.uvMaterial);
            fixed4 materialHColor = tex2DStochastic(_TextureH, IN.uvMaterial);

            // store heights for all materials on this pixel
            fixed groundHeight = groundColor.a;
            fixed4 materialHeights = fixed4(materialAColor.a, materialBColor.a, materialCColor.a, materialDColor.a);
            fixed4 materialHeightsb = fixed4(materialEColor.a, materialFColor.a, materialGColor.a, materialHColor.a);


            // avoid black artefacts by division by zero
            materialHeights = max(0.0001, materialHeights);
            materialHeightsb = max(0.0001, materialHeightsb);

            // get material amounts from splatmap
            fixed4 materialAmounts = _SplatMap.Sample(sampler_SplatMap, IN.uvSplat).argb; // tex2D(_SplatMap, IN.uvSplat).argb;
            fixed4 materialAmounts2 = _SplatMap2.Sample(sampler_SplatMap, IN.uvSplat).argb; // tex2D(_SplatMap, IN.uvSplat).argb;

            // the ground amount takes up all unused space
            fixed groundAmount = 1.0 - min(1.0, materialAmounts.r + materialAmounts.g + materialAmounts.b + materialAmounts.a +
                materialAmounts2.r + materialAmounts2.g + materialAmounts2.b + materialAmounts2.a);

            // calculate material strenghts
            fixed alphaGround = groundAmount * _PrioGround * groundHeight;
            fixed4 alphaMaterials = materialAmounts * IN.materialPrios * materialHeights;
            fixed4 alphaMaterials2 = materialAmounts2 * IN.materialPrios2 * materialHeightsb;

            // find strongest point of all materials
            fixed max_01234 = max(alphaGround, alphaMaterials.r);
            max_01234 = max(max_01234, alphaMaterials.g);
            max_01234 = max(max_01234, alphaMaterials.b);
            max_01234 = max(max_01234, alphaMaterials.a);
            max_01234 = max(max_01234, alphaMaterials2.r);
            max_01234 = max(max_01234, alphaMaterials2.g);
            max_01234 = max(max_01234, alphaMaterials2.b);
            max_01234 = max(max_01234, alphaMaterials2.a);

            //lower threshold
            max_01234 = max(max_01234 - _Depth, 0);

            // mask all materials above threshold
            fixed b0 = max(alphaGround - max_01234, 0);
            fixed b1 = max(alphaMaterials.r - max_01234, 0);
            fixed b2 = max(alphaMaterials.g - max_01234, 0);
            fixed b3 = max(alphaMaterials.b - max_01234, 0);
            fixed b4 = max(alphaMaterials.a - max_01234, 0);
            fixed b5 = max(alphaMaterials2.r - max_01234, 0);
            fixed b6 = max(alphaMaterials2.g - max_01234, 0);
            fixed b7 = max(alphaMaterials2.b - max_01234, 0);
            fixed b8 = max(alphaMaterials2.a - max_01234, 0);

            // combine all materials and normalize
            fixed alphaSum = b0 + b1 + b2 + b3 + b4 + b5 + b6 + b7 + b8;
            fixed4 col = (
                groundColor * b0 +
                materialAColor * b1 +
                materialBColor * b2 +
                materialCColor * b3 +
                materialDColor * b4 +
                materialEColor * b5 +
                materialFColor * b6 +
                materialGColor * b7 +
                materialHColor * b8
            ) / alphaSum;

            //include vertex colors
            col *= IN.color;

            // compute shadow attenuation (1.0 = fully lit, 0.0 = fully shadowed)
            fixed shadow = SHADOW_ATTENUATION(IN);
            // darken light's illumination with shadow, keep ambient intact
            fixed3 lighting = IN.diff * shadow + IN.ambient;

            col.rgb *= IN.diff * SHADOW_ATTENUATION(IN) + IN.ambient;


            //FOG OF WAR
            {
                fixed3 sum = fixed3(0, 0, 0);

                sum += _FOWMap.Sample(sampler_SplatMap, half2(IN.uvFOW.x, IN.uvFOW.y)); //tex2D(_FOWMap, half2(IN.uvFOW.x, IN.uvFOW.y)) * 1;

                int totalLength = 3;
                int totalSample = (totalLength * totalLength) * (totalLength * totalLength);
                for (int x = -(totalSample / totalLength / totalLength); x < (totalSample / totalLength / totalLength); x++)
                {
                    sum += _FOWMap.Sample(sampler_SplatMap, half2(IN.uvFOW.x - x * _FOWSampleRadiusBlur, IN.uvFOW.y + x * _FOWSampleRadiusBlur)); //tex2D(_FOWMap, half2(IN.uvFOW.x - x * _FOWSampleRadiusBlur, IN.uvFOW.y + x * _FOWSampleRadiusBlur));
                }
                for (int x = -(totalSample / totalLength / totalLength); x < (totalSample / totalLength / totalLength); x++)
                {
                    sum += _FOWMap.Sample(sampler_SplatMap, half2(IN.uvFOW.x - x * _FOWSampleRadiusBlur, IN.uvFOW.y - x * _FOWSampleRadiusBlur)); //tex2D(_FOWMap, half2(IN.uvFOW.x - x * _FOWSampleRadiusBlur, IN.uvFOW.y - x * _FOWSampleRadiusBlur));
                }

                sum += _FOWMap.Sample(sampler_SplatMap, half2(IN.uvFOW.x, IN.uvFOW.y - _FOWSampleRadiusBlur)); //tex2D(_FOWMap, half2(IN.uvFOW.x, IN.uvFOW.y - _FOWSampleRadiusBlur));
                sum += _FOWMap.Sample(sampler_SplatMap, half2(IN.uvFOW.x, IN.uvFOW.y + _FOWSampleRadiusBlur)); //tex2D(_FOWMap, half2(IN.uvFOW.x, IN.uvFOW.y + _FOWSampleRadiusBlur));

 /*               sum += tex2D(_FOWMap, half2(IN.uvFOW.x - _FOWSampleRadiusBlur, IN.uvFOW.y));
                sum += tex2D(_FOWMap, half2(IN.uvFOW.x + _FOWSampleRadiusBlur, IN.uvFOW.y));*/


                sum /= (totalSample + 4);
                sum.r = clamp(sum.r, _UnexploredFog.r, 1);

                fixed3 fogReveal = _FOWMap.Sample(sampler_SplatMap, half2(IN.uvFOW.x, IN.uvFOW.y)); //tex2D(_FOWMap, half2(IN.uvFOW.x, IN.uvFOW.y));



                float powReveal = 1; //fogReveal.r;

                //if (fogReveal.r < 0.94)
                {
                    float2 fogFOW = IN.uvFOW * 11;
                    float2 fogFOW_1 = IN.uvFOW * 4.44;
                    fogFOW.x += _Time * 1;
                    fogFOW.y += _Time * 0.8;
                    fogFOW_1.x += _Time * 1.3;
                    fogFOW_1.y += _Time * -0.9;
                    sum.r += tex2DStochastic(_CloudFog, fogFOW, sampler_SplatMap).r * _ContrastCloud * 0.55 * powReveal;
                    sum.r += tex2DStochastic(_CloudFog, fogFOW_1, sampler_SplatMap).r * _ContrastCloud * 0.45 * powReveal;

                }



                sum.g = sum.r; //forcing greyscale
                sum.b = sum.r;

                col.rgb *= sum;
            }

            //normalmap
            {
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - IN.posWorld.xyz);
                float3 lightDirection;
                float atten;

                if (_WorldSpaceLightPos0.w == 0.0)
                { // Directional Light
                    atten = 1.0;
                    lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                }
                else
                {
                    float3 fragmentToLightSource = _WorldSpaceLightPos0.xyz - IN.posWorld.xyz;
                    float distance = length(fragmentToLightSource);
                    float atten = 1 / distance;
                    lightDirection = normalize(fragmentToLightSource);
                }

                float4 tex = col;
                //float4 texN = tex2DStochastic(_NormalMapA, IN.uvMaterial);

                float4 texN = (
                    tex2DStochastic(_NormalMapA, IN.uvMaterial, sampler_NormalMapA) * b1 +
                    tex2DStochastic(_NormalMapB, IN.uvMaterial, sampler_NormalMapA) * b2 +
                    tex2DStochastic(_NormalMapC, IN.uvMaterial, sampler_NormalMapA) * b3 +
                    tex2DStochastic(_NormalMapD, IN.uvMaterial, sampler_NormalMapA) * b4 +
                    tex2DStochastic(_NormalMapE, IN.uvMaterial, sampler_NormalMapA) * b5 +
                    tex2DStochastic(_NormalMapF, IN.uvMaterial, sampler_NormalMapA) * b6 +
                    tex2DStochastic(_NormalMapG, IN.uvMaterial, sampler_NormalMapA) * b7 +
                    tex2DStochastic(_NormalMapH, IN.uvMaterial, sampler_NormalMapA) * b8
                    );

                texN *= IN.color;

                float3 localCoords = float3(2.0 * texN.ag - float2(1.0, 1.0), 0.0);
                localCoords.z = _BumpDepth;

                // Normal Transpose Matrix
                float3x3 local2WorldTranspose = float3x3(
                    IN.tangentWorld,
                    IN.binormalWorld,
                    IN.normalWorld
                );

                // Calculate Normal Direction
                float3 normalDirection = normalize(mul(localCoords, local2WorldTranspose));

                // Lighting
                float3 diffuseReflection = atten * _LightColor0.rgb * saturate(dot(normalDirection, lightDirection));
                float3 specularReflection = diffuseReflection * _SpecColor.rgb * pow(saturate(dot(reflect(-lightDirection, normalDirection), viewDirection)), _Shininess);

                // Rim Lighting
                float rim = 1 - saturate(dot(viewDirection, normalDirection));
                float3 rimLighting = saturate(pow(rim, _RimPower) * _RimColor.rgb * diffuseReflection);
                float3 lightFinal = diffuseReflection + specularReflection + rimLighting + UNITY_LIGHTMODEL_AMBIENT.rgb;

                col = float4(col * lightFinal * _AlbedoPower, 1.0);
            }

            {
                float x_border_col = (_BorderMap - IN.posWorld.r / 2) / _BorderMap;
                float x_border_end = (_MapSize.r - IN.posWorld.r / 2) / _BorderMap;
                float y_border_col = (_BorderMap - IN.posWorld.b / 2) / _BorderMap;
                float y_border_end = (_MapSize.g - IN.posWorld.b / 2) / _BorderMap;

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

                if (IN.posWorld.r / 2 < _BorderMap)
                {
                    col.r *= x_border_col;
                    col.g *= x_border_col;
                    col.b *= x_border_col;
                }
                if (IN.posWorld.b / 2 < _BorderMap)
                {
                    col.r *= y_border_col;
                    col.g *= y_border_col;
                    col.b *= y_border_col;
                }
                if (IN.posWorld.r / 2 > _MapSize.r - _BorderMap)
                {
                    col.r *= x_border_end;
                    col.g *= x_border_end;
                    col.b *= x_border_end;

                    if (IN.posWorld.r / 2 > _MapSize.r)
                    {
                        col.r = 0;
                        col.g = 0;
                        col.b = 0;
                    }
                }
                if (IN.posWorld.b / 2 > _MapSize.g - _BorderMap)
                {
                    col.r *= y_border_end;
                    col.g *= y_border_end;
                    col.b *= y_border_end;

                    if (IN.posWorld.b / 2 > _MapSize.g)
                    {
                        col.r = 0;
                        col.g = 0;
                        col.b = 0;
                    }
                }
            }

            return col;
        }



        ENDCG
    }


        // shadow casting support
        UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
}