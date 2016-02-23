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
using System.Collections.Generic;

namespace kode80.PixelRender
{
	public class PixelArtShaderEditor : ShaderGUI 
	{
		private bool _isFirstRun = true;

		private MaterialProperty _texture = null;
		private MaterialProperty _normalMap = null;
		private MaterialProperty _palette = null;
		private MaterialProperty _lightDirection = null;
		private MaterialProperty _dither = null;
		private MaterialProperty _paletteMix = null;
		private MaterialProperty _palette2 = null;
		private bool _shadowsEnabled = false;

		public override void OnGUI (MaterialEditor editor, MaterialProperty[] props)
		{
			Material material = editor.target as Material;
			FindProperties( material, props);

			EditorGUI.BeginChangeCheck();
			{
				editor.TexturePropertySingleLine( new GUIContent( "Texture"), _texture);
				editor.TexturePropertySingleLine( new GUIContent( "Normal Map"), _normalMap);
				editor.TexturePropertySingleLine( new GUIContent( "Palette"), _palette);
				EditorGUILayout.Space();
				editor.TexturePropertySingleLine( new GUIContent( "Palette 2"), _palette2);
				editor.FloatProperty( _paletteMix, "Palette Mix");
				EditorGUILayout.Space();
				editor.VectorProperty( _lightDirection, "Light Direction");
				editor.FloatProperty( _dither, "Dither");
				_shadowsEnabled = EditorGUILayout.Toggle( "Shadows", _shadowsEnabled);
			}
			if( EditorGUI.EndChangeCheck())
			{
				SetKeywords( material);
			}

			if( _isFirstRun)
			{
				SetKeywords( material);
				_isFirstRun = false;
			}
		}

		public void FindProperties( Material material, MaterialProperty[] properties)
		{
			_texture = FindProperty( "_MainTex", properties);
			_normalMap = FindProperty( "_NormalTex", properties);
			_palette = FindProperty( "_PaletteTex", properties);
			_lightDirection = FindProperty( "_LightDir", properties);
			_dither = FindProperty( "_DitherThreshold", properties);
			_paletteMix = FindProperty( "_PaletteMix", properties);
			_palette2 = FindProperty( "_Palette2Tex", properties);
			_shadowsEnabled = material.IsKeywordEnabled( "_SHADOWS");
		}

		private void SetKeywords( Material material)
		{
			SetKeyword( material, "_NORMALMAP", material.GetTexture( "_NormalTex"));
			SetKeyword( material, "_PALETTEMIX", material.GetTexture( "_Palette2Tex"));
			SetKeyword( material, "_SHADOWS", _shadowsEnabled);
		}

		private void SetKeyword( Material material, string keyword, bool enabled)
		{
			if( enabled) { material.EnableKeyword( keyword); }
			else { material.DisableKeyword( keyword); }
		}
	}
}
