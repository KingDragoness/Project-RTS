using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ProtoRTS
{
	public class Map : MonoBehaviour
	{

		[SerializeField] private SyntiosTerrainData _terrainData;



		public static Vector2 MapSize
        {
            get
            {
                return new Vector2(TerrainData.size_x, TerrainData.size_y);
            }
        }

        public static SyntiosTerrainData TerrainData
        {
            get { return instance._terrainData; }
        }

        public static Map instance;

        private void Awake()
        {
            instance = this;
        }
    }
}