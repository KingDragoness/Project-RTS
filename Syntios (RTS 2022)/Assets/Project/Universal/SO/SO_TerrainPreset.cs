using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ProtoRTS
{

	[CreateAssetMenu(fileName = "Earth", menuName = "Syntios/Terrain Preset", order = 1)]

	public class SO_TerrainPreset : ScriptableObject
	{

		public string PresetID = "Earth";
		public Texture2D ground;

		public string layer1_name = "Layer 1";
		public Texture2D layer1;

		public string layer2_name = "Layer 2";
		public Texture2D layer2;

		public string layer3_name = "Layer 3";
		public Texture2D layer3;

		public string layer4_name = "Layer 4";
		public Texture2D layer4;

		public string layer5_name = "Layer 5";
		public Texture2D layer5;

		public string layer6_name = "Layer 6";
		public Texture2D layer6;

		public string layer7_name = "Layer 7";
		public Texture2D layer7;

		public string layer8_name = "Layer 8";
		public Texture2D layer8;


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
	}
}