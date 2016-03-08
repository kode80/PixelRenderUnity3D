//  Copyright (c) 2016, Ben Hopkins (kode80)
//  All rights reserved.
//  
//  Redistribution and use in source and binary forms, with or without modification, 
//  are permitted provided that the following conditions are met:
//  
//  1. Redistributions of source code must retain the above copyright notice, 
//     this list of conditions and the following disclaimer.
//  
//  2. Redistributions in binary form must reproduce the above copyright notice, 
//     this list of conditions and the following disclaimer in the documentation 
//     and/or other materials provided with the distribution.
//  
//  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY 
//  EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF 
//  MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL 
//  THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, 
//  SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT 
//  OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) 
//  HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT 
//  (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, 
//  EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using UnityEngine;
using System.Collections;

namespace kode80.Utils
{
	public class TimeUtil
	{
		/// <summary>
		/// Used to loop animations. Takes a normalized time (0.0 to 1.0) and
		/// returns the sub normalized time based on number of loops.
		/// </summary>
		/// <param name="normalizedTime">Master normalized time (0.0 to 1.0).</param>
		/// <param name="loopCount">Number of times to loop the animation, 0 means input is unchanged, 1 means '2 plays' etc.</param>
		/// <param name="pingPong">If set to <c>true</c> every 2nd loop will be reversed.</param>
		public static float Loop( float normalizedTime, int loopCount, bool pingPong=false)
		{
			float loopedTime = normalizedTime * (float)(loopCount + 1);
			float subTime = loopedTime - Mathf.Floor( loopedTime);

			if( pingPong && ((int)loopedTime % 2) == 1) {
				subTime = 1.0f - subTime;
			}

			return subTime;
		}
	}
}
