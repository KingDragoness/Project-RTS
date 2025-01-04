using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ProtoRTS
{

	public struct RESULT_Neighbor
	{
		public bool isNotOnEdge;
		public bool hasNeighbor;

		public bool IsValid()
		{
			return isNotOnEdge && hasNeighbor;
		}
	}

	[System.Serializable]
	public class SyntiosTerrainData
	{

		public enum DirectionNeighbor
		{
			North,
			South,
			West,
			East,
			NorthEast,
			NorthWest,
			SouthEast,
			SouthWest
		}

		public string ID = "Earth";
		//256 * 256 = 65.536
		[Range(32, 256)] public int size_x = 64;
		[Range(32, 256)] public int size_y = 64;

		// [000000] 00 (using first 6 bit to store cliff heights: 64 heights)
		//  000000 [00] (last 2 bit to store what type of cliffs: 4 cliff types)
		// wait there's bool array of manmadeCliffs

		[HideInEditorMode] [FoldoutGroup("Arrays")] public byte[] cliffLevel;

		[HideInEditorMode] [FoldoutGroup("Arrays")] public byte[] heightVariation; //0 to 255
		[HideInEditorMode] [FoldoutGroup("Arrays")] public byte[] fogOfWar;
		[HideInEditorMode] [FoldoutGroup("Arrays")] public bool[] manmadeCliffs;
		[HideInEditorMode] [FoldoutGroup("Arrays")] public byte[] terrain_layer1;
		[HideInEditorMode] [FoldoutGroup("Arrays")] public byte[] terrain_layer2;
		[HideInEditorMode] [FoldoutGroup("Arrays")] public byte[] terrain_layer3;
		[HideInEditorMode] [FoldoutGroup("Arrays")] public byte[] terrain_layer4;
		[HideInEditorMode] [FoldoutGroup("Arrays")] public byte[] terrain_layer5;
		[HideInEditorMode] [FoldoutGroup("Arrays")] public byte[] terrain_layer6;
		[HideInEditorMode] [FoldoutGroup("Arrays")] public byte[] terrain_layer7;
		[HideInEditorMode] [FoldoutGroup("Arrays")] public byte[] terrain_layer8;
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
			cliffLevel = new byte[TotalLength];
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
		[Button("ClearArrays Data")]
		public void ClearArrays()
		{
			cliffLevel = new byte[1];
			heightVariation = new byte[1];
			fogOfWar = new byte[1];
			manmadeCliffs = new bool[1];
			terrain_layer1 = new byte[1]; //splat r
			terrain_layer2 = new byte[1]; //splat g
			terrain_layer3 = new byte[1]; //splat b
			terrain_layer4 = new byte[1]; //splat a
			terrain_layer5 = new byte[1]; //splat2 r
			terrain_layer6 = new byte[1]; //splat2 g
			terrain_layer7 = new byte[1]; //splat2 b
			terrain_layer8 = new byte[1]; //splat2 a
		}

		[FoldoutGroup("DEBUG")]
		[Button("Randomized Data")]
		public void RandomizedData()
		{
			if (cliffLevel.Length <= 2)
			{
				InitializeData();
			}

			for (int x = 0; x < cliffLevel.Length; x++)
			{
				cliffLevel[x] = 0;
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
				c2.g = (byte)(DEBUG_TargetColor2.g);
				c2.b = (byte)(DEBUG_TargetColor2.b);
				c2.a = (byte)(DEBUG_TargetColor2.a);


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

			{
				for (int i = 0; i < TotalLength; i++)
				{
					int x = i % size_x;
					int y = i / size_y;

					if (i == 1000 | i == 1024 | i == 1025 | i == 1026)
					{
						cliffLevel[i] = 1;
					}
					if ((x == 15) && (y >= 11 | y <= 20))
					{
						cliffLevel[i] = 1;
					}

					if (x < 14 | x > 30) continue;
					if (y < 30 | y > 32) continue;

					cliffLevel[i] = 1;


				}
			}


		}

		public int cliffLevel_neighbour(DirectionNeighbor dir, Vector2Int origin, int magnitude = 1)
		{
			Vector2Int offsetPos = GetDirection(dir);

			offsetPos *= magnitude;

			if ((origin + offsetPos).x >= size_x |
			(origin + offsetPos).x < 0 |
			(origin + offsetPos).y >= size_y |
			(origin + offsetPos).y < 0)
			{
				return 0;
			}
			else
			{
				return cliffLevel[GetIndex(origin.x + offsetPos.x, origin.y + offsetPos.y)];
			}
		}

		public Vector2Int GetDirection(DirectionNeighbor dir)
		{
			Vector2Int offsetPos = new Vector2Int(0, 1);

			if (dir == DirectionNeighbor.North)
			{
				offsetPos = new Vector2Int(0, 1);
			}
			else if (dir == DirectionNeighbor.South)
			{
				offsetPos = new Vector2Int(0, -1);
			}
			else if (dir == DirectionNeighbor.West)
			{
				offsetPos = new Vector2Int(-1, 0);
			}
			else if (dir == DirectionNeighbor.East)
			{
				offsetPos = new Vector2Int(1, 0);
			}
			else if (dir == DirectionNeighbor.NorthEast)
			{
				offsetPos = new Vector2Int(1, 1);
			}
			else if (dir == DirectionNeighbor.NorthWest)
			{
				offsetPos = new Vector2Int(-1, 1);
			}
			else if (dir == DirectionNeighbor.SouthEast)
			{
				offsetPos = new Vector2Int(1, -1);
			}
			else if (dir == DirectionNeighbor.SouthWest)
			{
				offsetPos = new Vector2Int(-1, -1);
			}

			return offsetPos;
		}

		private RESULT_Neighbor resultReport_neighbor;

		public RESULT_Neighbor IsNeighborValid(DirectionNeighbor dir, Vector2Int myPos)
		{
			resultReport_neighbor = new RESULT_Neighbor();
			resultReport_neighbor.hasNeighbor = false;
			resultReport_neighbor.isNotOnEdge = false;
			int level = cliffLevel_neighbour(dir, myPos); //GetIndex(origin.x + offsetPos.x, origin.y + offsetPos.y);
			var v2_dir = GetDirection(dir);
			int index = GetIndex(myPos.x, myPos.y);
			int index_extra = 0;

			Vector2Int secondNeighbor = myPos + v2_dir;

			if (secondNeighbor.x <= size_x && secondNeighbor.y <= size_y &&
				secondNeighbor.x >= 0 && secondNeighbor.y >= 0)
				index_extra = GetIndex(secondNeighbor.x, secondNeighbor.y);


			if (index_extra != 0 && cliffLevel[index_extra] == level)
			{
				Debug.Log($"NO: {myPos} | second neigh: {secondNeighbor}");
				resultReport_neighbor.isNotOnEdge = false;
				resultReport_neighbor.hasNeighbor = true;
			}
			else if (cliffLevel[index] == 0)
			{

			}
			//problem persists: checks cliff level on shared cell. 
			//the coord must sample 4 tiles.
			else if (cliffLevel[index] == level)
			{
				resultReport_neighbor.isNotOnEdge = true;
				resultReport_neighbor.hasNeighbor = true;
			}

			return resultReport_neighbor;
		}

		public Vector2Int[] GetValidNeighbors(Vector2Int origin)
		{
			int length = 0;
			Vector2Int offsetPos1 = new Vector2Int(0, 0);
			Vector2Int offsetPos2 = new Vector2Int(1, 0);
			Vector2Int offsetPos3 = new Vector2Int(0, 1);
			Vector2Int offsetPos4 = new Vector2Int(1, 1);

			List<Vector2Int> myPosArray = new List<Vector2Int>();

			if ((origin + offsetPos1).x >= size_x |
				(origin + offsetPos1).x < 0 |
				(origin + offsetPos1).y >= size_y |
				(origin + offsetPos1).y < 0)
			{

			}
			else
			{
				myPosArray.Add(origin + offsetPos1);
			}

			if ((origin + offsetPos2).x >= size_x |
			  (origin + offsetPos2).x < 0 |
			  (origin + offsetPos2).y >= size_y |
			  (origin + offsetPos2).y < 0)
			{

			}
			else
			{
				myPosArray.Add(origin + offsetPos2);
			}

			if ((origin + offsetPos3).x >= size_x |
			  (origin + offsetPos3).x < 0 |
			  (origin + offsetPos3).y >= size_y |
			  (origin + offsetPos3).y < 0)
			{

			}
			else
			{
				myPosArray.Add(origin + offsetPos3);
			}

			if ((origin + offsetPos4).x >= size_x |
			  (origin + offsetPos4).x < 0 |
			  (origin + offsetPos4).y >= size_y |
			  (origin + offsetPos4).y < 0)
			{

			}
			else
			{
				myPosArray.Add(origin + offsetPos4);
			}

			return myPosArray.ToArray();
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