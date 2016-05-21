Shader "Hypercube/Unlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Mod ("Brightness Mod", Range (0, 100)) = 1
        [MaterialEnum(Off,0,Front,1,Back,2)] _Cull ("Cull", Int) = 2
    }
    SubShader
    {
        Tags { "RenderType"="Opaque"  }
        Cull [_Cull]
        ZWrite On
 
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
            };
 
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
 
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Mod;
         
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
         
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                col *= _Mod;        
 
                return col;
            }
            ENDCG
        }
    }
	Fallback "Diffuse"
}
