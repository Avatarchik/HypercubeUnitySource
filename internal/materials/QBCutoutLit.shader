Shader "Hypercube/Cutout - lit"
{
    Properties {
        _MainTex ("Base (RGB) Transparency (A)", 2D) = "" {}
		_Color("Color", Color) = (1,1,1,1)
        _Cutoff ("Alpha cutoff", Range (0,1)) = 0.5
		//[MaterialEnum(Off,0,On,1)] _useLighting ("Lighting", Int) = 1
		[MaterialEnum(Off,0,Front,1,Back,2)] _Cull ("Cull", Int) = 2
    }
    SubShader {

	Tags { "RenderType"="TransparentCutout" }
	Cull [_Cull]

        Pass {

            AlphaTest Greater [_Cutoff]
			Lighting On
            Material 
			{
                Diffuse [_Color]
				Ambient [_Color]
            }
            
            SetTexture [_MainTex] { combine texture * primary }
        }
    }

	Fallback "Diffuse"
}
