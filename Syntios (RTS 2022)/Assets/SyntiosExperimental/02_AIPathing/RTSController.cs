using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using System;

namespace ProtoRTS
{


	public class RTSController : MonoBehaviour
	{

        [SerializeField] private Vector2 mapSize = new Vector2(256, 256);




		public static RTSController Instance;

        private void Awake()
        {
			Instance = this;
        }


		private void Start()
		{
			
		}


        public Vector2 MapSize { get => mapSize; set => mapSize = value; }

		public static Vector3 MapSize1
        {
			get
            {
				return Instance.mapSize;
            }
        }


    }
}