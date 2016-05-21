Shader "Unlit/dicomShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "black" {}
		_Mod ("Brightness Mod", Range (0, 100)) = 1 
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" }
		LOD 100
		Cull Off
		ZWrite Off
		Blend One One

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
				float3 normal : NORMAL;
				
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float3 normalViewSpace : TEXCOORD1;
				float3 viewVectorViewspace : TEXCOORD2;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _Mod;
						
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                o.normalViewSpace = normalize(v.normal);
                o.viewVectorViewspace = normalize(ObjSpaceViewDir(v.vertex));

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);

				float3 temp =i.viewVectorViewspace ;
				float facing = dot(i.normalViewSpace, i.viewVectorViewspace);
				
				facing = abs(facing); //this makes it visible from the backface

				col *= facing * facing * facing;
				col *= _Mod;
				return col;
			}
			ENDCG
		}
	}
}
