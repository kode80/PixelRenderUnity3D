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
	public struct QuantizedVector3
	{
		private Vector3 _quantizeStep;
		public Vector3 quantizeStep {
			get { return _quantizeStep; }
			set {
				_quantizeStep = value;
				UpdateQuantizedValue();
			}
		}

		private Vector3 _realValue;
		public Vector3 realValue {
			get { return _realValue; }
			set {
				_realValue = value;
				UpdateQuantizedValue();
			}
		}

		private Vector3 _offset;
		public Vector3 offset {
			get { return _offset; }
			set {
				_offset = value - Quantize( value);
				UpdateQuantizedValue();
			}
		}

		private Vector3 _quantizedValue;
		public Vector3 quantizedValue { get { return _quantizedValue; } }

		private void UpdateQuantizedValue()
		{
			_quantizedValue = Quantize( _realValue) + _offset;
		}

		private Vector3 Quantize( Vector3 input)
		{
			input.x = Mathf.Floor( input.x / _quantizeStep.x) * _quantizeStep.x;
			input.y = Mathf.Floor( input.y / _quantizeStep.y) * _quantizeStep.y;
			input.z = Mathf.Floor( input.z / _quantizeStep.z) * _quantizeStep.z;
			return input;
		}
	}
}
