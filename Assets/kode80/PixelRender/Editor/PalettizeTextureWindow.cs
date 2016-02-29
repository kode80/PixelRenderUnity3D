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

namespace kode80.PixelRender
{
	public class PalettizeTextureWindow : EditorWindow
	{
		private const int ProgressUpdateFreq = 100000;

		private GUIVertical _gui;
		private GUITextureField _guiTexture;
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
			_guiTexture = _gui.Add( new GUITextureField( new GUIContent( "Texture"), TextureChanged)) as GUITextureField;
			_guiPalette = _gui.Add( new GUITextureField( new GUIContent( "Palette"), null)) as GUITextureField;
			_guiPalettizedTexture = _gui.Add( new GUITextureField( new GUIContent( "Palettized Texture"), null)) as GUITextureField;
			_guiSave = _gui.Add( new GUIButton( new GUIContent( "Save Palette & Texture"), SaveClicked)) as GUIButton;

			_guiPalette.isEnabled = false;
			_guiPalettizedTexture.isEnabled = false;
			_guiSave.isEnabled = false;
		}

		void OnDisable()
		{
			_gui = null;
			_guiTexture = null;
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

				Texture2D palette = PalettizeTexture( palettizedTexture, 4);

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

		private Texture2D PalettizeTexture( Texture2D texture, int shades)
		{
			Color32[] data = texture.GetPixels32();
			Color32[] uniqueColors = GetUniqueColors( data);
			if( uniqueColors == null)
			{
				return null;
			}

			int uniqueCount = uniqueColors.Length;

			for( int i=0; i<data.Length; i++)
			{
				byte paletteIndex = (byte) Array.IndexOf( uniqueColors, data[i]);
				paletteIndex = (byte) ( ((float)paletteIndex / (float)(uniqueCount - 1)) * 255.0f);
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

			return CreatePaletteTexture( uniqueColors, shades);
		}

		private Texture2D CreatePaletteTexture( Color32[] colors, int shades)
		{
			Texture2D palette = new Texture2D( colors.Length, shades);
			palette.filterMode = FilterMode.Point;
			palette.wrapMode = TextureWrapMode.Clamp;
			Color32[] data = palette.GetPixels32();

			for( int x=0; x<palette.width; x++)
			{
				for( int y=0; y<palette.height; y++)
				{
					data[ y * palette.width + x] = colors[x];
				}
			}

			palette.SetPixels32( data);
			palette.Apply();

			return palette;
		}

		private Color32[] GetUniqueColors( Color32[] data)
		{
			HashSet<Color32> hash = new HashSet<Color32>();

			for( int i=0; i<data.Length; i++)
			{
				hash.Add( data[ i]);

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

			Color32[] uniqueColors = new Color32[ hash.Count];
			hash.CopyTo( uniqueColors);

			return uniqueColors;
		}
	}
}
