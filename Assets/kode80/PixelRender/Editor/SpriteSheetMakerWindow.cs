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
using System.IO;
using kode80.GUIWrapper;

namespace kode80.PixelRender
{
	public class SpriteSheetMakerWindow : EditorWindow
	{
		private const string _RootName = "com.kode80.PixelRender.SpriteSheetMaker.RootGameObject";
		private const string _ModelName = "com.kode80.PixelRender.SpriteSheetMaker.ModelGameObject";
		private const string _CameraName = "com.kode80.PixelRender.SpriteSheetMaker.PreviewCamera";
		private const int _PreviewLayer = 31;

		private GUIHorizontal _gui;
		private GUIVertical _guiSide;
		private GUIIntSlider _guiFrameCount;
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
			_lastFrameTime = Time.realtimeSinceStartup;

			_gui = new GUIHorizontal();

			_guiSide = _gui.Add( new GUIVertical( GUILayout.MaxWidth(290.0f))) as GUIVertical;
			_guiSide.Add( new GUIObjectField<GameObject>( new GUIContent( "GameObject", "GameObject to render as sprite sheet"),
														  true, GameObjectChanged));
			_guiFrameCount = _guiSide.Add( new GUIIntSlider( new GUIContent( "Frame Count", "Number of frames in the sprite sheet"),
				12, 1, 32)) as GUIIntSlider;
			_guiSide.Add( new GUIButton( new GUIContent( "Rotate"), RotateModel));
			_guiSide.Add( new GUIButton( new GUIContent( "Render"), RenderModel));

			_guiPreview = _gui.Add( new GUIVertical( GUILayout.ExpandWidth( true), GUILayout.ExpandHeight( true))) as GUIVertical;
			_guiPreview.shouldStoreLastRect = true;

			InitPreviewRenderTexture();
			InitPreviewCamera();
			InitRootGameObject();

			RenderPreview();
		}

		void OnDisable()
		{
			_gui = null;
			_guiSide = null;
			_guiPreview = null;
			_previewTexture = null;
			_rootGameObject = null;
			_modelGameObject = null;
		}

		void Update()
		{
			float fps = 1.0f / 10.0f;

			if( _lastFrameTime <= Time.realtimeSinceStartup - fps)
			{
				_lastFrameTime = Time.realtimeSinceStartup;
			}
		}

		void OnGUI()
		{
			if( _gui != null)
			{
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

			if( _modelGameObject != null)
			{
				_modelGameObject.transform.parent = null;
				DestroyImmediate( _modelGameObject);
			}

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

			RenderPreview();
		}

		private void RotateModel( GUIBase sender)
		{
			if( _rootGameObject == null) { return; }

			_rootGameObject.transform.localEulerAngles += new Vector3( 0.0f, 360.0f / _guiFrameCount.value, 0.0f);
			RenderPreview();
		}

		private void RenderModel( GUIBase sender)
		{
			int frameCount = _guiFrameCount.value;
			int sheetWidth = _previewCamera.pixelWidth * frameCount;
			int sheetHeight = _previewCamera.pixelHeight;
			RenderTexture sheet = new RenderTexture( sheetWidth, sheetHeight, 0);
			Rect rect = new Rect( Vector2.zero, new Vector2( _previewCamera.pixelWidth, _previewCamera.pixelHeight));
			Vector2 step = new Vector2( _previewCamera.pixelWidth, 0.0f);
			Color oldBackground = _previewCamera.backgroundColor;
			_previewCamera.backgroundColor = Color.clear;

			RenderTexture.active = sheet;
			GL.Clear( true, true, Color.clear);
			RenderTexture.active = null;

			for( int i=0; i<frameCount; i++)
			{
				RenderPreview();
				RenderTexture.active = sheet;
				GL.PushMatrix();
				GL.LoadPixelMatrix( 0, sheet.width, sheet.height, 0);
				Graphics.DrawTexture( rect, _previewTexture);
				GL.PopMatrix();
				RenderTexture.active = null;

				rect.position += step;
				_rootGameObject.transform.localEulerAngles += new Vector3( 0.0f, 360.0f / frameCount, 0.0f);
				EditorUtility.DisplayProgressBar( "Rendering", "Rendering frames", (float)i / (float)frameCount);
			}
			_previewCamera.backgroundColor = oldBackground;
			RenderTexture.active = sheet;
			Texture2D sheetTexture = new Texture2D( sheet.width, sheet.height);
			sheetTexture.ReadPixels( new Rect( Vector2.zero, new Vector2( sheet.width, sheet.height)), 0, 0);
			RenderTexture.active = null;

			string path = "Assets/TestSheet.png";
			SaveTexture( sheetTexture, path);
			AssetDatabase.Refresh();
			UpdateSpriteImportSettings( path);

			EditorUtility.ClearProgressBar();

			// Needed to display original background color
			RenderPreview();
		}

		#endregion

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
				spriteData[i].rect = new Rect( i * _previewTexture.width, 0, _previewTexture.width, _previewTexture.height);
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
			_previewTexture = new RenderTexture( 100, 100, 0);
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

			_previewCamera = cameraGO.GetComponent<Camera>();
			_previewOutline = cameraGO.GetComponent<PixelOutlineEffect>();
			_previewOutline.outlineColor = Color.black;
			_previewOutline.depthThreshold = 0.0001f;

			_previewCamera.targetTexture = _previewTexture;
			_previewCamera.clearFlags = CameraClearFlags.SolidColor;
			_previewCamera.backgroundColor = Color.gray;
			_previewCamera.cullingMask = 1 << _PreviewLayer;
			_previewCamera.transform.position = new Vector3( 0.0f, 0.0f, -6.0f);
			_previewCamera.enabled = false;
		}

		private void InitRootGameObject()
		{
			Debug.Log( "InitRootGameObject");
			_rootGameObject = GameObject.Find( _RootName);
			if( _rootGameObject == null)
			{
				Debug.Log("Creating preview root");
				_rootGameObject = EditorUtility.CreateGameObjectWithHideFlags( _RootName, 
																			   HideFlags.HideAndDontSave);
			}
			else
			{
				Debug.Log( "Root GameObject already exists");
			}

			// Use transform.Find, as model may be deactivated
			Transform transform = _rootGameObject.transform.Find( _ModelName);
			_modelGameObject = transform == null ? null : transform.gameObject;
			if( _modelGameObject == null)
			{
				Debug.Log("Creating model gameobject");

				_modelGameObject = GameObject.CreatePrimitive( PrimitiveType.Sphere);
				_modelGameObject.hideFlags = HideFlags.HideAndDontSave;
				_modelGameObject.name = _ModelName;
				_modelGameObject.transform.parent = _rootGameObject.transform;
			}
			else
			{
				Debug.Log("Model gameobject already exists");
			}

			_rootGameObject.transform.position = Vector3.zero;
			_rootGameObject.layer = _PreviewLayer;

			_modelGameObject.transform.localPosition = Vector3.zero;
			_modelGameObject.layer = _PreviewLayer;
			_modelGameObject.SetActive( false);
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
			maxDimension += maxDimension * 0.5f;

			float distance = Mathf.Abs( _previewCamera.transform.position.z);
			Vector3 bottomLeft = _previewCamera.ViewportToWorldPoint( new Vector3( 0, 0, distance));
			Vector3 topRight = _previewCamera.ViewportToWorldPoint( new Vector3( 1, 1, distance));
			Vector3 delta = topRight - bottomLeft;
			float minViewDimension = Mathf.Min( Mathf.Abs( delta.x), Mathf.Abs( delta.y));
			float scale = minViewDimension / maxDimension;

			_modelGameObject.transform.localScale = new Vector3( scale, scale, scale);
			Vector3 center = bounds.center * -scale;
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

		private void RenderPreview()
		{
			string type = _modelGameObject == null ? "null" : _modelGameObject.GetType().ToString();
			Debug.Log( "RenderPreview: " + type);
			if( _modelGameObject != null)
			{
				_modelGameObject.SetActive( true);
				_previewCamera.Render();
				_modelGameObject.SetActive( false);
			}
		}
	}
}
