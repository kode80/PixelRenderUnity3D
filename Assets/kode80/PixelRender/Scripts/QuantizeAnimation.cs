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

namespace kode80.PixelRender
{
	[RequireComponent(typeof(Animator))]
	public class QuantizeAnimation : MonoBehaviour 
	{
		public string animationName;
		public float speed = 1.0f;
		public int fps = 10;

		private Animator _animator;
		private float _time;

		// Use this for initialization
		void Start () {
		
		}

		void OnEnable()
		{
			_animator = GetComponent<Animator>();
			_time = 0.0f;
		}

		void OnDisable()
		{
			_animator = null;
		}
		
		void Update () 
		{
			_time += Time.deltaTime;
			if( _time > speed)
			{
				_time -= speed;
			}

			float frame = Mathf.Round( _time / speed * fps);
			frame *= 1.0f /fps;

			_animator.Play( animationName, -1, frame);
			_animator.speed = 0.0f;
		}
	}
}