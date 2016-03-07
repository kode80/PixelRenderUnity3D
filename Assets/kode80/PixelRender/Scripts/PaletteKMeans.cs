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
using kode80.Utils;
using kode80.ExtensionMethods;

namespace kode80.PixelRender
{
	public class PaletteKMeans 
	{
		private class Cluster
		{
			public List<UInt32> colors;
			public Vector3 centroid;

			public void CalcRange( out Color32 range, out Color32 min, out Color32 max)
			{
				min = new Color32( byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
				max = new Color32( byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue);

				foreach( UInt32 c in colors)
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

			public void RecalcCentroid()
			{
				centroid = Vector3.zero;
				foreach( UInt32 color in colors)
				{
					centroid.x += Color32Util.GetR( color);
					centroid.y += Color32Util.GetG( color);
					centroid.z += Color32Util.GetB( color);
				}

				centroid.x = centroid.x / (float)colors.Count;
				centroid.y = centroid.y / (float)colors.Count;
				centroid.z = centroid.z / (float)colors.Count;
			}

			public UInt32 ClosestColorToCentroid()
			{
				float distance = float.MaxValue;
				UInt32 closestColor = colors[0];
				foreach( UInt32 color in colors)
				{
					float newDistance = Distance( color);
					if( newDistance < distance)
					{
						distance = newDistance;
						closestColor = color;
					}
				}
				return closestColor;
			}

			public float Distance( UInt32 color)
			{
				UInt32 centroidColor = new Color32( (byte)centroid.x, (byte)centroid.y, (byte)centroid.z, 255).ToRGBAUInt32();
				return ColorDistance( centroidColor, color);
			}

			public Cluster()
			{
				colors = new List<UInt32>();
			}

			public static float ColorDistance( UInt32 a, UInt32 b)
			{
				Vector3 e1 = new Vector3( Color32Util.GetR( a), Color32Util.GetG( a), Color32Util.GetB( a));
				Vector3 e2 = new Vector3( Color32Util.GetR( b), Color32Util.GetG( b), Color32Util.GetB( b));
				Vector3 rel = e2 - e1; 
				Vector3 d1 = new Vector3(2, 3, 4) - e1; 
				Vector3 d2 = new Vector3(2, 3, 4) - e2; 

				d1.Normalize(); 
				d2.Normalize(); 
				double ang = 2.0 - Math.Abs(Vector3.Dot(d1, d2)); 
				double length = rel.magnitude; 

				return (float)(length * ang); 
			}
		}

		private struct MoveOp
		{
			public UInt32 color;
			public Cluster from;
			public Cluster to;

			public void Execute()
			{
				from.colors.Remove( color);
				to.colors.Add( color);
			}
		}

		public List<UInt32> palette;
		private List<Cluster> _clusters;

		public PaletteKMeans( List<UInt32> colors, int targetCount, int maxIterations)
		{
			MoveOp[] moveOps = new MoveOp[ colors.Count*2];
			int moveCount = 1;

			targetCount = Math.Min( targetCount, colors.Count);

			_clusters = new List<Cluster>();

			System.Random rng = new System.Random();
			int n = colors.Count;  
			while (n > 1) {  
				n--;  
				int k = rng.Next(n + 1);  
				UInt32 value = colors[k];  
				colors[k] = colors[n];  
				colors[n] = value;  
			}  

			int i;
			for( i=0; i<targetCount; i++)
			{
				Cluster cluster = new Cluster();
				cluster.colors.Add( colors[i]);
				_clusters.Add( cluster);
				cluster.RecalcCentroid();
			}

			colors.RemoveRange( 0, targetCount);

			while( colors.Count > 0)
			{
				UInt32 color = colors[0];
				colors.RemoveRange( 0, 1);

				float distance = float.MaxValue;
				Cluster bestCluster = _clusters[0];
				foreach( Cluster cluster in _clusters)
				{
					float newDistance = cluster.Distance( color);
					if( newDistance < distance)
					{
						distance = newDistance;
						bestCluster = cluster;
					}
				}
				bestCluster.colors.Add( color);
			}

			foreach( Cluster cluster in _clusters) {
				cluster.RecalcCentroid();
			}

			i=0;
			while( i < maxIterations && moveCount > 0)
			{
				moveCount = 0;
				foreach( Cluster cluster in _clusters)
				{
					if( cluster.colors.Count <= 1) { continue; }

					foreach( UInt32 color in cluster.colors)
					{
						float currentDistance = cluster.Distance( color);

						foreach( Cluster compareCluster in _clusters)
						{
							if( cluster == compareCluster) { continue; }
								
							float newDistance = compareCluster.Distance( color);
							if( newDistance < currentDistance)
							{
								moveOps[ moveCount].color = color;
								moveOps[ moveCount].from = cluster;
								moveOps[ moveCount].to = compareCluster;
								moveCount++;
							}
						}
					}
				}

				for( int j=0; j<moveCount; j++) {
					moveOps[j].Execute();
				}

				foreach( Cluster cluster in _clusters) {
					cluster.RecalcCentroid();
				}

				i++;
			}

			palette = CreatePalette();
		}

		public List<UInt32> CreatePalette()
		{
			List<UInt32> pal = new List<UInt32>();

			foreach( Cluster cluster in _clusters)
			{
				pal.Add( cluster.ClosestColorToCentroid());
			}

			return pal;
		}

		public int GetIndex( UInt32 input)
		{
			int i=0;
			float minDelta = float.MaxValue;
			int index = 0;
			foreach( UInt32 color in palette)
			{
				float delta = Cluster.ColorDistance( color, input);

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
