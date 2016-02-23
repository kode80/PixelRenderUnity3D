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
	[ExecuteInEditMode]
	public class TargetCamera : MonoBehaviour 
	{
		public Transform target;
		public float distance = 10.0f;
		private Vector3 _forwardTarget;
		private Vector3 _positionTarget;

		// Use this for initialization
		void Start () {
			_forwardTarget = transform.forward;
			_positionTarget = transform.position;
		}
		
		// Update is called once per frame
		void LateUpdate () 
		{
			_forwardTarget = (target.position - transform.position).normalized;
			_forwardTarget.y = transform.forward.y;

			_positionTarget = target.position - _forwardTarget * distance;
			_positionTarget.y = transform.position.y;

			transform.forward = Vector3.Lerp( transform.forward, _forwardTarget, Time.deltaTime * 2.0f);
			//transform.position = Vector3.Lerp( transform.position, _positionTarget, Time.deltaTime * 2.0f);
		}
	}
}