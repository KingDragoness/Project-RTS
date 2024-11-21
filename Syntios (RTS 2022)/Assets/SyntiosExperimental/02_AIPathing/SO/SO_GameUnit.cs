using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ProtoRTS
{

	//CUSTOM EDITOR UPDATE:
	//Later in custom editor, SO_GameUnit will only be used for vanilla unit.
	//The class of "Data_GameUnit" will be generated at Awake in AssetDatabase.

	[CreateAssetMenu(fileName = "Seaver", menuName = "Syntios/Game Unit (vanilla)", order = 1)]
	public class SO_GameUnit : ScriptableObject
	{

		public string ID = "Seaver";
		public string NameDisplay = "Seaver";
		public string Rank = "";
		public Sprite spriteWireframe;
		public int MaxHP = 40;
		public bool HasEnergy = false;

		public List<Unit.Tag> AllUnitTags = new List<Unit.Tag>();

	}
}