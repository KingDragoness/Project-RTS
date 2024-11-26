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
		public Texture2D layer1;
		public Texture2D layer2;
		public Texture2D layer3;
		public Texture2D layer4;
		public Texture2D layer5;
		public Texture2D layer6;
		public Texture2D layer7;
		public Texture2D layer8;

	}
}