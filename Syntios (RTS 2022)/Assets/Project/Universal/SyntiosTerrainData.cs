using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ProtoRTS
{

	[System.Serializable]
	public class SyntiosTerrainData
	{

		public string ID = "Earth";
		//256 * 256 = 65.536
		[Range(32, 256)] public int size_x = 64;
		[Range(32, 256)] public int size_y = 64;
		public sbyte[] cliffLevel; //-128 to 128 (only -8 to 8 used)
        public byte[] heightVariation; //0 to 255
		public byte[] fogOfWar;
		public bool[] manmadeCliffs;
		public byte[] terrain_layer1;
		public byte[] terrain_layer2;
		public byte[] terrain_layer3;
		public byte[] terrain_layer4;
		public byte[] terrain_layer5;
		public byte[] terrain_layer6;
		public byte[] terrain_layer7;
		public byte[] terrain_layer8;

		public int TotalLength
        {
			get 
			{
				int length = size_x * size_y;

				if (length > 90000)
                {
					throw new System.Exception("Size is too big!");
                }
				return length; 
			}
        }

		/// <summary>
		/// The game scans X first then Y.
		/// I.e: 64x96 size, if wants to find at (53, 44) then 53 + (44 * 64)
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public int GetIndex(int x, int y)
        {
			if (x > size_x) throw new System.Exception("Invalid X coord!");
			if (y > size_y) throw new System.Exception("Invalid Y coord!");
			if (x < 0) throw new System.Exception("Invalid X coord!");
			if (y < 0) throw new System.Exception("Invalid Y coord!");

			return x + (size_x * y);
		}

		[FoldoutGroup("DEBUG")]
		[Button("Initialize Data")]
		public void InitializeData()
        {
			cliffLevel = new sbyte[TotalLength];
			heightVariation = new byte[TotalLength];
			fogOfWar = new byte[TotalLength * 4];
			manmadeCliffs = new bool[TotalLength];
			terrain_layer1 = new byte[TotalLength];
			terrain_layer2 = new byte[TotalLength];
			terrain_layer3 = new byte[TotalLength];
			terrain_layer4 = new byte[TotalLength];
			terrain_layer5 = new byte[TotalLength];
			terrain_layer6 = new byte[TotalLength];
			terrain_layer7 = new byte[TotalLength];
			terrain_layer8 = new byte[TotalLength];

		}

		[FoldoutGroup("DEBUG")] 
		[Button("Randomized Data")]
		public void RandomizedData()
		{
			for(int x = 0; x < cliffLevel.Length; x++)
            {
				cliffLevel[x] = (sbyte)Random.Range(0, sbyte.MaxValue);
			}

			for (int x = 0; x < heightVariation.Length; x++)
			{
				heightVariation[x] = (byte)Random.Range(0, byte.MaxValue);
			}

			//modifies terrain layers
			{
				for (int x = 0; x < terrain_layer1.Length; x++)
				{
					terrain_layer1[x] = (byte)Random.Range(0, byte.MaxValue);
				}

				for (int x = 0; x < terrain_layer2.Length; x++)
				{
					terrain_layer2[x] = (byte)Random.Range(0, byte.MaxValue);
				}

				for (int x = 0; x < terrain_layer3.Length; x++)
				{
					terrain_layer3[x] = (byte)Random.Range(0, byte.MaxValue);
				}

				for (int x = 0; x < terrain_layer4.Length; x++)
				{
					terrain_layer4[x] = (byte)Random.Range(0, byte.MaxValue);
				}

				for (int x = 0; x < terrain_layer5.Length; x++)
				{
					terrain_layer5[x] = (byte)Random.Range(0, byte.MaxValue);
				}

				for (int x = 0; x < terrain_layer6.Length; x++)
				{
					terrain_layer6[x] = (byte)Random.Range(0, byte.MaxValue);
				}

				for (int x = 0; x < terrain_layer7.Length; x++)
				{
					terrain_layer7[x] = (byte)Random.Range(0, byte.MaxValue);
				}

				for (int x = 0; x < terrain_layer8.Length; x++)
				{
					terrain_layer8[x] = (byte)Random.Range(0, byte.MaxValue);
				}
			}


		}

		public void SaveMapData()
        {

        }

	
	}
}