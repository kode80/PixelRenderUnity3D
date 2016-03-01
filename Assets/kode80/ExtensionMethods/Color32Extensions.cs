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

namespace kode80.ExtensionMethods
{
	public static class Color32Extensions
	{
		public static UInt32 ToRGBAUInt32( this Color32 color)
		{
			return ((UInt32)color.r << 24) | 
				   ((UInt32)color.g << 16) | 
				   ((UInt32)color.b << 8) | 
				   (UInt32)color.a;
		}
	}
}