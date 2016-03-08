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
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using kode80.GUIWrapper;
using kode80.ExtensionMethods;
using kode80.Utils;

namespace kode80.PixelRender
{
	public class PalettizeTextureWindow : EditorWindow
	{
		private const int ProgressUpdateFreq = 100000;

		private GUIVertical _gui;
		private GUITextureField _guiTexture;
		private GUIIntSlider _guiMaxColors;
		private GUIButton _guiConvert;
		private GUITextureField _guiPalette;
		private GUITextureField _guiPalettizedTexture;
		private GUIButton _guiSave;

		[MenuItem( "Window/kode80/PixelRender/Palettize Texture")]
		public static void Init()
		{
			PalettizeTextureWindow win = EditorWindow.GetWindow( typeof( PalettizeTextureWindow)) as PalettizeTextureWindow;
			win.titleContent = new GUIContent( "Palettize Texture");
			win.Show();
		}

		void OnEnable()
		{
			_gui = new GUIVertical();
			GUIScrollView scroll = _gui.Add( new GUIScrollView()) as GUIScrollView;

			_guiTexture = scroll.Add( new GUITextureField( new GUIContent( "Texture"), TextureChanged)) as GUITextureField;
			_guiMaxColors = scroll.Add( new GUIIntSlider( new GUIContent( "Max Colors"), 32, 2, 32)) as GUIIntSlider;
			_guiConvert = scroll.Add( new GUIButton( new GUIContent( "Convert"), ConvertClicked)) as GUIButton;
			scroll.Add( new GUISpace());
			_guiPalette = scroll.Add( new GUITextureField( new GUIContent( "Palette"), null)) as GUITextureField;
			_guiPalettizedTexture = scroll.Add( new GUITextureField( new GUIContent( "Palettized Texture"), null)) as GUITextureField;
			_guiSave = scroll.Add( new GUIButton( new GUIContent( "Save Palette & Texture"), SaveClicked)) as GUIButton;

			_guiPalette.isEnabled = false;
			_guiPalettizedTexture.isEnabled = false;
			_guiSave.isEnabled = false;
			_guiConvert.isEnabled = false;
		}

		void OnDisable()
		{
			_gui = null;
			_guiTexture = null;
			_guiMaxColors = null;
			_guiConvert = null;
			_guiPalette = null;
			_guiPalettizedTexture = null;
			_guiSave = null;
		}

		void OnGUI()
		{
			if( _gui != null)
			{
				_gui.OnGUI();
			}
		}

		#region GUI Actions

		private void TextureChanged( GUIBase sender)
		{
			_guiConvert.isEnabled = _guiTexture.texture != null;
			_guiPalette.texture = null;
			_guiPalettizedTexture.texture = null;
			_guiSave.isEnabled = false;
		}

		private void ConvertClicked( GUIBase sender)
		{
			Texture2D texture = _guiTexture.texture;

			if( texture != null)
			{
				_guiSave.isEnabled = true;

				// Make sure texture is readable & non-compressed. Compressed textures
				// have artifacts which result in more unique colors being detected
				// than exist in the original, uncompressed texture.
				string texturePath = AssetDatabase.GetAssetPath( texture);
				UpdateTextureImportSettings( texturePath, false, true);
				texture = AssetDatabase.LoadAssetAtPath<Texture2D>( texturePath);
				_guiTexture.texture = texture;

				RenderTexture rt = new RenderTexture( texture.width, texture.height, 0);

				Graphics.Blit( texture, rt);

				RenderTexture.active = rt;
				Texture2D palettizedTexture = new Texture2D( texture.width, texture.height);
				palettizedTexture.filterMode = FilterMode.Point;
				palettizedTexture.wrapMode = TextureWrapMode.Clamp;
				palettizedTexture.ReadPixels( new Rect( 0.0f, 0.0f, texture.width, texture.height), 0, 0);
				palettizedTexture.Apply();
				RenderTexture.active = null;

				Texture2D palette = PalettizeTexture( palettizedTexture, _guiMaxColors.value, 4);

				if( palette == null)
				{
					_guiTexture.texture = null;
					_guiPalette.texture = null;
					_guiPalettizedTexture.texture = null;
				}
				else
				{
					_guiPalette.texture = palette;
					_guiPalettizedTexture.texture = palettizedTexture;
				}
			}
			else
			{
				_guiPalette.texture = null;
				_guiPalettizedTexture.texture = null;
				_guiSave.isEnabled = false;
			}
		}

		private void SaveClicked( GUIBase sender)
		{
			string texturePath = AssetDatabase.GetAssetPath( _guiTexture.texture);

			if( texturePath.Length > 0)
			{
				string palettePath = AddPathSuffix( texturePath, "_Palette");
				string palettizedTexturePath = AddPathSuffix( texturePath, "_Palettized");
				SaveTexture( _guiPalette.texture, palettePath);
				SaveTexture( _guiPalettizedTexture.texture, palettizedTexturePath);
				AssetDatabase.Refresh();

				UpdateTextureImportSettings( palettePath, false, true);
				UpdateTextureImportSettings( palettizedTexturePath, true);
			}
		}

		private void SaveTexture( Texture2D texture, string assetPath)
		{
			string dataPath = Application.dataPath.Substring( 0, Application.dataPath.Length - "/Assets".Length);
			string path = Path.Combine( dataPath, assetPath);

			File.WriteAllBytes( path, texture.EncodeToPNG());
		}

		private void UpdateTextureImportSettings( string path, bool bypassSRGB = false, bool readable = false)
		{
			TextureImporter importer = AssetImporter.GetAtPath( path) as TextureImporter;
			importer.textureType = TextureImporterType.Advanced;
			importer.mipmapEnabled = false;
			importer.npotScale = TextureImporterNPOTScale.None;
			importer.textureFormat = TextureImporterFormat.AutomaticTruecolor;
			importer.isReadable = readable;
			importer.filterMode = FilterMode.Point;
			importer.wrapMode = TextureWrapMode.Clamp;
			importer.linearTexture = bypassSRGB;
			importer.SaveAndReimport();
			AssetDatabase.ImportAsset( path);
		}

		#endregion

		string AddPathSuffix( string filename, string suffix)
		{
			string directory = Path.GetDirectoryName( filename);
			string name = Path.GetFileNameWithoutExtension( filename);
			string extension = Path.GetExtension( filename);
			return Path.Combine( directory, String.Concat( name, suffix, extension));
		}

		private Texture2D PalettizeTexture( Texture2D texture, int maxColors, int shades)
		{
			Color32[] data = texture.GetPixels32();
			UInt32[] uniqueColors = GetUniqueColors( data);
			if( uniqueColors == null)
			{
				return null;
			}

			PaletteKMeans kMeans = new PaletteKMeans( new List<UInt32>( uniqueColors), maxColors, 30);
			Dictionary<UInt32, int> paletteLUT = new Dictionary<UInt32, int>();
			foreach( UInt32 color in uniqueColors) {
				paletteLUT[ color] = kMeans.GetIndex( color);
			}

			int paletteCount = kMeans.palette.Count;

			for( int i=0; i<data.Length; i++)
			{
				byte paletteIndex = (byte) paletteLUT[ data[i].ToRGBAUInt32()];
				paletteIndex = (byte) ( ((float)paletteIndex / (float)(paletteCount - 1)) * 255.0f);
				data[ i] = new Color32( paletteIndex, paletteIndex, paletteIndex, 255);

				if( i % ProgressUpdateFreq == 0)
				{
					if( EditorUtility.DisplayCancelableProgressBar( "Converting", 
																	"Generating index texture", 
																	(float)i / (float) data.Length))
					{
						EditorUtility.ClearProgressBar();
						return null;
					}
				}
			}
			EditorUtility.ClearProgressBar();

			texture.SetPixels32( data);
			texture.Apply();

			return CreatePaletteTexture( kMeans.palette, shades);
		}

		private Texture2D CreatePaletteTexture( List<UInt32> colors, int shades)
		{
			Texture2D palette = new Texture2D( colors.Count, shades);
			palette.filterMode = FilterMode.Point;
			palette.wrapMode = TextureWrapMode.Clamp;
			Color32[] data = palette.GetPixels32();

			for( int x=0; x<palette.width; x++)
			{
				for( int y=0; y<palette.height; y++)
				{
					data[ y * palette.width + x] = Color32Util.FromRGBAUint32( colors[x]);
				}
			}

			palette.SetPixels32( data);
			palette.Apply();

			return palette;
		}

		private UInt32[] GetUniqueColors( Color32[] data)
		{
			HashSet<UInt32> hash = new HashSet<UInt32>();
			int i;
			for( i=0; i<data.Length; i++)
			{
				hash.Add( data[i].ToRGBAUInt32());

				if( i % ProgressUpdateFreq == 0)
				{
					if( EditorUtility.DisplayCancelableProgressBar( "Converting", 
																	"Calculating unique colors", 
																	(float)i / (float) data.Length))
					{
						EditorUtility.ClearProgressBar();
						return null;
					}
				}
			}

			UInt32[] uniqueColors = new UInt32[ hash.Count];
			hash.CopyTo( uniqueColors);
			return uniqueColors;
		}

		private Dictionary<UInt32, UInt32> GetHistogram( Color32[] data)
		{
			Dictionary<UInt32, UInt32> histogram = new Dictionary<UInt32, UInt32>();
			int i;
			UInt32 color;

			for( i=0; i<data.Length; i++)
			{
				color = data[i].ToRGBAUInt32();
				if( histogram.ContainsKey( color)) { histogram[ color]++; }
				else { histogram[ color] = 0; }

				if( i % ProgressUpdateFreq == 0)
				{
					if( EditorUtility.DisplayCancelableProgressBar( "Converting", 
						"Calculating unique colors", 
						(float)i / (float) data.Length))
					{
						EditorUtility.ClearProgressBar();
						return null;
					}
				}
			}

			int count = histogram.Count;
			UInt32[] colors = new UInt32[ count];
			int[] hues = new int[ count];
			int[] sats = new int[ count];
			int[] vals = new int[ count];
			i=0;
			foreach( KeyValuePair<UInt32, UInt32> kv in histogram)
			{
				float h, s, v;
				Color.RGBToHSV( Color32Util.FromRGBAUint32( kv.Key), 
								out h, out s, out v);
				
				colors[ i] = kv.Key;
				hues[ i] = (int)(h * 255.0f);
				sats[ i] = (int)(s * 255.0f);
				vals[ i] = (int)(v * 255.0f);
				i++;
			}

			for( i=0; i<count; i++)
			{
				int hueDelta = 0;
				int satDelta = 0;
				int valDelta = 0;
				color = colors[i];
				int hue = hues[i];
				int sat = sats[i];
				int val = vals[i];
				for( int j=0; j<count; j++)
				{
					hueDelta += Math.Abs( hues[j] - hue);
					satDelta += Math.Abs( sats[j] - sat);
					valDelta += Math.Abs( vals[j] - val);
				}

				histogram[ color] = (UInt32) (hueDelta + valDelta + satDelta);
			}

			return histogram;
		}

		private int ConvertHistogramToPaletteLUT( Dictionary<UInt32, UInt32> histogram, int maxColors)
		{
			List<KeyValuePair<UInt32, UInt32>> orderedHistogram = new List<KeyValuePair<UInt32, UInt32>>( histogram);
			orderedHistogram.Sort( (a, b) => b.Value.CompareTo( a.Value));

			int count = histogram.Count;
			UInt32 color;

			// Setup indices for most used colors
			int clippedCount = Math.Min( count, maxColors);
			for( int i=0; i<clippedCount; i++)
			{
				color = orderedHistogram[i].Key;
				histogram[ color] = (UInt32)i;
			}

			// Setup indices for any clipped colors
			for( int i=maxColors; i<count; i++)
			{
				UInt32 currentColor = orderedHistogram[i].Key;
				// Find closest color
				Int32 maxDelta = Int32.MaxValue;
				int bestIndex = 0;
				for( int j=0; j<maxColors; j++)
				{
					Int32 delta = CalcRGBDelta( currentColor, orderedHistogram[j].Value);
					if( delta < maxDelta)
					{
						maxDelta = delta;
						bestIndex = j;
					}
				}

				color = orderedHistogram[i].Key;
				histogram[ color] = (UInt32)bestIndex;
			}

			return clippedCount;
		}

		private int CalcRGBDelta( UInt32 a, UInt32 b)
		{
			int dr = Math.Abs( (int)((a >> 24) & 0xff) - (int)((b >> 24) & 0xff));
			int dg = Math.Abs( (int)((a >> 16) & 0xff) - (int)((b >> 16) & 0xff));
			int db = Math.Abs( (int)((a >> 8) & 0xff) - (int)((b >> 8) & 0xff));

			return dr + dg + db;
		}
	}
}
