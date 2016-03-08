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
using kode80.GUIWrapper;

namespace kode80.PixelRender
{
	public class GUISpriteSheetClips : GUIVertical 
	{
		private Animator[] _animators;
		public Animator[] animators {
			get { return _animators; }
			set {
				_animators = FilterAnimatorsWithClips( value);
				UpdateGUI();
			}
		}

		private GUIPopup[] _guiClips;

		public GUISpriteSheetClips( OnGUIAction action=null)
		{
			if( action != null)
			{
				onGUIAction += action;
			}
		}

		public void SampleAnimations( float normalizedTime)
		{
			int count = _animators.Length;

			for( int i=0; i<count; i++)
			{
				int clipIndex = _guiClips[i].selectedIndex;

				if( clipIndex > 0)
				{
					clipIndex--;
					Animator animator = _animators[i];
					AnimationClip clip = animator.runtimeAnimatorController.animationClips[ clipIndex];

					AnimationMode.BeginSampling();
					AnimationMode.SampleAnimationClip( animator.gameObject, clip, normalizedTime * clip.length);
					AnimationMode.EndSampling();
				}
			}
		}

		private Animator[] FilterAnimatorsWithClips( Animator[] animators)
		{
			if( animators == null) { return null; }

			List<Animator> filteredList = new List<Animator>();

			foreach( Animator animator in animators) 
			{
				if( animator.runtimeAnimatorController != null && 
					animator.runtimeAnimatorController.animationClips != null) 
				{
					filteredList.Add( animator);
				}
			}

			Animator[] filteredAnimators = new Animator[filteredList.Count];
			filteredList.CopyTo( filteredAnimators);

			return filteredAnimators;
		}

		private void UpdateGUI()
		{
			RemoveAll();

			int count = _animators != null ? _animators.Length : 0;

			_guiClips = new GUIPopup[ count];

			if( count < 1) { return; }

			GUIFoldout foldout = Add( new GUIFoldout( new GUIContent( "Animators"))) as GUIFoldout;
			for( int i=0; i<count; i++)
			{
				GUIContent[] options = GetAnimatorDisplayedOptions( _animators[i]);
				_guiClips[i] = new GUIPopup( new GUIContent( "Animator " + i), options, 1, ChangeHandler);

				foldout.Add( _guiClips[i]);
			}
		}

		private GUIContent[] GetAnimatorDisplayedOptions( Animator animator)
		{
			int count = animator.runtimeAnimatorController.animationClips.Length;
			GUIContent[] options = new GUIContent[ count + 1];
			options[0] = new GUIContent( "None");

			for( int i=0; i<count; i++) {
				options[ i+1] = new GUIContent( animator.runtimeAnimatorController.animationClips[i].name);
			}

			return options;
		}

		private void ChangeHandler( GUIBase sender)
		{
			CallGUIAction();
		}
	}
}
