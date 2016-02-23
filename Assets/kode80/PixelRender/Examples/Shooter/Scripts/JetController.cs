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
	public class JetController : MonoBehaviour 
	{
		public float speed = 30.0f;
		public float scrollSpeed = 8.0f;

		public GameObject bullet;
		public Transform bulletSpawn;

		private Vector3 _velocity;

		// Use this for initialization
		void Start () {
			_velocity = Vector3.zero;
		}
		
		// Update is called once per frame
		void Update () 
		{
			_velocity.x = Input.GetAxis( "Horizontal") * speed + scrollSpeed;
			_velocity.y = Input.GetAxis( "Vertical") * speed;

			Vector3 newPosition = transform.position + _velocity * Time.deltaTime;

			Vector3 collisionPoint, collisionNormal;
			if( CheckLevelCollision( transform.position, newPosition, out collisionPoint, out collisionNormal))
			{
				newPosition.y = collisionPoint.y + collisionNormal.y * 0.1f;
			}

			transform.position = newPosition;

			if( Input.GetButtonDown( "Fire"))
			{
				Instantiate( bullet, bulletSpawn.position, Quaternion.identity);
			}
		}

		bool CheckLevelCollision( Vector3 oldPosition, Vector3 newPosition, out Vector3 collisionPoint, out Vector3 collisionNormal)
		{
			Vector3 direction = newPosition - oldPosition;
			Ray ray = new Ray( transform.position, direction.normalized);
			RaycastHit hit;
			int layer = 1 << LayerMask.NameToLayer( "Level");

			if( Physics.Raycast( ray, out hit, direction.magnitude, layer))
			{
				collisionPoint = ray.origin + direction * hit.distance;
				collisionNormal = hit.normal;
				return true;
			}

			collisionPoint = Vector3.zero;
			collisionNormal = Vector3.zero;

			return false;
		}

		void OnDrawGizmos()
		{
			Gizmos.color = Color.red;
			Gizmos.DrawLine( transform.position, transform.position + _velocity.normalized * 2.0f);
		}
	}
}
