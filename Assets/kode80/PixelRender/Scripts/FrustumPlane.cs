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
	[RequireComponent(typeof(Camera))]
	[ExecuteInEditMode]
	public class FrustumPlane : MonoBehaviour 
	{
		public Transform planeTransform;
		private Camera _camera;
		private bool _isIntersecting;
		private Vector3 _blIntersect;
		private Vector3 _brIntersect;
		private Vector3 _tlIntersect;
		private Vector3 _trIntersect;


		void OnEnable () 
		{
			_camera = GetComponent<Camera>();
		}
		
		void Update () 
		{
			CalculateIntersect();
		}

		void OnDrawGizmos()
		{
			if( _isIntersecting)
			{
				Gizmos.DrawLine( _blIntersect, _brIntersect);
				Gizmos.DrawLine( _brIntersect, _trIntersect);
				Gizmos.DrawLine( _trIntersect, _tlIntersect);
				Gizmos.DrawLine( _tlIntersect, _blIntersect);
			}
		}

		void CalculateIntersect()
		{
			Plane plane = new Plane( planeTransform.forward, planeTransform.position);
			Ray blRay = _camera.ViewportPointToRay( new Vector3( 0.0f, 0.0f));
			Ray brRay = _camera.ViewportPointToRay( new Vector3( 1.0f, 0.0f));
			Ray tlRay = _camera.ViewportPointToRay( new Vector3( 0.0f, 1.0f));
			Ray trRay = _camera.ViewportPointToRay( new Vector3( 1.0f, 1.0f));
			bool blIntersects = false;
			bool brIntersects = false;
			bool tlIntersects = false;
			bool trIntersects = false;

			blIntersects = PlaneRayIntersect( plane, blRay, out _blIntersect);
			brIntersects = PlaneRayIntersect( plane, brRay, out _brIntersect);
			tlIntersects = PlaneRayIntersect( plane, tlRay, out _tlIntersect);
			trIntersects = PlaneRayIntersect( plane, trRay, out _trIntersect);

			_isIntersecting = blIntersects && brIntersects && tlIntersects && trIntersects;
		}

		private bool PlaneRayIntersect( Plane plane, Ray ray, out Vector3 point)
		{
			bool result = false;
			float enter = 0.0f;
			point = Vector3.zero;

			if( result = plane.Raycast( ray, out enter))
			{
				point = ray.origin + ray.direction * enter;
			}

			return result;
		}
	}
}
