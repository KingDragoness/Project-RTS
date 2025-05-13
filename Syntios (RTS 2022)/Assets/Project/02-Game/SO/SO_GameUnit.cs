using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Video;
using ProtoRTS.Game;
using UnityEngine.UIElements;

namespace ProtoRTS
{


	[System.Serializable]
	public class CommandCard
	{
		public string cardName = "default";
		public List<UnitButtonCommand> commands = new List<UnitButtonCommand>();


        public bool CheckHasCommandAtIndex(int index)
        {
            foreach(var comm in commands) if (comm.position == index) return true;

            return false;
        }

        public UnitButtonCommand CommandButton(int index)
        {
            var button = commands.Find(x => x.position == index);

            return button;
        }

    }

	[System.Serializable]
	public class UnitButtonCommand
	{

        public enum AbilityOrder
        {
            None,
            Move,
            Stop,
            Attack,
            HoldPosition,
            Patrol,
            EnterExitUnit,
            BuildBuilding,
            TrainUnit,
            GatherResources,
            HealRepair,
            ExecuteAbility

        }


        public enum Type
		{
			AbilityCommand,
			CancelPlacement,
			CancelSubmenu,
			CancelTarget,
			Submenu
		}

        public CommandButtonSO button;
        [FoldoutGroup("$Combined_1")] public Type commandType;
        [FoldoutGroup("$Combined_1")][ReadOnly] public SO_GameUnit gameUnit;
        [FoldoutGroup("$Combined_1")][Range(0, 11)] public int position;
        [FoldoutGroup("$Combined_1")][ShowIf("commandType", Type.AbilityCommand)] public AbilityOrder abilityType;
        [FoldoutGroup("$Combined_1")][ShowIf("abilityType", AbilityOrder.BuildBuilding)] public SO_GameUnit buildingSO;
        [FoldoutGroup("$Combined_1")][ShowIf("abilityType", AbilityOrder.TrainUnit)] public SO_GameUnit unitSO;
        [FoldoutGroup("$Combined_1")][ShowIf("commandType", Type.Submenu)][ValueDropdown("AllCommandCard")] public string cardToOpen = "";
        [FoldoutGroup("$Combined_1")][ValueDropdown("All_UnitOrderEnum")] public System.Type classUnitOrder;


        public string Combined_1 
        { 
            get 
            {
                if (commandType == Type.Submenu)
                {
                    return this.commandType + $" {cardToOpen}";
                }    
                if (commandType == Type.AbilityCommand)
                {
                    if (abilityType == AbilityOrder.BuildBuilding) return $"Build {buildingSO}";
                    if (abilityType == AbilityOrder.TrainUnit) return $"Train {unitSO}";
                    return this.abilityType.ToString();
                }

                return this.commandType + ""; 
            } 
        }


        private IEnumerable AllCommandCard()
        {
            if (gameUnit == null) return null;
            return gameUnit.commandCards.Select(x => new ValueDropdownItem(x.cardName, x.cardName));
        }

        IEnumerable<Orders.UnitOrder> GetAllOrderClass()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsSubclassOf(typeof(Orders.UnitOrder)))
                .Select(type => Activator.CreateInstance(type) as Orders.UnitOrder);
        }

        private IEnumerable All_UnitOrderEnum()
        {
            var ienumrables = GetAllOrderClass().Select(x => new ValueDropdownItem(x.GetType().Name, x.GetType()));
            ienumrables = ienumrables.Append(new ValueDropdownItem("", null));
            return ienumrables;
        }

        [FoldoutGroup("Test")]
        [Button("Test_PrintClass")]
        public void Test_PrintClass()
        {

            Debug.Log(System.Type.GetType($"{classUnitOrder}"));
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
		public Sprite spriteWireframe;



        [FoldoutGroup("Resources")] public int MineralCost = 50;
        [FoldoutGroup("Resources")] public int EnergyCost = 25;
        [FoldoutGroup("Resources")][Range(0, 10)] public int SupplyCount = 1;
        [FoldoutGroup("Resources")] [Range(0, 500)] public int BuildTime = 24;

        #region Unit Properties
        [FoldoutGroup("Unit Properties")] public int MaxHP = 40;
        [FoldoutGroup("Unit Properties")] public bool IsFlyUnit = false;
        [FoldoutGroup("Unit Properties")] public bool HasShield = false;
        [FoldoutGroup("Unit Properties")] public bool HasEnergy = false;
        [FoldoutGroup("Unit Properties")][Range(2, 16)] public int LineOfSight = 7;
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
        //[ValueDropdown("DefaultCardCommands")] public string defaultRightClickAbility = "";
        [InfoBox("Command card at [0] index is always default")] public List<CommandCard> commandCards = new List<CommandCard>();


        private IEnumerable DefaultCardCommands()
        {
            if (commandCards.Count == 0) return null;
            return commandCards[0].commands.Select(x => new ValueDropdownItem(x.abilityType.ToString(), x.abilityType.ToString()));
        }

        public CommandCard DefaultCard
        {
            get { return commandCards[0]; }
        }

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