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
	public class IrisWipeEffect : MonoBehaviour 
	{
		public Vector2 center;
		[Range( 0.0f, 1.0f)]
		public float position = 0.5f;
		private Material _material;

		void Start()
		{
			if( Application.isPlaying)
			{
				position = 0.0f;
			}
		}

		void Update()
		{
			if( position < 1.0f)
			{
				position += (1.0f / 1.5f) * Time.deltaTime;
			}
		}

		void OnRenderImage( RenderTexture source, RenderTexture destination)
		{
			if( _material == null)
			{
				_material = new Material( Shader.Find( "Hidden/kode80/PixelRender/IrisWipe"));
				_material.hideFlags = HideFlags.HideAndDontSave;
			}

			float w = destination.width;
			float h = destination.height;
			Vector2 aspect = w > h ? new Vector2( 1.0f, h / w) : new Vector2( w / h, 1.0f);
			Vector2 nCenter = Vector2.Scale( center, aspect);
			float m = Mathf.Max( Mathf.Max( nCenter.x, aspect.x - nCenter.x),
								 Mathf.Max( nCenter.y, aspect.y - nCenter.y));
			float maxRadius = Mathf.Sqrt( m * m + m * m);

			_material.SetVector( "_Aspect", aspect);
			_material.SetVector( "_Center", nCenter);
			_material.SetFloat( "_Position", position);
			_material.SetFloat( "_MaxRadius", maxRadius);

			Graphics.Blit( source, destination, _material);
		}
	}
}
