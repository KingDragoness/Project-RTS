Shader "Custom/SyntiosTerrain"
{
    Properties
    {
          _Color ("Main Color", Color) = (1,1,1,1)
          _MainTex ("Diffuse Texture", 2D) = "white" {}
          _BumpMap ("NormalMap", 2D) = "bump" {}
          _BaseScale ("BaseScale", Vector) = (1,1,1,0)
    }
    SubShader
    {
           Tags { "RenderType" = "Opaque" }
      CGPROGRAM
      #pragma surface surf Lambert
    
      struct Input {
          float3 worldPos;
          float3 worldNormal; INTERNAL_DATA
          float3 vertColors;

      };

        void vert (inout appdata_full v, out Input o)
        {
            o.vertColors = abs(v.normal);
        }
    
      sampler2D _MainTex;
      sampler2D _BumpMap;
      fixed4 _Color;
      float3 _BaseScale;

    void surf (Input IN, inout SurfaceOutput o) {
        fixed4 texXY = tex2D(_MainTex, IN.worldPos.xy * _BaseScale.z);
        fixed4 texXZ = tex2D(_MainTex, IN.worldPos.xz * _BaseScale.y);
        fixed4 texYZ = tex2D(_MainTex, IN.worldPos.yz * _BaseScale.x);
        fixed3 mask = fixed3(
        dot (IN.worldNormal, fixed3(0,0,1)),
        dot (IN.worldNormal, fixed3(0,1,0)),
        dot (IN.worldNormal, fixed3(1,0,0)));
        fixed4 tex = texXY * abs(mask.x) +texXZ * abs(mask.y) +texYZ * abs(mask.z);
        fixed4 c = tex * _Color;

            float3 uv = IN.worldPos.xyz;

            half4 x = tex2D(_BumpMap, uv.zy);
            half4 y = tex2D(_BumpMap, uv.zx);
            half4 z = tex2D(_BumpMap, uv.xy);
            half4 n = float4(1,1,1,1);
            n = lerp(n,x,IN.vertColors.r);
            n = lerp(n,y,IN.vertColors.g);
            n = lerp(n,z,IN.vertColors.b);

                     o.Albedo = c.rgb;
                     o.Normal = UnpackNormal(n);

      }
      ENDCG
    }
    
    FallBack "Diffuse"
}
