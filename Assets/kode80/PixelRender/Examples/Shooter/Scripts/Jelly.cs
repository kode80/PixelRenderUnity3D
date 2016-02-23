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
using kode80.PixelRender;

namespace kode80.PixelRender.ShooterExample
{
	public class Jelly : MonoBehaviour 
	{
		public int life = 1;
		public float flashDuration = 0.5f;
		public Material deadMaterial;
		private bool _isAlive = true;
		private Vector3 _velocity;
		private Vector3 _angularVelocity;
		private QuantizedVector3 _quantizedLocalEuler;
		private float _flashCounter = 0.0f;
		private Material _originalMaterial;

		// Use this for initialization
		void Start () {
			_velocity = Vector3.zero;
			_angularVelocity = Vector3.zero;

			float q = 360.0f / 8.0f;
			_quantizedLocalEuler.quantizeStep = new Vector3( q, q, q);
			_quantizedLocalEuler.offset = transform.localEulerAngles;
			_quantizedLocalEuler.realValue = transform.localEulerAngles;

			_originalMaterial = GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial;
		}
		
		// Update is called once per frame
		void Update () 
		{
			if( _isAlive)
			{
				if( _flashCounter > 0.0f)
				{
					_flashCounter -= Time.deltaTime;
					if( _flashCounter <= 0.0f)
					{
						GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial = _originalMaterial;
						_flashCounter = 0.0f;
					}
				}
			}
			else
			{
				_velocity.x += 1.0f * Time.deltaTime;
				_velocity.y -= 1.0f * Time.deltaTime;
				_velocity.z = -90.0f * Time.deltaTime;
				_angularVelocity.y -= 30.0f * Time.deltaTime;
			}

			transform.position += _velocity;

			_quantizedLocalEuler.realValue += _angularVelocity;
			transform.localEulerAngles = _quantizedLocalEuler.quantizedValue;

			if( transform.position.y < -5.0f)
			{
				Destroy( gameObject);
			}
		}

		public void Kill()
		{
			life--;
			_isAlive = life > 0;
			GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial = deadMaterial;
			_flashCounter = flashDuration;
			transform.localScale += Vector3.one * 0.2f;
		}
	}
}