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

namespace kode80.PixelRender.ShooterExample
{
	public class Explosion : MonoBehaviour 
	{
		private const float _duration = 0.5f;
		private float _counter = _duration;
		// Use this for initialization
		void Start () {
		
		}
		
		// Update is called once per frame
		void Update () {
			transform.localEulerAngles += new Vector3( 0.0f, 0.0f, 180.0f * Time.deltaTime);

			transform.localScale = Vector3.one * (_counter / _duration);

			if( _counter >= 0.0f)
			{
				_counter -= Time.deltaTime;
				if( _counter <= 0.0f)
				{
					Destroy( gameObject);
				}
			}
		}
	}
}