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
	public class Bullet : MonoBehaviour 
	{
		public float speed = 40.0f;
		public float lifeTime = 2.0f;
		public GameObject explosion;

		private BoxCollider _boxCollider;

		void Start()
		{
			_boxCollider = GetComponent<BoxCollider>();
		}

		void Update () 
		{
			float distance = speed * Time.deltaTime;
			Vector3 halfExtents = _boxCollider.size / 2.0f;
			int layer = 1 << LayerMask.NameToLayer( "Enemy");

#if UNITY_5_3
            RaycastHit[] hits = Physics.BoxCastAll( transform.position, halfExtents, Vector3.right, Quaternion.identity, distance, layer);
#else
            RaycastHit[] hits = Physics.SphereCastAll(transform.position, halfExtents.x, Vector3.right, distance, layer);
#endif

            if ( hits.Length > 0)
			{
				foreach( RaycastHit hit in hits)
				{
					hit.collider.gameObject.GetComponent<Jelly>().Kill();
				}
				Instantiate( explosion, transform.position, Quaternion.identity);
				Destroy( gameObject);
			}
			else
			{
				transform.position += Vector3.right * distance;
				lifeTime -= Time.deltaTime;
				if( lifeTime <= 0.0f)
				{
					Destroy( gameObject);
				}
			}
		}
	}
}