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
		private Camera _previewCamera;
		private PixelOutlineEffect _previewOutline;

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
			InitPreviewCamera();
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
					Rect rect = SizeRectToFit( new Rect( 0.0f, 0.0f, _previewCamera.pixelWidth, _previewTexture.height),
											   _guiPreview.lastRect);
					EditorGUI.DrawPreviewTexture( rect, _previewTexture);
				}
			}
		}

		private void InitPreviewCamera()
		{
			const string gameObjectName = "com.kode80.PixelRender.SpriteSheetMaker.PreviewCamera";

			GameObject cameraGO = GameObject.Find( gameObjectName);
			if( cameraGO == null)
			{
				cameraGO = EditorUtility.CreateGameObjectWithHideFlags( gameObjectName, 
																		HideFlags.HideAndDontSave, 
																		typeof(Camera), 
																		typeof( PixelOutlineEffect));
			}

			_previewCamera = cameraGO.GetComponent<Camera>();
			_previewOutline = cameraGO.GetComponent<PixelOutlineEffect>();

			_previewCamera.targetTexture = _previewTexture;
			_previewCamera.Render();
		}

		private Rect SizeRectToFit( Rect input, Rect container, bool clip=true)
		{
			float scale = Mathf.Min( container.width/input.width, container.height/input.height);
			scale = Mathf.Max( 1.0f, Mathf.Floor( scale));
			input.width *= scale;
			input.height *= scale;
			input.x = (container.width - input.width) * 0.5f + container.x;
			input.y = (container.height - input.height) * 0.5f + container.y;

			if( clip &&
				(input.x < container.x || input.y < container.y ||
				 input.right > container.right || input.bottom > container.bottom))
			{
				input.x = Mathf.Max( input.x, container.x);
				input.y = Mathf.Max( input.y, container.y);
				input.width = Mathf.Min( input.x + input.width, container.width);
				input.height = Mathf.Min( input.y + input.height, container.height);
			}

			return input;
		}
	}
}
