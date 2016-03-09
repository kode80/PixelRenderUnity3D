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
using kode80.Utils;

namespace kode80.PixelRender
{
	public class SpriteSheetMakerWindow : EditorWindow
	{
		private const string _RootName = "com.kode80.PixelRender.SpriteSheetMaker.RootGameObject";
		private const string _ModelName = "com.kode80.PixelRender.SpriteSheetMaker.ModelGameObject";
		private const string _CameraName = "com.kode80.PixelRender.SpriteSheetMaker.PreviewCamera";
		private const string _SpriteSheetExportFolder = "PixelRenderSpriteSheets";
		private const int _PreviewLayer = 31;

		private GUIHorizontal _gui;
		private GUIScrollView _guiSide;
		private GUIIntSlider _guiFrameCount;
		private GUIIntSlider _guiFrameWidth;
		private GUIIntSlider _guiFrameHeight;
		private GUISlider _guiFOV;
		private GUIIntSlider _guiCurrentFrame;
		private GUISlider _guiDuration;
		private GUIToggle _guiPlay;
		private GUIVector3Field _guiPositionOffset;
		private GUISlider _guiScaleOffset;
		private GUIVector3Field _guiStartRotation;
		private GUIVector3Field _guiEndRotation;
		private GUIIntSlider _guiLoopCount;
		private GUIToggle _guiPingPong;
		private GUISpriteSheetClips _guiAnimationClips;
		private GUISpriteSheetMaterials _guiMaterials;
		private GUITextField _guiSpriteSheetName;
		private GUIButton _guiExport;
		private GUIVertical _guiPreview;

		private RenderTexture _previewTexture;
		private Camera _previewCamera;
		private PixelOutlineEffect _previewOutline;
		private GameObject _rootGameObject;
		private GameObject _modelGameObject;
		private float _lastFrameTime;


		[MenuItem( "Window/kode80/PixelRender/Sprite Sheet Maker")]
		public static void Init()
		{
			SpriteSheetMakerWindow win = EditorWindow.GetWindow( typeof( SpriteSheetMakerWindow)) as SpriteSheetMakerWindow;
			win.titleContent = new GUIContent( "Sprite Sheet Maker");
			win.Show();
		}

		void OnEnable()
		{
			CreateExportFolderIfNeeded();

			_lastFrameTime = Time.realtimeSinceStartup;

			_gui = new GUIHorizontal();

			GUIVertical sideContainer = _gui.Add( new GUIVertical( GUILayout.MaxWidth(290.0f))) as GUIVertical;
			_guiSide = sideContainer.Add( new GUIScrollView()) as GUIScrollView;

			GUIObjectField<GameObject> guiGameObject = _guiSide.Add( new GUIObjectField<GameObject>( new GUIContent( "GameObject", "GameObject to render as sprite sheet"),
				true, GameObjectChanged)) as GUIObjectField<GameObject>;
			_guiFrameCount = _guiSide.Add( new GUIIntSlider( new GUIContent( "Frame Count", "Number of frames in the sprite sheet"),
				12, 1, 64, FrameCountChanged)) as GUIIntSlider;
			_guiFrameWidth = _guiSide.Add( new GUIIntSlider( new GUIContent( "Frame Width", "Width of each frame in the sprite sheet"),
				100, 32, 512, ResizeFrame)) as GUIIntSlider;
			_guiFrameHeight = _guiSide.Add( new GUIIntSlider( new GUIContent( "Frame Height", "Height of each frame in the sprite sheet"),
				100, 32, 512, ResizeFrame)) as GUIIntSlider;
			_guiFOV = _guiSide.Add( new GUISlider( new GUIContent( "FOV"), 20, 1, 179, OffsetChanged)) as GUISlider;

			_guiSide.Add( new GUISpace());
			_guiCurrentFrame = _guiSide.Add( new GUIIntSlider( new GUIContent( "Current Frame"), 
				0, 0, _guiFrameCount.value-1, RenderPreviewAction)) as GUIIntSlider;
			_guiDuration = _guiSide.Add( new GUISlider( new GUIContent( "Duration"), 
				1, 0, 100, RenderPreviewAction)) as GUISlider;
			_guiPlay = _guiSide.Add( new GUIToggle( new GUIContent( "Play"))) as GUIToggle;

			_guiSide.Add( new GUISpace());
			GUIFoldout offsetFoldout = _guiSide.Add( new GUIFoldout( new GUIContent( "Position/Scale"))) as GUIFoldout;
			_guiPositionOffset = offsetFoldout.Add( new GUIVector3Field( new GUIContent( "Position Offset"), OffsetChanged)) as GUIVector3Field;
			_guiScaleOffset = offsetFoldout.Add( new GUISlider( new GUIContent( "Scale Offset"), 0.0f, -10.0f, 10.0f, OffsetChanged)) as GUISlider;

			_guiAnimationClips = _guiSide.Add( new GUISpriteSheetClips( RenderPreviewAction)) as GUISpriteSheetClips;
			_guiMaterials = _guiSide.Add( new GUISpriteSheetMaterials( RenderPreviewAction)) as GUISpriteSheetMaterials;

			GUIFoldout rotationFoldout = _guiSide.Add( new GUIFoldout( new GUIContent( "Rotation"))) as GUIFoldout;
			_guiStartRotation = rotationFoldout.Add( new GUIVector3Field( new GUIContent( "Start Rotation"), RenderPreviewAction)) as GUIVector3Field;
			_guiEndRotation = rotationFoldout.Add( new GUIVector3Field( new GUIContent( "End Rotation"), RenderPreviewAction)) as GUIVector3Field;

			GUIFoldout loopFoldout = _guiSide.Add( new GUIFoldout( new GUIContent( "Rotation/Material Looping"))) as GUIFoldout;
			_guiLoopCount = loopFoldout.Add( new GUIIntSlider( new GUIContent( "Loop Count"), 1, 1, 10, RenderPreviewAction)) as GUIIntSlider;
			_guiPingPong = loopFoldout.Add( new GUIToggle( new GUIContent( "Pingpong"), RenderPreviewAction)) as GUIToggle;

			GUIFoldout outlineFoldout = _guiSide.Add( new GUIFoldout( new GUIContent( "Outline Effect"))) as GUIFoldout;
			GUIColorField outlineColor = outlineFoldout.Add( new GUIColorField( new GUIContent( "Color"), 
				OutlineColorChanged)) as GUIColorField;
			GUISlider outlineThreshold = outlineFoldout.Add( new GUISlider( new GUIContent( "Threshold"), 
				0.05f, 0.0f, 0.05f, OutlineThresholdChanged)) as GUISlider;

			_guiSide.Add( new GUISpace());
			_guiSpriteSheetName = _guiSide.Add( new GUITextField( new GUIContent( "Sprite Sheet Name"))) as GUITextField;
			_guiExport = _guiSide.Add( new GUIButton( new GUIContent( "Export Sprite Sheet"), ExportSpriteSheet)) as GUIButton;

			_guiPreview = _gui.Add( new GUIVertical( GUILayout.ExpandWidth( true), GUILayout.ExpandHeight( true))) as GUIVertical;
			_guiPreview.shouldStoreLastRect = true;

			InitPreviewRenderTexture();
			InitPreviewCamera();
			InitRootGameObject();
			guiGameObject.value = _modelGameObject;
			GameObjectChanged( guiGameObject);
			RenderPreview( 0);

			_guiStartRotation.vector = Vector3.zero;
			_guiEndRotation.vector = Vector3.zero;
			outlineColor.color = _previewOutline.outlineColor;
			outlineThreshold.value = _previewOutline.depthThreshold;
		}

		void OnDisable()
		{
			_gui = null;
			_guiSide = null;
			_guiFrameCount = null;
			_guiFrameWidth = null;
			_guiFrameHeight = null;
			_guiCurrentFrame = null;
			_guiPositionOffset = null;
			_guiScaleOffset = null;
			_guiAnimationClips = null;
			_guiAnimationClips = null;
			_guiMaterials = null;
			_guiStartRotation = null;
			_guiEndRotation = null;
			_guiLoopCount = null;
			_guiPingPong = null;
			_guiSpriteSheetName = null;
			_guiExport = null;
			_guiPreview = null;
		}

		void Update()
		{
			float fps = _guiDuration.value / _guiFrameCount.value;

			if( _lastFrameTime <= Time.realtimeSinceStartup - fps)
			{
				float delta = Time.realtimeSinceStartup - _lastFrameTime;
				_lastFrameTime = Time.realtimeSinceStartup;

				if( _modelGameObject != null && _guiPlay.isToggled)
				{
					int frames = (int)(delta / fps);
					int nextFrame = (_guiCurrentFrame.value + frames) % _guiFrameCount.value;
					_guiCurrentFrame.value = nextFrame;
						
					RenderPreview( _guiCurrentFrame.value);
				}
			}
		}

		void OnGUI()
		{
			if( _gui != null)
			{
				EditorGUIUtility.labelWidth = 110.0f;

				_gui.OnGUI();

				if( Event.current.rawType == EventType.Repaint && _previewTexture != null && _previewCamera != null)
				{
					Rect rect = SizeRectToFit( new Rect( 0.0f, 0.0f, _previewCamera.pixelWidth, _previewTexture.height),
											   _guiPreview.lastRect);
					EditorGUI.DrawPreviewTexture( rect, _previewTexture);
				}
			}
		}

		#region GUI Callbacks

		private void GameObjectChanged( GUIBase sender)
		{
			GUIObjectField<GameObject> gameObjectField = sender as GUIObjectField<GameObject>;
			_guiSpriteSheetName.value = GetExportNameForGameObject( gameObjectField.value);

			if( _modelGameObject != null)
			{
				_modelGameObject.transform.parent = null;
				DestroyImmediate( _modelGameObject);
				_modelGameObject = null;
			}

			if( gameObjectField.value != null)
			{
				_modelGameObject = GameObject.Instantiate( gameObjectField.value);
				_modelGameObject.name = _ModelName;
				_modelGameObject.transform.parent = _rootGameObject.transform;
				_modelGameObject.transform.localPosition = Vector3.zero;

				Transform[] children = _modelGameObject.GetComponentsInChildren<Transform>(true);
				foreach( Transform child in children)
				{
					child.gameObject.hideFlags = HideFlags.HideAndDontSave;
					child.gameObject.layer = _PreviewLayer;
				}

				ScaleModelToFitCamera();

				_guiAnimationClips.animators = _modelGameObject.GetComponentsInChildren<Animator>( true);
				_guiMaterials.materials = GetUniquePixelArtMaterials( _modelGameObject);
			}
			else 
			{
				_guiAnimationClips.animators = null;
				_guiMaterials.materials = null;
			}

			_guiExport.isEnabled = gameObjectField.value != null;
			RenderPreview( _guiCurrentFrame.value);
		}

		private void ResizeFrame( GUIBase sender)
		{
			if( _previewTexture == null || _previewCamera == null) { return; }

			InitPreviewRenderTexture();
			_previewCamera.targetTexture = _previewTexture;

			ScaleModelToFitCamera();
			RenderPreview( _guiCurrentFrame.value);
		}

		private void FrameCountChanged( GUIBase sender)
		{
			_guiCurrentFrame.maxValue = _guiFrameCount.value - 1;
			_guiCurrentFrame.value = Math.Min( _guiFrameCount.value, _guiCurrentFrame.value);
		}

		private void OffsetChanged( GUIBase sender)
		{
			ScaleModelToFitCamera();
			RenderPreview( _guiCurrentFrame.value);
		}

		private void RenderPreviewAction( GUIBase sender)
		{
			RenderPreview( _guiCurrentFrame.value);
		}

		private void OutlineColorChanged( GUIBase sender)
		{
			GUIColorField color = sender as GUIColorField;
			_previewOutline.outlineColor = color.color;
			RenderPreview( _guiCurrentFrame.value);
		}

		private void OutlineThresholdChanged( GUIBase sender)
		{
			GUISlider threshold = sender as GUISlider;
			_previewOutline.depthThreshold = threshold.value;
			RenderPreview( _guiCurrentFrame.value);
		}

		private void ExportSpriteSheet( GUIBase sender)
		{
			int frameCount = _guiFrameCount.value;
			Vector2 sheetDimensions = GetSheetDimensions();
			RenderTexture sheet = new RenderTexture( (int)sheetDimensions.x, (int)sheetDimensions.y, 0);
			Color oldBackground = _previewCamera.backgroundColor;
			_previewCamera.backgroundColor = Color.clear;

			RenderTexture.active = sheet;
			GL.Clear( true, true, Color.clear);
			RenderTexture.active = null;

			for( int i=0; i<frameCount; i++)
			{
				RenderPreview( i);
				RenderTexture.active = sheet;
				GL.PushMatrix();
				GL.LoadPixelMatrix( 0, sheet.width, sheet.height, 0);
				Graphics.DrawTexture( GetFrameRect( i), _previewTexture);
				GL.PopMatrix();
				RenderTexture.active = null;

				EditorUtility.DisplayProgressBar( "Rendering", "Rendering frames", (float)i / (float)frameCount);
			}
			_previewCamera.backgroundColor = oldBackground;
			RenderTexture.active = sheet;
			Texture2D sheetTexture = new Texture2D( sheet.width, sheet.height);
			sheetTexture.ReadPixels( new Rect( Vector2.zero, new Vector2( sheet.width, sheet.height)), 0, 0);
			RenderTexture.active = null;

			string path = GetExportPath();
			SaveTexture( sheetTexture, path);
			AssetDatabase.Refresh( ImportAssetOptions.ForceUpdate);
			UpdateSpriteImportSettings( path);

			EditorUtility.ClearProgressBar();

			// Needed to display original background color
			RenderPreview( _guiCurrentFrame.value);
		}

		#endregion

		private void CreateExportFolderIfNeeded()
		{
			string folder = "Assets/" + _SpriteSheetExportFolder;

			if( AssetDatabase.IsValidFolder( folder) == false) {
				AssetDatabase.CreateFolder( "Assets", _SpriteSheetExportFolder);
			}
		}

		private string GetExportNameForGameObject( GameObject gameObject)
		{
			if( gameObject == null) { return ""; }

			string name = AssetDatabase.GetAssetPath( gameObject);
			string postfix = "";

			if( name.Length < 1) {
				name = AssetDatabase.GetAssetOrScenePath( gameObject); 
				postfix = "_" + gameObject.name;
			}

			name = Path.GetFileNameWithoutExtension( name);
			name += postfix;

			return name;
		}

		private string GetExportPath()
		{
			string name = _guiSpriteSheetName.value;

			if( name == null || name.Length < 1) { name = "PixelRenderSpriteSheet"; }

			string path = "Assets/" + _SpriteSheetExportFolder + "/" + name + ".png";
			path = AssetDatabase.GenerateUniqueAssetPath( path);

			return path;
		}

		private void SaveTexture( Texture2D texture, string assetPath)
		{
			string dataPath = Application.dataPath.Substring( 0, Application.dataPath.Length - "/Assets".Length);
			string path = Path.Combine( dataPath, assetPath);

			File.WriteAllBytes( path, texture.EncodeToPNG());
		}

		private void UpdateSpriteImportSettings( string path)
		{
			int frameCount = _guiFrameCount.value;
			SpriteMetaData[] spriteData = new SpriteMetaData[ frameCount];
			for( int i=0; i<frameCount; i++)
			{
				spriteData[i].name = "Sprite_ " + i;
				spriteData[i].rect = GetFrameRect( i, true);
			}

			TextureImporter importer = AssetImporter.GetAtPath( path) as TextureImporter;
			importer.textureType = TextureImporterType.Sprite;
			importer.spriteImportMode = SpriteImportMode.Multiple;
			importer.spritesheet = spriteData;
			importer.textureFormat = TextureImporterFormat.AutomaticTruecolor;
			importer.filterMode = FilterMode.Point;
			importer.SaveAndReimport();
			AssetDatabase.ImportAsset( path);
		}

		private void InitPreviewRenderTexture()
		{
			_previewTexture = new RenderTexture( _guiFrameWidth.value, _guiFrameHeight.value, 0);
			_previewTexture.filterMode = FilterMode.Point;
			_previewTexture.hideFlags = HideFlags.HideAndDontSave;
		}

		private void InitPreviewCamera()
		{

			GameObject cameraGO = GameObject.Find( _CameraName);
			if( cameraGO == null)
			{
				cameraGO = EditorUtility.CreateGameObjectWithHideFlags( _CameraName, 
																		HideFlags.HideAndDontSave, 
																		typeof(Camera), 
																		typeof( PixelOutlineEffect));
			}

			cameraGO.hideFlags = HideFlags.HideAndDontSave;

			_previewCamera = cameraGO.GetComponent<Camera>();
			_previewOutline = cameraGO.GetComponent<PixelOutlineEffect>();
			_previewOutline.outlineColor = Color.black;
			_previewOutline.depthThreshold = 0.001f;

			_previewCamera.targetTexture = _previewTexture;
			_previewCamera.clearFlags = CameraClearFlags.SolidColor;
			_previewCamera.backgroundColor = Color.gray;
			_previewCamera.cullingMask = 1 << _PreviewLayer;
			_previewCamera.transform.position = new Vector3( 0.0f, 0.0f, -6.0f);
			_previewCamera.enabled = false;
		}

		private void InitRootGameObject()
		{
			_rootGameObject = GameObject.Find( _RootName);
			if( _rootGameObject == null)
			{
				_rootGameObject = EditorUtility.CreateGameObjectWithHideFlags( _RootName, 
																			   HideFlags.HideAndDontSave);
			}

			// Use transform.Find, as model may be deactivated
			Transform transform = _rootGameObject.transform.Find( _ModelName);
			_modelGameObject = transform == null ? null : transform.gameObject;
			if( _modelGameObject != null)
			{
				_modelGameObject.transform.localPosition = Vector3.zero;
				_modelGameObject.layer = _PreviewLayer;
				_modelGameObject.SetActive( false);
			}

			_rootGameObject.transform.position = Vector3.zero;
			_rootGameObject.layer = _PreviewLayer;
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
					input.xMax > container.xMax || input.yMax > container.yMax))
			{
				input.x = Mathf.Max( input.x, container.x);
				input.y = Mathf.Max( input.y, container.y);
				input.width = Mathf.Min( input.x + input.width, container.width);
				input.height = Mathf.Min( input.y + input.height, container.height);
			}

			return input;
		}

		private void ScaleModelToFitCamera()
		{
			if( _modelGameObject == null || _previewCamera == null) { return; }

			Bounds bounds = GetBounds( _modelGameObject);
			float maxDimension = Mathf.Max( bounds.extents.x, Mathf.Max( bounds.extents.y, bounds.extents.z)) * 2.0f;

			_previewCamera.fieldOfView = _guiFOV.value;

			float distance = Mathf.Abs( _previewCamera.transform.position.z);
			Vector3 bottomLeft = _previewCamera.ViewportToWorldPoint( new Vector3( 0, 0, distance));
			Vector3 topRight = _previewCamera.ViewportToWorldPoint( new Vector3( 1, 1, distance));
			Vector3 delta = topRight - bottomLeft;
			float minViewDimension = Mathf.Min( Mathf.Abs( delta.x), Mathf.Abs( delta.y));
			float scale = minViewDimension / maxDimension;
			scale += _guiScaleOffset.value;

			_modelGameObject.transform.localScale = new Vector3( scale, scale, scale);
			Vector3 center = (bounds.center + _guiPositionOffset.vector) * -scale;
			_modelGameObject.transform.position = center;
		}

		private Bounds GetBounds( GameObject gameObject)
		{
			gameObject.transform.localScale = Vector3.one;
			gameObject.transform.position = Vector3.zero;

			Bounds bounds = new Bounds();
			Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>( false);

			foreach( Renderer renderer in renderers)
			{
				bounds.Encapsulate( renderer.bounds);
			}

			return bounds;
		}

		private int GetFramesPerRow()
		{
			return (int) Mathf.Ceil( Mathf.Sqrt( _guiFrameCount.value));
		}

		private Vector2 GetSheetDimensions()
		{
			int framesPerRow = GetFramesPerRow();
			int framesPerColumn = (int) Mathf.Ceil( (float)_guiFrameCount.value / (float)framesPerRow);
			return new Vector2( framesPerRow * _previewTexture.width, framesPerColumn * _previewTexture.height);
		}

		private void SetupFrame( int frame)
		{
			float time = (float)frame / (float)(_guiFrameCount.value - 1);
			if( float.IsNaN( time)) { time = 0.0f; }

			float loopedTime = TimeUtil.Loop( time, _guiLoopCount.value - 1, _guiPingPong.isToggled);

			_rootGameObject.transform.localEulerAngles = Vector3.Lerp( _guiStartRotation.vector, _guiEndRotation.vector, loopedTime);
			_guiMaterials.UpdateMaterials( loopedTime);

			_guiAnimationClips.SampleAnimations( time);
		}

		private Material[] GetUniquePixelArtMaterials( GameObject gameObject)
		{
			Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>( true);
			HashSet<Material> uniqueMaterialsSet = new HashSet<Material>();
			foreach( Renderer renderer in renderers)
			{
				if( renderer.sharedMaterial.shader.name == "kode80/PixelRender/PixelArtShader")
				{
					uniqueMaterialsSet.Add( renderer.sharedMaterial);
				}
			}

			Material[] uniqueMaterials = new Material[ uniqueMaterialsSet.Count];
			uniqueMaterialsSet.CopyTo( uniqueMaterials);

			return uniqueMaterials;
		}

		private Rect GetFrameRect( int frameIndex, bool bottomToTop=false)
		{
			Vector2 sheetDimensions = GetSheetDimensions();
			int framesPerRow = GetFramesPerRow();
			int w = _previewTexture.width;
			int h = _previewTexture.height;
			int x = (frameIndex % framesPerRow) * w;
			int y = (frameIndex / framesPerRow) * h;

			if( bottomToTop) { y = (int)sheetDimensions.y - y - h; }

			return new Rect( x, y, w, h);
		}

		private void RenderPreview( int frame)
		{
			if( _modelGameObject != null)
			{
				_modelGameObject.SetActive( true);
				AnimationMode.StartAnimationMode();
				SetupFrame( frame);
				_previewCamera.Render();
				AnimationMode.StopAnimationMode();
				_modelGameObject.SetActive( false);
			}
		}
	}
}
