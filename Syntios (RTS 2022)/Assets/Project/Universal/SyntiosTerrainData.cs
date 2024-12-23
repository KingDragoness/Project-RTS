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
		[FoldoutGroup("Arrays")] public sbyte[] cliffLevel; //-128 to 128 (only -8 to 8 used)
		[FoldoutGroup("Arrays")] public byte[] heightVariation; //0 to 255
		[FoldoutGroup("Arrays")] public byte[] fogOfWar;
		[FoldoutGroup("Arrays")] public bool[] manmadeCliffs;
		[FoldoutGroup("Arrays")] public byte[] terrain_layer1;
		[FoldoutGroup("Arrays")] public byte[] terrain_layer2;
		[FoldoutGroup("Arrays")] public byte[] terrain_layer3;
		[FoldoutGroup("Arrays")] public byte[] terrain_layer4;
		[FoldoutGroup("Arrays")] public byte[] terrain_layer5;
		[FoldoutGroup("Arrays")] public byte[] terrain_layer6;
		[FoldoutGroup("Arrays")] public byte[] terrain_layer7;
		[FoldoutGroup("Arrays")] public byte[] terrain_layer8;
		[FoldoutGroup("DEBUG")] public Color32 DEBUG_TargetColor1;
		[FoldoutGroup("DEBUG")] public Color32 DEBUG_TargetColor2;

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


		//4 times size
		//(every splatmap is 1k texture / 1 million pixels)
		public int SplatmapLength
        {
            get
            {
				return (256 * 4) * (256 * 4);

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

		public int GetSplatmapIndex(int x, int y)
		{
			if (x > size_x * 4) return 0;
			if (y > size_y * 4) return 0;
			if (x < 0) return 0;
			if (y < 0) return 0;

			return x + (1024 * y);
		}

		[FoldoutGroup("DEBUG")]
		[Button("Initialize Data")]
		public void InitializeData()
        {
			cliffLevel = new sbyte[TotalLength];
			heightVariation = new byte[TotalLength];
			fogOfWar = new byte[TotalLength * 4];
			manmadeCliffs = new bool[TotalLength];
			terrain_layer1 = new byte[SplatmapLength]; //splat r
			terrain_layer2 = new byte[SplatmapLength]; //splat g
			terrain_layer3 = new byte[SplatmapLength]; //splat b
			terrain_layer4 = new byte[SplatmapLength]; //splat a
			terrain_layer5 = new byte[SplatmapLength]; //splat2 r
			terrain_layer6 = new byte[SplatmapLength]; //splat2 g
			terrain_layer7 = new byte[SplatmapLength]; //splat2 b
			terrain_layer8 = new byte[SplatmapLength]; //splat2 a

		}

		[FoldoutGroup("DEBUG")] 
		[Button("Randomized Data")]
		public void RandomizedData()
		{
			if (cliffLevel.Length <= 2)
            {
				InitializeData();
			}

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
				int perLayerLength = terrain_layer1.Length / 8;

				//for (int x = 0; x < perLayerLength; x++)
				//{
				//	terrain_layer1[x] = 255;		
				//}

				//for (int x = perLayerLength; x < perLayerLength * 2; x++)
				//{
				//	terrain_layer2[x] = 255;
				//}

				//for (int x = perLayerLength * 2; x < perLayerLength * 3; x++)
				//{
				//	terrain_layer3[x] = 255;
				//}

				//for (int x = perLayerLength * 3; x < perLayerLength * 4; x++)
				//{
				//	terrain_layer4[x] = 255;
				//}

				//for (int x = perLayerLength * 4; x < perLayerLength * 5; x++)
				//{
				//	terrain_layer5[x] = 255;
				//}

				//for (int x = perLayerLength * 5; x < perLayerLength * 6; x++)
				//{
				//	terrain_layer6[x] = 255;
				//}

				//for (int x = perLayerLength * 6; x < perLayerLength * 7; x++)
				//{
				//	terrain_layer7[x] = 255;
				//}

				//for (int x = perLayerLength * 7; x < terrain_layer8.Length; x++)
				//{
				//	terrain_layer8[x] = 255;
				//}
			}

			//test all colors paint
			{
				Color32 c1 = new Color32();
				Color32 c2 = new Color32();
				c1.r = (byte)(DEBUG_TargetColor1.r);
				c1.g = (byte)(DEBUG_TargetColor1.g);
				c1.b = (byte)(DEBUG_TargetColor1.b);
				c1.a = (byte)(DEBUG_TargetColor1.a);
				c2.r = (byte)(DEBUG_TargetColor2.r);
				c2.g = (byte)(DEBUG_TargetColor2.g );
				c2.b = (byte)(DEBUG_TargetColor2.b );
				c2.a = (byte)(DEBUG_TargetColor2.a );


				for (int x = 0; x < terrain_layer8.Length; x++)
                {			
					terrain_layer1[x] = c1.r;
					terrain_layer2[x] = c1.g;
					terrain_layer3[x] = c1.b;
					terrain_layer4[x] = c1.a;
					terrain_layer5[x] = c2.r;
					terrain_layer6[x] = c2.g;
					terrain_layer7[x] = c2.b;
					terrain_layer8[x] = c2.a;

				}

				Debug.Log($"{c1} / {c2}");

			}


		}

		public void SetTerrainLayer(int layer, int index, float targetStrength)
        {
			if (layer == 0)
			{
				terrain_layer1[index] = (byte)targetStrength;
			}
			if (layer == 1)
			{
				terrain_layer2[index] = (byte)targetStrength;
			}
			if (layer == 2)
			{
				terrain_layer3[index] = (byte)targetStrength;
			}
			if (layer == 3)
			{
				terrain_layer4[index] = (byte)targetStrength;
			}
			if (layer == 4)
			{
				terrain_layer5[index] = (byte)targetStrength;
			}
			if (layer == 5)
			{
				terrain_layer6[index] = (byte)targetStrength;
			}
			if (layer == 6)
			{
				terrain_layer7[index] = (byte)targetStrength;
			}
			if (layer == 7)
			{
				terrain_layer8[index] = (byte)targetStrength;
			}
		}


		public byte GetTerrainLayer_str(int layer, int index)
        {
			if (layer == 0)
			{
				return terrain_layer1[index];
			}
			if (layer == 1)
			{
				return terrain_layer2[index];
			}
			if (layer == 2)
			{
				return terrain_layer3[index];
			}
			if (layer == 3)
			{
				return terrain_layer4[index];
			}
			if (layer == 4)
			{
				return terrain_layer5[index];
			}
			if (layer == 5)
			{
				return terrain_layer6[index];
			}
			if (layer == 6)
			{
				return terrain_layer7[index];
			}
			if (layer == 7)
			{
				return terrain_layer8[index];
			}

			return 0;
		}


		public void SaveMapData()
        {

        }

	
	}
}