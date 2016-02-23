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

Shader "kode80/PixelRender/SpriteCutout" 
{
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
	}
	SubShader 
	{
		Tags { "RenderType"="Opaque" }

		Pass
		{
			Name "ShadowCaster"
			Tags { "LightMode" = "ShadowCaster" }

			ZWrite On
			Ztest LEqual 
			//LOD 200
			
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float _Cutoff;
			
			struct v2f {
			   float4 position : SV_POSITION;
			   float2 uv : TEXCOORD0;
			};
			
			v2f vert(appdata_base v)
			{
			   	v2f o;
				o.position = UnityPixelSnap( mul( UNITY_MATRIX_MVP, v.vertex));
				o.uv = v.texcoord;
				
			   	return o;
			}
			
			half4 frag (v2f input) : COLOR
			{
				float4 output = tex2D( _MainTex, input.uv);
				if( output.a < _Cutoff)
				{
					discard;
				}
				return output;
			}
			
			ENDCG
		}

		Pass
		{
			ZWrite On
			Ztest LEqual 
			//LOD 200
			
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float _Cutoff;
			
			struct v2f {
			   float4 position : SV_POSITION;
			   float2 uv : TEXCOORD0;
			};
			
			v2f vert(appdata_base v)
			{
			   	v2f o;
				o.position = UnityPixelSnap( mul( UNITY_MATRIX_MVP, v.vertex));
				o.uv = v.texcoord;
				
			   	return o;
			}
			
			half4 frag (v2f input) : COLOR
			{
				float4 output = tex2D( _MainTex, input.uv);
				if( output.a < _Cutoff)
				{
					discard;
				}
				return output;
			}
			
			ENDCG
		}
	} 
}
