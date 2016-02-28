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
using System.Collections;
using kode80.GUIWrapper;

namespace kode80.PixelRender
{
	public class SpriteSheetMakerWindow : EditorWindow
	{
		private GUIHorizontal _gui;
		private GUIVertical _guiSide;
		private GUIVertical _guiPreview;
		private RenderTexture _previewTexture;

		[MenuItem( "Window/kode80/PixelRender/Sprite Sheet Maker")]
		public static void Init()
		{
			SpriteSheetMakerWindow win = EditorWindow.GetWindow( typeof( SpriteSheetMakerWindow)) as SpriteSheetMakerWindow;
			win.titleContent = new GUIContent( "Sprite Sheet Maker");
			win.Show();
		}

		void OnEnable()
		{
			_gui = new GUIHorizontal();

			_guiSide = _gui.Add( new GUIVertical( GUILayout.MaxWidth(290.0f))) as GUIVertical;
			_guiSide.Add( new GUIButton( new GUIContent( "Test")));

			_guiPreview = _gui.Add( new GUIVertical( GUILayout.ExpandWidth( true), GUILayout.ExpandHeight( true))) as GUIVertical;
			_guiPreview.shouldStoreLastRect = true;

			_previewTexture = new RenderTexture( 100, 100, 0);
			RenderTexture.active = _previewTexture;
			Graphics.Blit( Texture2D.blackTexture, _previewTexture);
			RenderTexture.active = null;
		}

		void OnDisable()
		{
			_gui = null;
			_guiSide = null;
			_guiPreview = null;
			_previewTexture = null;
		}

		void OnGUI()
		{
			if( _gui != null)
			{
				_gui.OnGUI();

				if( Event.current.rawType == EventType.Repaint && _previewTexture != null)
				{
					EditorGUI.DrawPreviewTexture( _guiPreview.lastRect, _previewTexture);
				}
			}
		}
	}
}
