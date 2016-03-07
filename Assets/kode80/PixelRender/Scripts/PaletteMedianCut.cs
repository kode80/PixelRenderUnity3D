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
using System.Collections.Generic;
using kode80.ExtensionMethods;
using kode80.Utils;

namespace kode80.PixelRender
{
	public class PaletteMedianCut
	{
		private class Bucket 
		{
			private List<UInt32> _colors;
			public Bucket()
			{
				_colors = new List<UInt32>();
			}

			public int Count()
			{
				return _colors.Count;
			}

			public void Add( List<UInt32> colors)
			{
				_colors.AddRange( colors);
			}

			public void CalcRange( out Color32 range)
			{
				Color32 min, max;
				CalcRange( out range, out min, out max);
			}

			public void CalcRange( out Color32 range, out Color32 min, out Color32 max)
			{
				min = new Color32( byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
				max = new Color32( byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue);

				foreach( UInt32 c in _colors)
				{
					Color32 color = Color32Util.FromRGBAUint32( c);
					min.r = Math.Min( color.r, min.r);
					min.g = Math.Min( color.g, min.g);
					min.b = Math.Min( color.b, min.b);

					max.r = Math.Max( color.r, max.r);
					max.g = Math.Max( color.g, max.g);
					max.b = Math.Max( color.b, max.b);
				}

				range = new Color32( (byte)(max.r - min.r), 
									 (byte)(max.g - min.g), 
									 (byte)(max.b - min.b), 
									 0);
			}

			public byte BiggestChannel( Color32 range)
			{
				return Math.Max( range.r, Math.Max( range.g, range.b));
			}

			public void SortColors( Color32 range)
			{
				byte channel = BiggestChannel( range);
				if( range.r == channel)
				{
					_colors.Sort( (a,b) => Color32Util.GetR( a).CompareTo( Color32Util.GetR( b)));
				}
				else if( range.g == channel)
				{
					_colors.Sort( (a,b) => Color32Util.GetG( a).CompareTo( Color32Util.GetG( b)));
				}
				else
				{
					_colors.Sort( (a,b) => Color32Util.GetB( a).CompareTo( Color32Util.GetB( b)));
				}
			}

			public void Split( out Bucket left, out Bucket right)
			{
				Color32 range;
				CalcRange( out range);
				SortColors( range);

				left = new Bucket();
				right = new Bucket();

				if( _colors.Count == 1)
				{
					left.Add( _colors);
				}
				else if( _colors.Count >= 2)
				{
					int totalCount = _colors.Count;
					int leftCount = totalCount / 2;

					left.Add( _colors.GetRange( 0, leftCount));
					right.Add( _colors.GetRange( leftCount, totalCount - leftCount));
				}
			}

			public UInt32 MiddleColor()
			{
				int r = 0;
				int g = 0;
				int b = 0;

				Color32 range, min, max;
				CalcRange( out range, out min, out max);

				r = min.r + range.r / 2;
				g = min.g + range.g / 2;
				b = min.b + range.b / 2;

				return new Color32( (byte)r, (byte)g, (byte)b, 255).ToRGBAUInt32();
			}

			public int MiddleDelta( UInt32 color)
			{
				int count = _colors.Count;
				if( count == 0) { return int.MaxValue; }
				UInt32 middleColor = _colors[ count / 2];

				int deltaR = Math.Abs( Color32Util.GetR( color) - Color32Util.GetR( middleColor));
				int deltaG = Math.Abs( Color32Util.GetG( color) - Color32Util.GetG( middleColor));
				int deltaB = Math.Abs( Color32Util.GetB( color) - Color32Util.GetB( middleColor));

				return deltaR + deltaG + deltaB;
			}
		}

		private List<Bucket> _buckets;
		public List<UInt32> palette;

		public PaletteMedianCut( List<UInt32> colors, int targetCount)
		{
			_buckets = new List<Bucket>();

			Bucket bucket = new Bucket();
			bucket.Add( colors);
			_buckets.Add( bucket);

			while( _buckets.Count < targetCount)
			{
				List<Bucket> newBuckets = new List<Bucket>();
				foreach( Bucket currentBucket in _buckets)
				{
					Bucket left, right;
					currentBucket.Split( out left, out right);
					newBuckets.Add( left);
					newBuckets.Add( right);
				}
				_buckets = newBuckets;
			}

			palette = CreatePalette();
		}

		public List<UInt32> CreatePalette()
		{
			int count = _buckets.Count;
			List<UInt32> pal = new List<UInt32>();

			for( int i=0; i<count; i++)
			{
				if( _buckets[i].Count() > 0)
				{
					pal.Add( _buckets[i].MiddleColor());
				}
			}

			return pal;
		}

		public int GetIndex( UInt32 input)
		{
			int i=0;
			int minDelta = int.MaxValue;
			int index = 0;
			foreach( UInt32 color in palette)
			{
				int deltaR = Math.Abs( Color32Util.GetR( color) - Color32Util.GetR( input));
				int deltaG = Math.Abs( Color32Util.GetG( color) - Color32Util.GetG( input));
				int deltaB = Math.Abs( Color32Util.GetB( color) - Color32Util.GetB( input));

				int delta = deltaR + deltaG + deltaB;

				if( delta < minDelta)
				{
					minDelta = delta;
					index = i;
				}

				i++;
			}

			return index;
		}
	}
}