// Upgrade NOTE: replaced '_CameraToWorld' with 'unity_CameraToWorld'

// Upgrade NOTE: commented out 'float4x4 _CameraToWorld', a built-in variable

// Upgrade NOTE: commented out 'float4x4 _CameraToWorld', a built-in variable

// Upgrade NOTE: commented out 'float4x4 _CameraToWorld', a built-in variable

// Upgrade NOTE: commented out 'float4x4 _CameraToWorld', a built-in variable
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/OceanEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _WaterHeight("Water Level",float)=0
        _ShallowColor("Shallow Color",Color)=(1,1,1,1)
        _DeepColor("Deep Color",Color)=(0,0,0,1)
        alphaMultiplier("Alpha Multiplier",float)=1
        depthMultiplier("Depth Multiplier",float)=1
        waveSpeed("Wave Speed",float)=1
        waveNormalA("Wave normal map A",2D)="white"{}
        waveNormalB("Wave normal map B",2D)="white"{}
        waveStrength("Wave Strength",float)=1
        waveNormalScale("Wave normal scale",float)=1
        smoothness("smoothness",float)=1
        specularColor("Specular Color",Color)=(1,1,1,1)
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "../OverCloud/Resources/Shaders/OverCloudCore.cginc"
            #include "../OverCloud/Resources/Shaders/Atmosphere.cginc"
		

            sampler2D _MainTex;
            sampler2D _CameraDepthTexture;

            float4x4 _ViewProjectInverse;


            float _WaterHeight;
            float4 _ShallowColor;
            float4 _DeepColor;

            float alphaMultiplier;
            float depthMultiplier;
            float smoothness;
            float4 specularColor;

            float waveSpeed;
            sampler2D waveNormalA;
			sampler2D waveNormalB;
			float waveStrength;
			float waveNormalScale;

            float3 dirToSun;
			
            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldDirection : TEXCOORD1;
            };
         
            v2f vert(appdata i)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(i.vertex);
                o.uv = i.uv;
                float4 D = mul(_ViewProjectInverse, float4((i.uv.x) * 2 - 1, (i.uv.y) * 2 - 1, 0.5, 1));
                D.xyz /= D.w;
                D.xyz -= _WorldSpaceCameraPos;
                float4 D0 = mul(_ViewProjectInverse, float4(0, 0, 0.5, 1));
                D0.xyz /= D0.w;
                D0.xyz -= _WorldSpaceCameraPos;
                o.worldDirection = D.xyz / length(D0.xyz);
                return o;
            }
            float rayPlaneIntersect(float3 rayPos,float3 rayDir,float3 planePoint,float3 planeNorm){
                float denom=dot(planeNorm,rayDir);
                if(denom<1e-6)
                {
                    return 1e16;
                }
                return dot(planePoint-rayPos,planeNorm)/denom;
            }
            float3 blend_rnm(float3 n1, float3 n2)
            {
                n1.z += 1;
                n2.xy = -n2.xy;

                return n1 * dot(n1, n2) / n1.z - n2;
            }

            // Sample normal map with triplanar coordinates
            // Returned normal will be in obj/world space (depending whether pos/normal are given in obj or world space)
            // Based on: medium.com/@bgolus/normal-mapping-for-a-triplanar-shader-10bf39dca05a
            float3 triplanarNormal(float3 vertPos, float3 normal, float3 scale, float2 offset, sampler2D normalMap) {
                float3 absNormal = abs(normal);

                // Calculate triplanar blend
                float3 blendWeight = saturate(pow(normal, 4));
                // Divide blend weight by the sum of its components. This will make x + y + z = 1
                blendWeight /= dot(blendWeight, 1);

                // Calculate triplanar coordinates
                float2 uvX = vertPos.zy * scale + offset;
                float2 uvY = vertPos.xz * scale + offset;
                float2 uvZ = vertPos.xy * scale + offset;

                // Sample tangent space normal maps
                // UnpackNormal puts values in range [-1, 1] (and accounts for DXT5nm compression)
                float3 tangentNormalX = UnpackNormal(tex2D(normalMap, uvX));
                float3 tangentNormalY = UnpackNormal(tex2D(normalMap, uvY));
                float3 tangentNormalZ = UnpackNormal(tex2D(normalMap, uvZ));

                // Swizzle normals to match tangent space and apply reoriented normal mapping blend
                tangentNormalX = blend_rnm(half3(normal.zy, absNormal.x), tangentNormalX);
                tangentNormalY = blend_rnm(half3(normal.xz, absNormal.y), tangentNormalY);
                tangentNormalZ = blend_rnm(half3(normal.xy, absNormal.z), tangentNormalZ);

                // Apply input normal sign to tangent space Z
                float3 axisSign = sign(normal);
                tangentNormalX.z *= axisSign.x;
                tangentNormalY.z *= axisSign.y;
                tangentNormalZ.z *= axisSign.z;

                // Swizzle tangent normals to match input normal and blend together
                float3 outputNormal = normalize(
                    tangentNormalX.zyx * blendWeight.x +
                    tangentNormalY.xzy * blendWeight.y +
                    tangentNormalZ.xyz * blendWeight.z
                );

                return outputNormal;
            }
            float4 frag(v2f i) : COLOR
            {
                fixed4 originalCol = tex2D(_MainTex, i.uv);
                float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
                //length to ground
                depth = LinearEyeDepth(depth);
                float3 originPos=_WorldSpaceCameraPos;

                /* World position of some random point along this ray */
                float3 WD = i.worldDirection;
                float distAboveOcean=rayPlaneIntersect(originPos,normalize(WD),float3(0,_WaterHeight,0),float3(0,-1,0));
                if(distAboveOcean>100000)
                    return originalCol;
                float oceanDepth=length(WD*depth)-distAboveOcean;
                if(oceanDepth>0){
                    float3 rayOceanIntersectPos=distAboveOcean*normalize(WD)+originPos;
                    float3 oceanSphereNormal=float3(0,1,0);
                    
                    float t = 1 - exp(-oceanDepth  * depthMultiplier);
					float alpha =  1-exp(-oceanDepth  * alphaMultiplier);
					float4 oceanCol = lerp(_ShallowColor,_DeepColor, t);
                    
					float2 waveOffsetA = float2(_Time.x * waveSpeed, _Time.x * waveSpeed * 0.8);
					float2 waveOffsetB =  float2(_Time.x * waveSpeed * - 0.8, _Time.x * waveSpeed * -0.3);
                    
                    float3 waveNormal = triplanarNormal(rayOceanIntersectPos, oceanSphereNormal, waveNormalScale , waveOffsetA, waveNormalA);
					waveNormal = triplanarNormal(rayOceanIntersectPos, waveNormal, waveNormalScale, waveOffsetB, waveNormalB);
					waveNormal = normalize(lerp(oceanSphereNormal, waveNormal, waveStrength));
					//return float4(oceanNormal * .5 + .5,1);
					float diffuseLighting = saturate(dot(oceanSphereNormal, dirToSun));
					float specularAngle = acos(dot(normalize((dirToSun) - normalize(WD)), waveNormal));
					float specularExponent = specularAngle / (1 - smoothness);
					float specularHighlight = exp(-specularExponent * specularExponent);
                    
					oceanCol *= diffuseLighting;
					oceanCol += specularHighlight * (distAboveOcean > 0) * specularColor;
                    float4 color=originalCol * (1-alpha) + oceanCol * alpha;
                     OVERCLOUD_FRAGMENT_FULL(color,i.uv,rayOceanIntersectPos);
                     return color;
                }
                    
                else
                    return originalCol;
                /* Multiply by 'depth' */
                WD *= depth;
                /* That's our world-coordinate position! */
                float3 W = WD + _WorldSpaceCameraPos;
                if(W.y<100)
                    return 1;
                return originalCol;
             
                /* Demo: multiply the pixel by frac(W.x), giving x-aligned bands of shadow */
                float4 c = tex2D(_MainTex, i.uv);
                c.rgb *= frac(W.x);
                return c;
            }
            ENDCG
        }
    }
}
