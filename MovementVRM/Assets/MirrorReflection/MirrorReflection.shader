Shader "Custom/MirrorReflection"
{
    Properties
    {
        _ReflectionTex ("ReflectionTexture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 projCoord : TEXCOORD0;
            };
            
            sampler2D _ReflectionTex;
            
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.projCoord = ComputeScreenPos(o.vertex);
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                // return tex2D(_ReflectionTex, i.projCoord.xy / i.projCoord.w);
                return tex2Dproj(_ReflectionTex, i.projCoord);
            }
            ENDCG
        }
    }
}
