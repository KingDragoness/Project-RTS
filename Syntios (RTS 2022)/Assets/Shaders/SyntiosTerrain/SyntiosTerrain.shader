// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/SyntiosTerrain"
{
    Properties
    {
        _SplatMap("Splatmap (RGB)", 2D) = "black" {}
        _FOWMap("Fog of War (Ever Explored)", 2D) = "black" {}
        _GroundTexture("Ground Texture", 2D) = "white" {}
        _CloudFog("Cloud Fog Pattern", 2D) = "black" {}
        _UnexploredFog("Unexplored Fog Color", Color) = (0.1,0.1,0.1,1)
        _SpecularMap("SpecularMap (RGB)", 2D) = "white" {}

        _TextureA("Layer 1", 2D) = "white" {}
        _NormalMapA("NormalMap 1", 2D) = "white" {}

        _TextureB("Layer 2", 2D) = "white" {}
        _NormalMapB("NormalMap 2", 2D) = "white" {}

        _TextureC("Layer 3", 2D) = "white" {}
        _NormalMapC("NormalMap 3", 2D) = "white" {}

        _TextureD("Layer 4", 2D) = "white" {}
        _NormalMapD("NormalMap 4", 2D) = "white" {}

        _TextureE("Layer 5", 2D) = "white" {}
        _TextureF("Layer 6", 2D) = "white" {}
        _TextureG("Layer 7", 2D) = "white" {}
        _TextureH("Layer 8", 2D) = "white" {}
        _TextureScale("TextureScale", Range(0.01,10)) = 0.25
        _SplatmapScale("SplatmapScale", Range(0.01,2)) = 0.5
        _FOWmapScale("FOWScale", Range(0.1,4)) = 1
        _FOWSampleRadiusBlur("FOW Blur Radius", Range(0.0,0.05)) = 0.005
        _PrioGround("Prio Ground", Range(0.01, 2.0)) = 1
        _PrioA("Prio layer1", Range(0.01, 2.0)) = 1
        _PrioB("Prio layer2", Range(0.01, 2.0)) = 1
        _PrioC("Prio layer3", Range(0.01, 2.0)) = 1
        _PrioD("Prio layer4", Range(0.01, 2.0)) = 1
        _ContrastCloud("Cloud Contrast", Range(0.001, 1.0)) = 0.01
        _Depth("Depth", Range(0.01,1.0)) = 0.2
        _MainLightPosition("MainLightPosition", Vector) = (0,0,0,0)
        _LightColor("LightColor", Color) = (1,1,1,1)
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
 
            sampler2D _SplatMap;
            sampler2D _FOWMap;
            sampler2D _GroundTexture;
            sampler2D _CloudFog;
            sampler2D _SpecularMap;
            uniform float4 _UnexploredFog;
            sampler2D _TextureA;
            sampler2D _TextureB;
            sampler2D _TextureC;
            sampler2D _TextureD;
            sampler2D _NormalMapA;
            sampler2D _NormalMapB;
            sampler2D _NormalMapC;
            sampler2D _NormalMapD;

            fixed _TextureScale;
            fixed _SplatmapScale;
            fixed _FOWmapScale;
            half _FOWSampleRadiusBlur;

            uniform float3 _MainLightPosition;
            uniform float4 _LightColor;

            fixed _PrioGround;
            fixed _PrioA;
            fixed _PrioB;
            fixed _PrioC;
            fixed _PrioD;
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
                float3 lightdir : TEXCOORD5;
                float3 viewdir : TEXCOORD6;
                float3 T : TEXCOORD7;
                float3 B : TEXCOORD8;
                float3 N : TEXCOORD9;

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
                    float4 worldPosition = mul(unity_ObjectToWorld, v.vertex);
                    float3 lightDir = worldPosition.xyz - _MainLightPosition.xyz;
                    OUT.lightdir = normalize(lightDir);

                    // calc viewDir vector 
                    float3 viewDir = normalize(worldPosition.xyz - _WorldSpaceCameraPos.xyz);
                    OUT.viewdir = viewDir;

                    // calc Normal, Binormal, Tangent vector in world space
                    // cast 1st arg to 'float3x3' (type of input.normal is 'float3')
                    float3 worldNormal = mul((float3x3)unity_ObjectToWorld, v.normal);
                    float3 worldTangent = mul((float3x3)unity_ObjectToWorld, v.tangent);

                    float3 binormal = cross(v.normal, v.tangent.xyz); // *input.tangent.w;
                    float3 worldBinormal = mul((float3x3)unity_ObjectToWorld, binormal);

                    // and, set them
                    OUT.N = normalize(worldNormal);
                    OUT.T = normalize(worldTangent);
                    OUT.B = normalize(worldBinormal);
                }


                // uvs of the rendered materials are based on world position
                OUT.uvMaterial = mul(unity_ObjectToWorld, v.vertex).xz * _TextureScale;
                OUT.materialPrios = fixed4(_PrioA, _PrioB, _PrioC, _PrioD);
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
 
            fixed4 frag(v2f IN) : SV_Target
            {
                float3 tangentNormalA = tex2D(_NormalMapA, IN.uvMaterial).xyz;
                float3 tangentNormalB = tex2D(_NormalMapB, IN.uvMaterial).xyz;
                float3 tangentNormalC = tex2D(_NormalMapC, IN.uvMaterial).xyz;
                float3 tangentNormalD = tex2D(_NormalMapD, IN.uvMaterial).xyz;


                fixed4 groundColor = tex2DStochastic(_GroundTexture, IN.uvMaterial);
                fixed4 materialAColor = tex2DStochastic(_TextureA, IN.uvMaterial);
                fixed4 materialBColor = tex2DStochastic(_TextureB, IN.uvMaterial);
                fixed4 materialCColor = tex2DStochastic(_TextureC, IN.uvMaterial);
                fixed4 materialDColor = tex2DStochastic(_TextureD, IN.uvMaterial);
 
                // store heights for all materials on this pixel
                fixed groundHeight = groundColor.a;
                fixed4 materialHeights = fixed4(materialAColor.a, materialBColor.a, materialCColor.a, materialDColor.a);
                // avoid black artefacts by division by zero
                materialHeights = max(0.0001, materialHeights);
 
                // get material amounts from splatmap
                //fixed4 materialAmounts = tex2D(_SplatMap, IN.uvSplat).argb;
                fixed4 materialAmounts = tex2D(_SplatMap, IN.uvSplat).argb;
                
                // the ground amount takes up all unused space
                fixed groundAmount = 1.0 - min(1.0, materialAmounts.r + materialAmounts.g + materialAmounts.b + materialAmounts.a);
                 
                // calculate material strenghts
                fixed alphaGround = groundAmount * _PrioGround * groundHeight;
                fixed4 alphaMaterials = materialAmounts * IN.materialPrios * materialHeights;
 
                // find strongest point of all materials
                fixed max_01234 = max(alphaGround, alphaMaterials.r);
                max_01234 = max(max_01234, alphaMaterials.g);
                max_01234 = max(max_01234, alphaMaterials.b);
                max_01234 = max(max_01234, alphaMaterials.a);
 
                //lower threshold
                max_01234 = max(max_01234 - _Depth, 0);
 
                // mask all materials above threshold
                fixed b0 = max(alphaGround - max_01234, 0);
                fixed b1 = max(alphaMaterials.r - max_01234, 0);
                fixed b2 = max(alphaMaterials.g - max_01234, 0);
                fixed b3 = max(alphaMaterials.b - max_01234, 0);
                fixed b4 = max(alphaMaterials.a - max_01234, 0);
 
                // combine all materials and normalize
                fixed alphaSum = b0 + b1 + b2 + b3 + b4;
                fixed4 col = (
                    groundColor * b0 +
                    materialAColor * b1 +
                    materialBColor * b2 +
                    materialCColor * b3 +
                    materialDColor * b4
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

                    sum += tex2D(_FOWMap, half2(IN.uvFOW.x, IN.uvFOW.y)) * 1;

                    int totalLength = 3;
                    int totalSample = (totalLength * totalLength) * (totalLength * totalLength);
                    for (int x = -(totalSample / totalLength / totalLength); x < (totalSample / totalLength / totalLength); x++)
                    {
                        sum += tex2D(_FOWMap, half2(IN.uvFOW.x - x * _FOWSampleRadiusBlur, IN.uvFOW.y + x * _FOWSampleRadiusBlur));
                    }
                    for (int x = -(totalSample / totalLength / totalLength); x < (totalSample / totalLength / totalLength); x++)
                    {
                        sum += tex2D(_FOWMap, half2(IN.uvFOW.x - x * _FOWSampleRadiusBlur, IN.uvFOW.y - x * _FOWSampleRadiusBlur));
                    }

                    sum += tex2D(_FOWMap, half2(IN.uvFOW.x, IN.uvFOW.y - _FOWSampleRadiusBlur));
                    sum += tex2D(_FOWMap, half2(IN.uvFOW.x, IN.uvFOW.y + _FOWSampleRadiusBlur));

                    sum += tex2D(_FOWMap, half2(IN.uvFOW.x - _FOWSampleRadiusBlur, IN.uvFOW.y));
                    sum += tex2D(_FOWMap, half2(IN.uvFOW.x + _FOWSampleRadiusBlur, IN.uvFOW.y));
                    
                    //sum += tex2D(_FOWMap, half2(IN.uvFOW.x - 4.0 * _FOWSampleRadiusBlur, IN.uvFOW.y - 4.0 * _FOWSampleRadiusBlur));
                    //sum += tex2D(_FOWMap, half2(IN.uvFOW.x - 3.0 * _FOWSampleRadiusBlur, IN.uvFOW.y - 3.0 * _FOWSampleRadiusBlur));
                    //sum += tex2D(_FOWMap, half2(IN.uvFOW.x - 2.0 * _FOWSampleRadiusBlur, IN.uvFOW.y - 2.0 * _FOWSampleRadiusBlur));
                    //sum += tex2D(_FOWMap, half2(IN.uvFOW.x - _FOWSampleRadiusBlur, IN.uvFOW.y - _FOWSampleRadiusBlur));
                    //sum += tex2D(_FOWMap, half2(IN.uvFOW.x + _FOWSampleRadiusBlur, IN.uvFOW.y + _FOWSampleRadiusBlur));
                    //sum += tex2D(_FOWMap, half2(IN.uvFOW.x + 2.0 * _FOWSampleRadiusBlur, IN.uvFOW.y + 2.0 * _FOWSampleRadiusBlur));
                    //sum += tex2D(_FOWMap, half2(IN.uvFOW.x + 3.0 * _FOWSampleRadiusBlur, IN.uvFOW.y + 3.0 * _FOWSampleRadiusBlur));
                    //sum += tex2D(_FOWMap, half2(IN.uvFOW.x + 4.0 * _FOWSampleRadiusBlur, IN.uvFOW.y + 4.0 * _FOWSampleRadiusBlur));

                    //sum += tex2D(_FOWMap, half2(IN.uvFOW.x + 4.0 * _FOWSampleRadiusBlur, IN.uvFOW.y - 4.0 * _FOWSampleRadiusBlur));
                    //sum += tex2D(_FOWMap, half2(IN.uvFOW.x + 3.0 * _FOWSampleRadiusBlur, IN.uvFOW.y - 3.0 * _FOWSampleRadiusBlur));
                    //sum += tex2D(_FOWMap, half2(IN.uvFOW.x + 2.0 * _FOWSampleRadiusBlur, IN.uvFOW.y - 2.0 * _FOWSampleRadiusBlur));
                    //sum += tex2D(_FOWMap, half2(IN.uvFOW.x + _FOWSampleRadiusBlur, IN.uvFOW.y - _FOWSampleRadiusBlur));
                    //sum += tex2D(_FOWMap, half2(IN.uvFOW.x - _FOWSampleRadiusBlur, IN.uvFOW.y + _FOWSampleRadiusBlur));
                    //sum += tex2D(_FOWMap, half2(IN.uvFOW.x - 2.0 * _FOWSampleRadiusBlur, IN.uvFOW.y + 2.0 * _FOWSampleRadiusBlur));
                    //sum += tex2D(_FOWMap, half2(IN.uvFOW.x - 3.0 * _FOWSampleRadiusBlur, IN.uvFOW.y + 3.0 * _FOWSampleRadiusBlur));
                    //sum += tex2D(_FOWMap, half2(IN.uvFOW.x - 4.0 * _FOWSampleRadiusBlur, IN.uvFOW.y + 4.0 * _FOWSampleRadiusBlur));

                    float2 fogFOW = IN.uvFOW * 11;
                    float2 fogFOW_1 = IN.uvFOW * 4.44;
                    fogFOW.x += _Time * 1;
                    fogFOW.y += _Time * 0.8;
                    fogFOW_1.x += _Time * 1.3;
                    fogFOW_1.y += _Time * -0.9;

                    sum /= (totalSample+4);
                    sum.r = clamp(sum.r, _UnexploredFog.r, 1);
                    sum.r += tex2DStochastic(_CloudFog, fogFOW).r * _ContrastCloud * 0.5;
                    sum.r += tex2DStochastic(_CloudFog, fogFOW_1).r * _ContrastCloud * 0.6;

                    sum.g = sum.r; //forcing greyscale
                    sum.b = sum.r;

                    col.rgb *= sum;
                }




                //NORMAL MAP and Light
                {
                    tangentNormalA = normalize(tangentNormalA * 2 - 1);
                    tangentNormalB = normalize(tangentNormalB * 2 - 1);
                    tangentNormalC = normalize(tangentNormalC * 2 - 1);
                    tangentNormalD = normalize(tangentNormalD * 2 - 1);

                    fixed3 normalCol = (
                        tangentNormalA * b1 +
                        tangentNormalB * b2 +
                        tangentNormalC * b3 +
                        tangentNormalD * b4
                        ) / 4;


                    // 'TBN' transforms the world space into a tangent space
                    // we need its inverse matrix
                    // Tip : An inverse matrix of orthogonal matrix is its transpose matrix
                    float3x3 TBN = float3x3(normalize(IN.T), normalize(IN.B), normalize(IN.N));
                    TBN = transpose(TBN);

                    // finally we got a normal vector from the normal map
                    float3 worldNormal = mul(TBN, normalCol);
          
                    float3 lightDir = normalize(IN.lightdir);
                    // calc diffuse, as we did in pixel shader
                    float3 diffuse = saturate(dot(worldNormal, -lightDir));
                    diffuse = _LightColor * col * diffuse;

                    // Specular here
                    float3 specular = 0;
                    if (diffuse.x > 0) {
                        float3 reflection = reflect(lightDir, worldNormal);
                        float3 viewDir = normalize(IN.viewdir);

                        specular = saturate(dot(reflection, -viewDir));
                        specular = pow(specular, 20.0f);

                        float4 specularIntensity = tex2D(_SpecularMap, IN.uvMaterial);
                        specular *= _LightColor * specularIntensity * 0.1;
                    }

                    // make some ambient,
                    float3 ambient = float3(0.1f, 0.1f, 0.1f) * 10 * col;

                    return float4(ambient + diffuse + specular, 1);

                }


                return col;
            }

            

            ENDCG
        }
         
        // shadow casting support
        UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
}
