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

Shader "kode80/PixelRender/PixelArtShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_NormalTex ("Normal Map", 2D) = "white" {}
		_PaletteTex ("Palette", 2D) = "white" {}
		_LightDir ( "Light Direction", Vector) = ( 0.0, 0.0, 0.0)
		_DitherThreshold ( "Dither Threshold", Range( 0.0, 1.0)) = 0.5
		_PaletteMix ( "Palette Mix", Range( 0.0, 1.0)) = 0.0
		_Palette2Tex ("Palette 2", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			Lighting On
			Tags {"LightMode" = "ForwardBase"}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma shader_feature _NORMALMAP
			#pragma shader_feature _PALETTEMIX
			#pragma shader_feature _SHADOWS
			#pragma multi_compile_fwdbase

			#include "UnityCG.cginc"
			#include "AutoLight.cginc"
 			#include "Lighting.cginc"

			struct input_t
			{
				float4 pos : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 pos : SV_POSITION;
				float3 normal : NORMAL;
				float3 wsVertex : TEXCOORD1;
				float4 screenUV : TEXCOORD2;
                LIGHTING_COORDS(3,4)

				#if _NORMALMAP
				half3 tspace0 : TEXCOORD5; // tangent.x, bitangent.x, normal.x
                half3 tspace1 : TEXCOORD6; // tangent.y, bitangent.y, normal.y
                half3 tspace2 : TEXCOORD7; // tangent.z, bitangent.z, normal.z
                #endif
			};

			sampler2D _MainTex;
			sampler2D _NormalTex;
			sampler2D _PaletteTex;
			sampler2D _Palette2Tex;
			float4 _PaletteTex_TexelSize;
			float4 _MainTex_ST;
			float3 _LightDir;
			float _DitherThreshold;
			float _PaletteMix;
			
			v2f vert (input_t v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.pos);
				o.uv = TRANSFORM_TEX( v.uv, _MainTex);

				// Normals must be normalized, scaling in Unity 5 affects normals
				o.normal = mul( _Object2World, float4( normalize(v.normal), 0.0 ) ).xyz;
				o.wsVertex = mul( _Object2World, v.pos).xyz;
				o.screenUV = o.pos;

				#if _NORMALMAP

				half3 wTangent = UnityObjectToWorldDir( v.tangent.xyz);
                // compute bitangent from cross product of normal and tangent
                half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
                half3 wBitangent = cross( o.normal, wTangent) * tangentSign;
                // output the tangent space matrix
                o.tspace0 = half3(wTangent.x, wBitangent.x, o.normal.x);
                o.tspace1 = half3(wTangent.y, wBitangent.y, o.normal.y);
                o.tspace2 = half3(wTangent.z, wBitangent.z, o.normal.z);
                #endif

                TRANSFER_VERTEX_TO_FRAGMENT(o);

				return o;
			}

			float dither2x2( float2 uv, float brightness, float edge1, float edge2) 
			{
				float2 position = uv * _ScreenParams.xy;
				int x = int(fmod(position.x, 2));
				int y = int(fmod(position.y, 2));
				int index = x + y * 2;
				float dither = 0.0;

				if (x < 8) {
					if (index == 0) dither = edge1;
					if (index == 1) dither = edge2;
					if (index == 2) dither = edge2;
					if (index == 3) dither = edge1;
				}

				bool shouldDither = smoothstep( edge1, edge2, brightness) > _DitherThreshold;
				return shouldDither ? dither : brightness;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float2 screenUV = (i.screenUV.xy / i.screenUV.w) * 0.5 + 0.5;
				float paletteIndex = tex2D(_MainTex, i.uv).r;
				float3 lightDir = normalize( _LightDir);

				#if _NORMALMAP
				half3 tnormal = UnpackNormal(tex2D(_NormalTex, i.uv));
                // transform normal from tangent to world space
                half3 fragNormal;
                fragNormal.x = dot(i.tspace0, tnormal);
                fragNormal.y = dot(i.tspace1, tnormal);
                fragNormal.z = dot(i.tspace2, tnormal);
                fragNormal = normalize( fragNormal);

				#else
				float3 fragNormal = normalize( i.normal);
				#endif

				//float d = saturate( dot( lightDir, fragNormal));
				float d = dot( lightDir, fragNormal) * 0.5 + 0.5;
				d = sin( d);

				float halfTexel = _PaletteTex_TexelSize.y * 0.5;

				float dQ = floor( d * _PaletteTex_TexelSize.w) * _PaletteTex_TexelSize.y + halfTexel;
				float dQ2 = dQ + _PaletteTex_TexelSize.y;
				d = dither2x2( screenUV, d, dQ, dQ2);

				#if _SHADOWS
				float  atten = 1.0 - LIGHT_ATTENUATION(i);
				//atten = float( atten < 0.99825);
				d -= atten * _PaletteTex_TexelSize.y * 4.0;
				d = saturate( d);
				#endif

				#if _PALETTEMIX
				float4 color1 = tex2D( _PaletteTex, float2( paletteIndex, d));
				float4 color2 = tex2D( _Palette2Tex, float2( paletteIndex, d));
				float4 output = lerp( color1, color2, saturate( _PaletteMix));
				#else
				float4 output = tex2D( _PaletteTex, float2( paletteIndex, d));
				#endif

				return output;
			}
			ENDCG
		}
	}
	CustomEditor "kode80.PixelRender.PixelArtShaderEditor"
	Fallback "Diffuse"
}
