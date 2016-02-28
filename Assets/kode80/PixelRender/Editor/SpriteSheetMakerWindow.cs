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
		private GUIVertical _gui;

		[MenuItem( "Window/kode80/PixelRender/Sprite Sheet Maker")]
		public static void Init()
		{
			SpriteSheetMakerWindow win = EditorWindow.GetWindow( typeof( SpriteSheetMakerWindow)) as SpriteSheetMakerWindow;
			win.titleContent = new GUIContent( "Sprite Sheet Maker");
			win.Show();
		}

		void OnEnable()
		{
			_gui = new GUIVertical();
		}

		void OnDisable()
		{
			_gui = null;
		}

		void OnGUI()
		{
			if( _gui != null)
			{
				_gui.OnGUI();
			}
		}
	}
}
