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
	public class Composite : MonoBehaviour 
	{
		public Texture texture;

		IEnumerator OnPostRender()
		{
			if( texture != null)
			{
				yield return new WaitForEndOfFrame();

				Rect mainRect = new Rect( 0.0f, 0.0f, Camera.main.pixelWidth, Camera.main.pixelHeight);
				Rect alignedRect = AlignedRect( texture.width, texture.height, mainRect);

				GL.PushMatrix();
				GL.LoadPixelMatrix(0, mainRect.width, mainRect.height,0);
				Graphics.DrawTexture( alignedRect, texture);
				GL.PopMatrix();
			}
		}

		private Rect AlignedRect( float width, float height, Rect container)
		{
			while( width * 2.0f < container.width && height * 2.0 < container.height)
			{
				width *= 2.0f;
				height *= 2.0f;
			}
//			width = container.width;
//			height = container.height;

			return new Rect( Mathf.Floor( (container.width - width) * 0.5f),
							 Mathf.Floor( (container.height - height) * 0.5f),
							 width,
							 height);
		}
	}
}
