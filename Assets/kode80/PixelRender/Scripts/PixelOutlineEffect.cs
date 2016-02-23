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

using UnityEngine;
using System.Collections;

namespace kode80.PixelRender
{
	[ExecuteInEditMode]
	public class PixelOutlineEffect : MonoBehaviour
	{
		public Color outlineColor;
		[Range( 0.0f, 0.05f)]
		public float depthThreshold;
		private Material _outlineMaterial;


		void OnEnable()
		{
			Camera cam = GetComponent<Camera>();
			cam.depthTextureMode = DepthTextureMode.Depth;
		}

		[ImageEffectOpaque]
		void OnRenderImage( RenderTexture source, RenderTexture destination)
		{
			if( _outlineMaterial == null)
			{
				_outlineMaterial = new Material( Shader.Find( "Hidden/kode80/PixelRender/PixelOutline"));
				_outlineMaterial.hideFlags = HideFlags.HideAndDontSave;
			}

			_outlineMaterial.SetColor( "_OutlineColor", outlineColor);
			_outlineMaterial.SetFloat( "_DepthThreshold", -depthThreshold);

			Graphics.Blit( source, destination, _outlineMaterial);
		}
	}
}
