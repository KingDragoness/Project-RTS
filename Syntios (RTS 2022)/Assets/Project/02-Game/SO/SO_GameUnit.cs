using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Video;

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

		[FoldoutGroup("Portraits")]
		[SerializeField]
		[Tooltip("<= 0 for non-hero units. > 0 for hero units.")]
		internal int port_Importance = -9000;

		[FoldoutGroup("Portraits")] [SerializeField] internal VideoClip[] port_Idles;
		[FoldoutGroup("Portraits")] [SerializeField] internal VideoClip[] port_Talkings;
		[FoldoutGroup("Portraits")] [SerializeField] internal AudioClip[] voiceline_Ready;
		[FoldoutGroup("Portraits")] [SerializeField] internal AudioClip[] voiceline_Move;

	}
}