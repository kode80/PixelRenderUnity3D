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
using System.Collections;
using kode80.GUIWrapper;

namespace kode80.PixelRender
{
	public class GUISpriteSheetMaterials : GUIVertical 
	{
		private Material[] _materials;
		public Material[] materials {
			get { return _materials; }
			set {
				_materials = value;
				UpdateGUI();
			}
		}

		private GUIVector3Field[] _guiStartLights;
		private GUIVector3Field[] _guiEndLights;
		private GUISlider[] _guiStartPaletteMixs;
		private GUISlider[] _guiEndPaletteMixs;
		private GUISlider[] _guiDithers;

		public GUISpriteSheetMaterials( OnGUIAction action=null)
		{
			if( action != null)
			{
				onGUIAction += action;
			}
		}

		public void UpdateMaterials( float normalizedTime)
		{
			int count = _materials.Length;
			Vector3 light;
			float mix;
			for( int i=0; i<count; i++)
			{
				light = Vector3.Lerp( _guiStartLights[i].vector, _guiEndLights[i].vector, normalizedTime);
				_materials[i].SetVector( "_LightDir", new Vector4( light.x, light.y, light.z));

				mix = Mathf.Lerp( _guiStartPaletteMixs[i].value, _guiEndPaletteMixs[i].value, normalizedTime);
				_materials[i].SetFloat( "_PaletteMix", mix);

				_materials[i].SetFloat( "_DitherThreshold", _guiDithers[i].value);
			}
		}

		private void UpdateGUI()
		{
			RemoveAll();

			int count = _materials != null ? _materials.Length : 0;

			_guiStartLights = new GUIVector3Field[ count];
			_guiEndLights = new GUIVector3Field[ count];
			_guiStartPaletteMixs = new GUISlider[ count];
			_guiEndPaletteMixs = new GUISlider[ count];
			_guiDithers = new GUISlider[ count];

			if( count < 1) { return; }

			Vector4 light;

			GUIFoldout foldout = Add( new GUIFoldout( new GUIContent( "PixelArt Materials"))) as GUIFoldout;
			for( int i=0; i<count; i++)
			{
				_guiStartLights[i] = new GUIVector3Field( new GUIContent( "Start Light Direction"), ChangeHandler) as GUIVector3Field;
				_guiEndLights[i] = new GUIVector3Field( new GUIContent( "End Light Direction"), ChangeHandler) as GUIVector3Field;
				_guiStartPaletteMixs[i] = new GUISlider( new GUIContent( "Start Palette Mix"), 0, 0, 1, ChangeHandler) as GUISlider;
				_guiEndPaletteMixs[i] = new GUISlider( new GUIContent( "End Palette Mix"), 0, 0, 1, ChangeHandler) as GUISlider;
				_guiDithers[i] = new GUISlider( new GUIContent( "Dither Threshold"), 0, 0, 1, ChangeHandler) as GUISlider;

				light = _materials[i].GetVector( "_LightDir");
				_guiStartLights[i].vector = _guiEndLights[i].vector = new Vector3( light.x, light.y, light.z);

				_guiStartPaletteMixs[i].value = _guiEndPaletteMixs[i].value = _materials[i].GetFloat( "_PaletteMix");
				_guiDithers[i].value = _materials[i].GetFloat( "_DitherThreshold");

				foldout.Add( _guiStartLights[i]);
				foldout.Add( _guiEndLights[i]);
				foldout.Add( _guiStartPaletteMixs[i]);
				foldout.Add( _guiEndPaletteMixs[i]);
				foldout.Add( _guiDithers[i]);

				if( i < count-1) { foldout.Add( new GUISpace()); }
			}
		}

		private void ChangeHandler( GUIBase sender)
		{
			CallGUIAction();
		}
	}
}
