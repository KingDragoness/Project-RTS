using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ProtoRTS
{
	/*
	 * CLIFFS indexes:
	 * 0 = x0 y0
	 * 1 = x1 y0
	 * 2 = x2 y0
	 * ...
	 * 15 = x3 y3
	 */


	/// <summary>
	/// 
	/// </summary>
	[CreateAssetMenu(fileName = "Earth", menuName = "Syntios/Terrain Preset", order = 1)]

	public class SO_TerrainPreset : ScriptableObject
	{

		public enum Tileset
        {
			Null,
			CornerNorthWest, //against left up
			CornerNorthEast,
			CornerSouthWest,
			CornerSouthEast,
			North, //horizontal face up
			South,
			West,
			East,
			DiagonalEast, // shaped like this-> /
			DiagonalWest,  // shaped like this-> \
			SharpCornerNorthWest, //L shape
			SharpCornerNorthEast,
			SharpCornerSouthWest,
			SharpCornerSouthEast,
			Flat
		}

		public string PresetID = "Earth";
		
		
		[FoldoutGroup("Layer")] public Texture2D ground;

		[FoldoutGroup("Layer")] public string layer1_name = "Layer 1";
		[FoldoutGroup("Layer")] public Texture2D layer1;

		[FoldoutGroup("Layer")] public string layer2_name = "Layer 2";
		[FoldoutGroup("Layer")] public Texture2D layer2;

		[FoldoutGroup("Layer")] public string layer3_name = "Layer 3";
		[FoldoutGroup("Layer")] public Texture2D layer3;

		[FoldoutGroup("Layer")] public string layer4_name = "Layer 4";
		[FoldoutGroup("Layer")] public Texture2D layer4;

		[FoldoutGroup("Layer")] public string layer5_name = "Layer 5";
		[FoldoutGroup("Layer")] public Texture2D layer5;

		[FoldoutGroup("Layer")] public string layer6_name = "Layer 6";
		[FoldoutGroup("Layer")] public Texture2D layer6;

		[FoldoutGroup("Layer")] public string layer7_name = "Layer 7";
		[FoldoutGroup("Layer")] public Texture2D layer7;

		[FoldoutGroup("Layer")] public string layer8_name = "Layer 8";
		[FoldoutGroup("Layer")] public Texture2D layer8;

		[FoldoutGroup("Cliff Models")] public List<GameObject> manmadeCliffs;
		[FoldoutGroup("Cliff Models")] public List<GameObject> naturalCliffs;


		public string GetLayerName(int id)
        {
			if (id == -1)
            {
				return "Ground";
            }
			else if (id == 0)
			{
				return layer1_name;
			}
			else if (id == 1)
			{
				return layer2_name;
			}
			else if (id == 2)
			{
				return layer3_name;
			}
			else if (id == 3)
			{
				return layer4_name;
			}
			else if (id == 4)
			{
				return layer5_name;
			}
			else if (id == 5)
			{
				return layer6_name;
			}
			else if (id == 6)
			{
				return layer7_name;
			}
			else if (id == 7)
			{
				return layer8_name;
			}
	

			return "";
        }

		public Texture2D GetLayer(int id)
		{
			if (id == -1)
			{
				return ground;
			}
			else if (id == 0)
			{
				return layer1;
			}
			else if (id == 1)
			{
				return layer2;
			}
			else if (id == 2)
			{
				return layer3;
			}
			else if (id == 3)
			{
				return layer4;
			}
			else if (id == 4)
			{
				return layer5;
			}
			else if (id == 5)
			{
				return layer6;
			}
			else if (id == 6)
			{
				return layer7;
			}
			else if (id == 7)
			{
				return layer8;
			}


			return ground;
		}

		public GameObject GetManmadeCliff(Tileset dir)
        {
			//based origin
			if (dir == Tileset.CornerNorthWest)
            {
				return manmadeCliffs[0];
            }
			else if (dir == Tileset.CornerSouthEast)
			{
				return manmadeCliffs[2];
			}
			else if (dir == Tileset.CornerNorthEast)
			{
				return manmadeCliffs[11];
			}
			else if (dir == Tileset.CornerSouthWest)
			{
				return manmadeCliffs[3];
			}
			else if (dir == Tileset.North)
            {
				return manmadeCliffs[14];
            }
			else if (dir == Tileset.South)
			{
				return manmadeCliffs[4];
			}
			else if (dir == Tileset.East)
			{
				return manmadeCliffs[6];
			}
			else if (dir == Tileset.West)
			{
				return manmadeCliffs[12];
			}
			else if (dir == Tileset.DiagonalEast)
			{
				return manmadeCliffs[1];
			}
			else if (dir == Tileset.DiagonalWest)
			{
				return manmadeCliffs[7];
			}
			else if (dir == Tileset.SharpCornerNorthWest)
			{
				return manmadeCliffs[8];
			}
			else if (dir == Tileset.SharpCornerNorthEast)
			{
				return manmadeCliffs[13];
			}
			else if (dir == Tileset.SharpCornerSouthWest)
			{
				return manmadeCliffs[5];
			}
			else if (dir == Tileset.SharpCornerSouthEast)
			{
				return manmadeCliffs[10];
			}
			else if (dir == Tileset.Flat)
			{
				return manmadeCliffs[9];
			}

			//corner
			return null;
		}
	}
}