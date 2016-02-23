//***************************************************
//
//  Author: Ben Hopkins
//  Copyright (C) 2016 kode80 LLC, 
//  all rights reserved
// 
//  Free to use for non-commercial purposes, 
//  see full license in project root:
//  PixelRenderNonCommercialLicense.html
//  
//  Commercial licenses available for purchase from:
//  http://kode80.com/
//
//***************************************************

Shader "kode80/DrawTexture" 
{
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader 
	{
		Tags { "Queue" = "Background+1" "RenderType"="Opaque" }
			
		Pass
		{
			Cull Off 
			ZWrite Off
			Ztest Less 
			//LOD 200
			Blend One OneMinusSrcAlpha
			
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			
			struct v2f {
			   float4 position : SV_POSITION;
			   float2 uv : TEXCOORD0;
			};
			
			v2f vert(appdata_base v)
			{
			   	v2f o;
				o.position = mul( UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.texcoord;
				
			   	return o;
			}
			
			half4 frag (v2f input) : COLOR
			{
				return tex2D( _MainTex, input.uv);
			}
			
			ENDCG
		}
	} 
}
