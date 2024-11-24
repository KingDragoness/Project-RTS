using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using System;

namespace ProtoRTS.Game
{


	public class RTSController : MonoBehaviour
	{


		public static RTSController Instance;

        private void Awake()
        {
			Instance = this;
        }


		private void Start()
		{
			
		}


    }
}