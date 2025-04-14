using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Video;
using ProtoRTS.Game;
using System.Linq;

namespace ProtoRTS
{


	[System.Serializable]
	public class CommandCard
	{
		public string cardName = "default";
		public List<UnitOrder> commands = new List<UnitOrder>();
	}

	[System.Serializable]
	public class UnitOrder
	{
		public enum Type
		{
			AbilityCommand,
			CancelPlacement,
			CancelSubmenu,
			CancelTarget,
			Submenu
		}

        public CommandButton button;
        [FoldoutGroup("")] public Type commandType;
        [FoldoutGroup("")][ReadOnly] public SO_GameUnit gameUnit;
        [FoldoutGroup("")][Range(0, 11)] public int position;
        [FoldoutGroup("")][ShowIf("commandType", Type.AbilityCommand)] [ValueDropdown("AllAbilities")] public string abilityScriptName = "default";
        [FoldoutGroup("")][ShowIf("commandType", Type.Submenu)][ValueDropdown("AllCommandCard")] public string cardToOpen = "";
        //public Requirement

        private IEnumerable AllAbilities()
        {
			return gameUnit.unitAbility.Select(x => new ValueDropdownItem(x.name, x.name));
        }

        private IEnumerable AllCommandCard()
        {
            return gameUnit.commandCards.Select(x => new ValueDropdownItem(x.cardName, x.cardName));
        }
    }



    //CUSTOM EDITOR UPDATE:
    //Later in custom editor, SO_GameUnit will only be used for vanilla unit.
    //The class of "Data_GameUnit" will be generated at Awake in AssetDatabase.

    [CreateAssetMenu(fileName = "Seaver", menuName = "Syntios/Game Unit (vanilla)", order = 1)]
	public class SO_GameUnit : ScriptableObject
	{

		public string ID = "Seaver";
		public string NameDisplay = "Seaver";
		public GameUnit basePrefab;
		public string Rank = "";
		public float Radius = 2;
		[Range(2,16)] public int LineOfSight = 7;
		public Sprite spriteWireframe;


        [FoldoutGroup("Resources")] public int MineralCost = 50;
        [FoldoutGroup("Resources")] public int EnergyCost = 25;
        [FoldoutGroup("Resources")][Range(0, 10)] public int SupplyCount = 1;
        [FoldoutGroup("Resources")] [Range(0, 500)] public int BuildTime = 24;

        #region Unit Properties
        [FoldoutGroup("Unit Properties")] public int MaxHP = 40;
        [FoldoutGroup("Unit Properties")] public bool HasEnergy = false;
        [FoldoutGroup("Unit Properties")] public bool IsUntouchable = false; //for scarab or missiles
        [FoldoutGroup("Unit Properties")] public List<Unit.Tag> AllUnitTags = new List<Unit.Tag>();
		#endregion

		#region AI behaviours
		[FoldoutGroup("AI Properties")] public bool AI_b_AttackOnSight = false;
        [FoldoutGroup("AI Properties")] public bool AI_b_AttackOnProvoked = false;
        [FoldoutGroup("AI Properties")] public bool AI_b_FleeOnProvoked = false;
        #endregion

        #region Portraits
        [FoldoutGroup("Portraits")]
		[SerializeField]
		[Tooltip("<= 0 for non-hero units. > 0 for hero units.")]
		internal int port_Importance = -9000;

		[FoldoutGroup("Portraits")] [SerializeField] internal VideoClip[] port_Idles;
		[FoldoutGroup("Portraits")] [SerializeField] internal VideoClip[] port_Talkings;
		[FoldoutGroup("Portraits")] [SerializeField] internal AudioClip[] voiceline_Ready;
		[FoldoutGroup("Portraits")] [SerializeField] internal AudioClip[] voiceline_Move;

        #endregion

        public List<CommandCard> commandCards = new List<CommandCard>();
        public List<UnitAbility> unitAbility = new List<UnitAbility>();

        private void OnValidate()
        {
            foreach(var card in commandCards)
			{
				foreach(var order in card.commands)
				{
					order.gameUnit = this;
				}
			}
        }
    }
}