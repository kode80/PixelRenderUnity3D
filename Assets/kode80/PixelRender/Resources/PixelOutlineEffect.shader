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

Shader "Hidden/kode80/PixelRender/PixelOutline"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
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

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
			sampler2D _CameraDepthTexture;
			float4 _OutlineColor;
			float _DepthThreshold;

			fixed4 frag (v2f i) : SV_Target
			{	
				float2 pixelSize = 1.0 / _ScreenParams.xy;
				fixed4 output = tex2D(_MainTex, i.uv);

				float a = 0.0;
				float depth = Linear01Depth( tex2D( _CameraDepthTexture, i.uv).r);
				float2 offsetUV;

				offsetUV = float2( i.uv.x, i.uv.y - pixelSize.y);
				a += depth - Linear01Depth( tex2D( _CameraDepthTexture, offsetUV).r);

				offsetUV = float2( i.uv.x - pixelSize.x, i.uv.y);
				a += depth - Linear01Depth( tex2D( _CameraDepthTexture, offsetUV).r);

				offsetUV = float2( i.uv.x + pixelSize.x, i.uv.y);
				a += depth - Linear01Depth( tex2D( _CameraDepthTexture, offsetUV).r);

				offsetUV = float2( i.uv.x, i.uv.y + pixelSize.y);
				a += depth - Linear01Depth( tex2D( _CameraDepthTexture, offsetUV).r);

				a = float(a < _DepthThreshold);
				output.rgb = lerp( output.rgb, _OutlineColor.rgb, saturate( a - (1.0 - _OutlineColor.a)));

				return output;
			}
			ENDCG
		}
	}
}
