// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/TerrainShader"
{
    Properties
    {
            testTextures("Texture2D",2D)="white"{}
            testScale("Scale",float)=1
            heightMult("Height Multiplier",float)=1
            steepnessMult("Steepness Multiplier",float)=1
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color :COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color=v.color;
                return o;
            }

            sampler2D _MainTex;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col =1- i.color;
                // just invert the colors
                // col.rgb = 1 - col.rgb;
                return col;
            }
            ENDCG
        }
        Tags { "RenderType" = "Opaque" }
        LOD 200
        
        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0
        float minHeight;
        float maxHeight;
        static const int MAX_LAYER_COUNT=10;
        int layerCount;
        float3 baseColors[MAX_LAYER_COUNT];
        float baseStartHeights[MAX_LAYER_COUNT];
        float baseBlendHeights[MAX_LAYER_COUNT];
        float baseTextureScale[MAX_LAYER_COUNT];
        float baseColorsStrength[MAX_LAYER_COUNT];
        const static float epsilon=1E-4;
        float heightMult;
        float steepnessMult;
        struct Input
        {
            float3 worldPos;
            float3 worldNormal;
        };

        float inverseLerp(float a,float b,float val){
            return saturate((val-a)/(b-a));
        }
        sampler2D testTextures;
        float testScale;
        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)
        UNITY_DECLARE_TEX2DARRAY(baseTextures);
        float3 triplanar(float3 scaleWorldPos,float3 blendAxes,int textureIndex){
            
            float3 xProjection=UNITY_SAMPLE_TEX2DARRAY(baseTextures,float3(scaleWorldPos.y,scaleWorldPos.z,textureIndex))*blendAxes.x;
            float3 yProjection=UNITY_SAMPLE_TEX2DARRAY(baseTextures,float3(scaleWorldPos.x,scaleWorldPos.z,textureIndex))*blendAxes.y;
            float3 zProjection=UNITY_SAMPLE_TEX2DARRAY(baseTextures,float3(scaleWorldPos.x,scaleWorldPos.y,textureIndex))*blendAxes.z;
            return xProjection+yProjection+zProjection;

        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float3 blendAxes=abs(IN.worldNormal);
            blendAxes/=blendAxes.x+blendAxes.y+blendAxes.z;
            
            float heightPercent=inverseLerp(minHeight,maxHeight,IN.worldPos.y);
            float steepness01=1-abs(IN.worldNormal.y)+epsilon;
            for(int i=0;i<layerCount;i++)
            {
                    
                    float drawStrength=(inverseLerp(-baseBlendHeights[i]/2-epsilon,baseBlendHeights[i]/2,heightPercent-baseStartHeights[i]))*heightMult;
                    float3 baseColor=baseColors[i]*baseColorsStrength[i];
                    float3 textureColor=triplanar(IN.worldPos/baseTextureScale[i],blendAxes,i)*(1-baseColorsStrength[i]);
                    o.Albedo=(o.Albedo*(1-drawStrength)+(baseColor+textureColor)*drawStrength);
                    float drawStrength2=(inverseLerp(-baseBlendHeights[i]/2-epsilon,baseBlendHeights[i]/2,steepness01-baseStartHeights[i]))*steepnessMult;
                    float3 baseColor2=baseColors[i]*baseColorsStrength[i];
                    float3 textureColor2=triplanar(IN.worldPos/baseTextureScale[i],blendAxes,i)*(1-baseColorsStrength[i]);
                    o.Albedo=(o.Albedo*(1-drawStrength2)+(baseColor2+textureColor2)*drawStrength2);
                    
            }
            
            
        }/////
        ENDCG
    }
    FallBack "Diffuse"
}
