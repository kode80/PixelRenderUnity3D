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
using System;
using System.Collections;

namespace kode80.Utils
{
	public class Color32Util
	{
		public static Color32 FromRGBAUint32( UInt32 color)
		{
			return new Color32( (byte) ((color >> 24) & 0xff),
								(byte) ((color >> 16) & 0xff),
								(byte) ((color >> 8) & 0xff),
								(byte) (color & 0xff));
		}
	}
}
