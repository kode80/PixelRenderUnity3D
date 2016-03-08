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
using System.Reflection;
using kode80.GUIWrapper;

namespace kode80.PixelRender
{
	public class PaletteEditor : EditorWindow 
	{
		enum PaletteEditMode {
			Single,
			Row,
			Column,
			All
		};

		private const string PaletteControlName = "PaletteColor";
		private const string PrefsKeyMode = "kode80.PixelRender.PaletteEditor.Mode";
		private const string PrefsKeyHue = "kode80.PixelRender.PaletteEditor.Hue";
		private const string PrefsKeySaturation = "kode80.PixelRender.PaletteEditor.Saturation";
		private const string PrefsKeyLuminance = "kode80.PixelRender.PaletteEditor.Luminance";

		private GUIVertical _gui;
		private GUITextureField _guiTexture;
		private GUIEnumPopup _guiEditMode;
		private List<GUIColorField> _guiColorFields;
		private GUISlider _guiHue;
		private GUISlider _guiSaturation;
		private GUISlider _guiLuminance;
		private bool _paletteDirty;
		private int _selectedColorX;
		private int _selectedColorY;

		[MenuItem( "Window/kode80/PixelRender/Palette Editor")]
		public static void Init()
		{
			PaletteEditor win = EditorWindow.GetWindow( typeof( PaletteEditor)) as PaletteEditor;
			win.titleContent = new GUIContent( "Palette Editor");
			win.Show();
		}

		void OnEnable()
		{
			RebuildGUI( null);
			Undo.undoRedoPerformed += UndoRedoPerformed;
		}

		void OnDisable()
		{
			SavePaletteIfDirty();
			StorePrefs();

			Undo.undoRedoPerformed -= UndoRedoPerformed;
			_gui = null;
			_guiTexture = null;
			_guiColorFields = null;
			_guiEditMode = null;
			_guiHue = null;
			_guiSaturation = null;
			_guiLuminance = null;
		}

		void OnGUI()
		{
			if( _gui != null)
			{
				_gui.OnGUI();
				UpdateSelectedColorCoords();
			}
		}

		#region GUI Actions

		private void PaletteTextureWillChange( GUIBase sender)
		{
			SavePaletteIfDirty();
		}

		private void PaletteTextureChanged( GUIBase sender)
		{
			_selectedColorX = _selectedColorY = 0;
			Texture2D texture = _guiTexture == null ? null : _guiTexture.texture;
			RebuildGUI( texture);
		}

		private void ShadeCountChanged( GUIBase sender)
		{
			GUIIntSlider slider = sender as GUIIntSlider;
			Texture2D palette = _guiTexture.texture;

			if( palette == null) { return; }

			if( IsTextureReadWrite( palette))
			{
				Undo.RecordObject( palette, "Resize Palette");

				int oldSize = palette.width * palette.height;
				int newSize = palette.width * slider.value;
				int copySize = Math.Min( oldSize, newSize);

				Color32[] pixels = palette.GetPixels32();
				Color32[] newPixels = new Color32[ newSize];
				Array.Copy( pixels, oldSize - copySize, 
							newPixels, newSize - copySize, 
							copySize);

				palette.Resize( palette.width, slider.value);
				palette.SetPixels32( newPixels);
				palette.Apply();
				_paletteDirty = true;

				RebuildGUI( palette);

				string path = AssetDatabase.GetAssetPath( Shader.Find( "kode80/PixelRender/PixelArtShader"));
				AssetDatabase.ImportAsset( path, ImportAssetOptions.ForceUpdate);
			}
			else
			{
				HandleTextureNotReadWrite();
			}
		}

		private void ColorChanged( GUIBase sender)
		{
			GUIColorField color = sender as GUIColorField;

			Texture2D texture = _guiTexture.texture;

			if( texture == null) { return; }

			if( IsTextureReadWrite( texture))
			{
				Undo.RecordObject( texture, "Edit Palette");

				_selectedColorX = color.tag % texture.width;
				_selectedColorY = color.tag / texture.width;
				ProcessColors( texture, _selectedColorX, _selectedColorY, (root, input, shade) => color.color);
			}
			else
			{
				HandleTextureNotReadWrite();
			}
		}

		private void GenerateShadesClicked( GUIBase sender)
		{
			Texture2D texture = _guiTexture.texture;

			if( texture == null) { return; }

			if( IsTextureReadWrite( texture))
			{
				Undo.RecordObject( texture, "Edit Palette");
				int lastShadeIndex = texture.height - 1;
				float h = _guiHue.value;
				float s = _guiSaturation.value;
				float l = _guiLuminance.value;

				PaletteEditMode mode = (PaletteEditMode)_guiEditMode.value;
				if( mode == PaletteEditMode.Single || mode == PaletteEditMode.Row)
				{
					ProcessColors( texture, _selectedColorX, _selectedColorY, (root, input, shade) => {
						return OffsetColorHSL( input, 1.0f, h, s, l);
					});
				}
				else
				{
					ProcessColors( texture, _selectedColorX, _selectedColorY, (root, input, shade) => {
						float a = (float)shade / (float)lastShadeIndex;
						return OffsetColorHSL( root, a, h, s, l);
					});
				}
			}
			else
			{
				HandleTextureNotReadWrite();
			}
		}

		private Color OffsetColorHSL( Color input, float offsetAlpha, 
									  float hueOffset, float saturationOffset, float luminanceOffset)
		{
			float h, s, l;
#if UNITY_5_3
            Color.RGBToHSV( input, out h, out s, out l);
#else
            EditorGUIUtility.RGBToHSV(input, out h, out s, out l);
#endif
            float newH = h + hueOffset * offsetAlpha;
			if( newH < 0.0f) { h = 1.0f + newH; }
			else if( newH > 1.0f) { h = newH - 1.0f; }
			else { h = newH;}

			s = Mathf.Clamp( s + saturationOffset * offsetAlpha, 0.0f, 1.0f);
			l = Mathf.Clamp( l + luminanceOffset * offsetAlpha, 0.0f, 1.0f);

#if UNITY_5_3
            return Color.HSVToRGB( h, s, l);
#else
            return EditorGUIUtility.HSVToRGB(h, s, l);
#endif
        }

#endregion

		private void ProcessColors( Texture2D palette, int colorX, int colorY, Func <Color, Color, int, Color> action)
		{
			int x0,y0,x1,y1;

			PaletteEditMode mode = (PaletteEditMode) _guiEditMode.value;
			switch( mode)
			{
			default:
			case PaletteEditMode.Single:
				x0 = colorX; 
				x1 = colorX + 1;
				y0 = colorY; 
				y1 = colorY + 1;
				break;
			case PaletteEditMode.Row:
				x0 = 0; 
				x1 = palette.width;
				y0 = colorY;
				y1 = colorY + 1;
				break;
			case PaletteEditMode.Column:
				x0 = colorX; 
				x1 = colorX + 1;
				y0 = 0;
				y1 = palette.height;
				break;
			case PaletteEditMode.All:
				x0 = 0; 
				x1 = palette.width;
				y0 = 0;
				y1 = palette.height;
				break;
			}

			int rootColorY = palette.height - 1;
			for( int y=y0; y<y1; y++) {
				for( int x=x0; x<x1; x++) {
					int shade = rootColorY - y;
					Color rootColor = palette.GetPixel( x, rootColorY);
					Color inputColor = palette.GetPixel( x, y);
					Color output = action( rootColor, inputColor, shade);

					palette.SetPixel( x, y, output);
					_guiColorFields[ shade * palette.width + x].color = output;
				}
			}

			palette.Apply();
			_paletteDirty = true;
		}

		private void RebuildGUI( Texture2D texture)
		{
			StorePrefs();

			_gui = new GUIVertical();
			GUIScrollView scroll = _gui.Add( new GUIScrollView()) as GUIScrollView;

			_guiTexture = scroll.Add( new GUITextureField( new GUIContent( "Palette Texture"), 
														 PaletteTextureChanged, 
														 PaletteTextureWillChange)) as GUITextureField;
			_guiTexture.texture = texture;

			if( texture == null) { return; }

			if( IsTextureReadWrite( texture))
			{
				_guiEditMode = scroll.Add( new GUIEnumPopup( new GUIContent( "Edit Mode", 
					"Palette edit operations affect either the single color or all colors in row/column/palette."), 
					PaletteEditMode.Single)) as GUIEnumPopup;
				
				scroll.Add( new GUIIntSlider( new GUIContent( "Shades", "Number of shades per color in palette"),
					texture.height, 2, 16, ShadeCountChanged));
				_guiColorFields = new List<GUIColorField>();

				// Adding 15 accounts for horizontal scrollbar if needed.
				// Need a non-hardcoded-hacky way of getting this info...
				float maxHeight = texture.height * 20.0f + 15.0f;
				GUIScrollView paletteScroll = scroll.Add( new GUIScrollView( GUILayout.MaxHeight( maxHeight))) as GUIScrollView;
				for( int y=texture.height-1; y>=0; y--)
				{
					GUIHorizontal horizontal = paletteScroll.Add( new GUIHorizontal()) as GUIHorizontal;
					for( int x=0; x<texture.width; x++)
					{
						GUIColorField color = horizontal.Add( new GUIColorField( null, ColorChanged)) as GUIColorField;
						color.color = texture.GetPixel( x, y);
						color.tag = y * texture.width + x;
						color.controlName = PaletteControlName + color.tag;
						_guiColorFields.Add( color);
					}
				}

				scroll.Add( new GUISpace());
				_guiHue = scroll.Add( new GUISlider( new GUIContent( "Hue Offset", 
					"Amount to offset hue when generating shades"), 0.0f, -1.0f, 1.0f)) as GUISlider;
				_guiSaturation = scroll.Add( new GUISlider( new GUIContent( "Saturation Offset", 
					"Amount to offset saturation when generating shades"), 0.0f, -1.0f, 1.0f)) as GUISlider;
				_guiLuminance = scroll.Add( new GUISlider( new GUIContent( "Luminance Offset", 
					"Amount to offset luminance when generating shades"), 0.0f, -1.0f, 1.0f)) as GUISlider;
				scroll.Add( new GUIButton( new GUIContent( "Generate Shades", "Generate shades from root colors"), GenerateShadesClicked));

				LoadPrefs();
			}
			else
			{
				HandleTextureNotReadWrite();
			}

			Repaint();
		}

		private void UpdateSelectedColorCoords()
		{
			if( _guiTexture != null && _guiTexture.texture != null)
			{
				string focusedControlName = GUI.GetNameOfFocusedControl();
				if( focusedControlName.Contains( PaletteControlName))
				{
					string indexString = focusedControlName.Substring( PaletteControlName.Length);
					int index = int.Parse( indexString);
					int x = index % _guiTexture.texture.width;
					int y = index / _guiTexture.texture.width;
					if( x < _guiTexture.texture.width && y < _guiTexture.texture.height)
					{
						_selectedColorX = x;
						_selectedColorY = y;
					}
				}
			}
		}

		private void SaveTextureAsset( Texture2D texture)
		{
			string assetPath = AssetDatabase.GetAssetPath( texture);
			string dataPath = Application.dataPath.Substring( 0, Application.dataPath.Length - "/Assets".Length);
			string path = Path.Combine( dataPath, assetPath);

			File.WriteAllBytes( path, texture.EncodeToPNG());
		}

		private bool IsTextureReadWrite( Texture2D texture)
		{
			bool isReadWrite = true;

			try { texture.GetPixel( 0, 0); }
			catch( AmbiguousMatchException) { isReadWrite = false; }

			return isReadWrite;
		}

		private void SavePaletteIfDirty()
		{
			if( _guiTexture.texture != null && _paletteDirty)
			{
				SaveTextureAsset( _guiTexture.texture);
				AssetDatabase.Refresh();
				_paletteDirty = false;
			}
		}

		private void HandleTextureNotReadWrite()
		{
			_guiTexture.texture = null;
			EditorUtility.DisplayDialog( "Error", 
										 "Texture is not Read/Write enabled. Enable Read/Write in Texture import settings and try again.", 
										 "OK");
		}

		private void UndoRedoPerformed()
		{
			RebuildGUI( _guiTexture == null ? null : _guiTexture.texture);
		}

		private void StorePrefs()
		{
			if( _guiEditMode != null) 
			{ EditorPrefs.SetInt( PrefsKeyMode, (int)(PaletteEditMode)_guiEditMode.value); }

			if( _guiHue != null) 
			{ EditorPrefs.SetFloat( PrefsKeyHue, _guiHue.value); }

			if( _guiSaturation != null) 
			{ EditorPrefs.SetFloat( PrefsKeySaturation, _guiSaturation.value); }

			if( _guiLuminance != null) 
			{ EditorPrefs.SetFloat( PrefsKeyLuminance, _guiLuminance.value); }
		}

		private void LoadPrefs()
		{
			if( _guiEditMode != null) 
			{ _guiEditMode.value = (PaletteEditMode) EditorPrefs.GetInt( PrefsKeyMode); }

			if( _guiHue != null) 
			{ _guiHue.value = EditorPrefs.GetFloat( PrefsKeyHue); }

			if( _guiSaturation != null) 
			{ _guiSaturation.value = EditorPrefs.GetFloat( PrefsKeySaturation); }

			if( _guiLuminance != null) 
			{ _guiLuminance.value = EditorPrefs.GetFloat( PrefsKeyLuminance); }
		}
	}
}
