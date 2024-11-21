using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace RTSGame
{
	public class SyntiosTerrainData : MonoBehaviour
	{

		//64 * 64 = 4096
		public int size_x = 64;
		public int size_y = 64;
		public sbyte[] cliffLevel; //-128 to 128 (only -8 to 8 used)
        public byte[] heightVariation; //0 to 255


        private void Start()
		{
			
		}

		private void Update()
		{
			
		}
		
		private void OnEnable()
		{
			
		}
	}
}