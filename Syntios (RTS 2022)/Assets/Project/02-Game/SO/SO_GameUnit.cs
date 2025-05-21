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


    //CUSTOM EDITOR UPDATE:
    //Later in custom editor, SO_GameUnit will only be used for vanilla unit.
    //The class of "Data_GameUnit" will be generated at Awake in AssetDatabase.

    [CreateAssetMenu(fileName = "Seaver", menuName = "Syntios/Game Unit (vanilla)", order = 1)]
    public class SO_GameUnit : ScriptableObject
    {

        public string ID = "Seaver";
        public string NameDisplay = "Seaver";
        public GameUnit basePrefab;
        public Ghost3dModel ghost3dModelPrefab;
        public string Rank = "";
        public float Radius = 2;
        public Sprite spriteWireframe;
        public List<SO_Weapon> allWeapons = new List<SO_Weapon>();



        [FoldoutGroup("Resources")] public int MineralCost = 50;
        [FoldoutGroup("Resources")] public int EnergyCost = 25;
        [FoldoutGroup("Resources")][Range(0, 10)] public int SupplyCount = 1;
        [FoldoutGroup("Resources")][ShowIf("IsSupplyProviderUnit")] public int SupplyProvide = 8;
        [FoldoutGroup("Resources")][Range(0, 500)] public int BuildTime = 24;

        #region Unit Properties
        [FoldoutGroup("Unit Properties")] public int MaxHP = 40;
        [FoldoutGroup("Unit Properties")] public int MaxShield = 40;
        [FoldoutGroup("Unit Properties")] public bool IsFlyUnit = false;
        [FoldoutGroup("Unit Properties")] public bool HasShield = false;
        [FoldoutGroup("Unit Properties")] public bool HasEnergy = false;
        [FoldoutGroup("Unit Properties")][Range(2, 16)] public int LineOfSight = 7;
        [FoldoutGroup("Unit Properties")] public bool IsUntouchable = false; //for scarab or missiles
        [FoldoutGroup("Unit Properties")] public List<Unit.Tag> AllUnitTags = new List<Unit.Tag>();
        #endregion

        #region AI behaviours
        //[FoldoutGroup("AI Properties")] public List<GameUnitBehavior.Class> allBehaviorClasses = new List<GameUnitBehavior.Class>();
        [FoldoutGroup("AI Properties")] public bool AI_b_AttackOnSight = false;
        [FoldoutGroup("AI Properties")] public bool AI_b_AttackOnProvoked = false;
        [FoldoutGroup("AI Properties")] public bool AI_b_FleeOnProvoked = false;
        #endregion

        #region Portraits
        [FoldoutGroup("Portraits")]
        [SerializeField]
        [Tooltip("<= 0 for non-hero units. > 0 for hero units.")]
        internal int port_Importance = -9000;

        [FoldoutGroup("Portraits")][SerializeField] internal VideoClip[] port_Idles;
        [FoldoutGroup("Portraits")][SerializeField] internal VideoClip[] port_Talkings;
        [FoldoutGroup("Portraits")][SerializeField] internal AudioClip[] voiceline_Ready;
        [FoldoutGroup("Portraits")][SerializeField] internal AudioClip[] voiceline_Move;

        #endregion
        //[ValueDropdown("DefaultCardCommands")] public string defaultRightClickAbility = "";
        [InfoBox("Command card at [0] index is always default")] public List<CommandCard> commandCards = new List<CommandCard>();


        //private IEnumerable DefaultCardCommands()
        //{
        //    if (commandCards.Count == 0) return null;
        //    return commandCards[0].commands.Select(x => new ValueDropdownItem(x.abilityType.ToString(), x.abilityType.ToString()));
        //}

        public CommandCard DefaultCard
        {
            get { return commandCards[0]; }
        }

        private void OnValidate()
        {
            foreach (var card in commandCards)
            {
                foreach (var order in card.commands)
                {
                    order.gameUnit = this;
                }
            }
        }

        public bool IsSupplyProviderUnit()
        {
            return AllUnitTags.Contains(Unit.Tag.SupplyProvider);
        }

        public int MaxMana()
        {
            return 200;
        }

        /// <summary>
        /// First it checks for similar button class,
        /// then checks for tags
        /// If tags failed, then it will return null
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        internal string GetSimilarButton(string ID)
        {
            foreach (var cc in commandCards)
            {
                foreach(var button in cc.commands)
                {
                    if (button.buttonID.ToLower() == ID.ToLower())
                        return button.buttonID;
                }
            }

            return "";
        }

        [FoldoutGroup("HELPERS")]
        [Button("Give IDs")]
        public void GiveIDToButtons()
        {
            foreach (var cc in commandCards)
            {
                foreach (var button in cc.commands)
                {
                    bool dupe = false;

                    foreach (var buttonToCompare in cc.commands)
                    {
                        if (buttonToCompare == button) continue;
                        if (buttonToCompare.orderClass == button.orderClass)
                        {
                            dupe = true;
                            break;
                        }
                    }

                    if (!dupe) button.buttonID = button.orderClass.ToString();
                }

                
            }
        }
    }


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



        public enum Type
		{
			AbilityCommand,
			CancelPlacement,
			CancelSubmenu,
			CancelTarget,
			Submenu
		}

        public CommandButtonSO button;

        //THIS IS THE BEST METHOD TO PAIR BUTTON, DON'T TRY TO CHANGE IT WITH OBJ REFERENCE OR DO ANY RETARDED PRETENTIOUS METHOD IN IT
        //Follow naming convention in OrderClass (enum) UNLESS it is generic ability 
        //Generic ability just write whatever ID you want (CHANCE OF COLLISION is LOW, FOR GOD SAKES)
        //IN THE CASE DUPLICATE in different CCs... IT IS MEANT TO BE BECAUSE STOP in (State 1) IS LITERALLY THE SAME in (State 2).
        //KEEP IT SIMPLE 4 FOR GOD SAKE, KEEP IT SIMPLE!!!
        public string buttonID = "_"; 




        [FoldoutGroup("$Combined_1")] public Type commandType;
        [FoldoutGroup("$Combined_1")][ReadOnly] public SO_GameUnit gameUnit;
        [FoldoutGroup("$Combined_1")][Range(0, 11)] public int position;
        [FoldoutGroup("$Combined_1")][ShowIf("commandType", Type.Submenu)][ValueDropdown("AllCommandCard")] public string cardToOpen = "";
        [FoldoutGroup("$Combined_1")][ShowIf("IsBuildingLikeOrder")] public SO_GameUnit buildingSO;
        [FoldoutGroup("$Combined_1")][ShowIf("orderClass", OrderClass.order_train_unit)] public SO_GameUnit unitSO;
        [FoldoutGroup("$Combined_1")][ShowIf("commandType", Type.AbilityCommand)] public OrderClass orderClass;


        public string Combined_1 
        { 
            get 
            {
                if (commandType == Type.Submenu)
                {
                    return this.commandType + $" {cardToOpen}";
                }    


                return this.commandType + ""; 
            } 
        }

        public bool IsBuildingLikeOrder()
        {
            return orderClass == OrderClass.order_build_Dionarian | orderClass == OrderClass.order_build_Mobius | orderClass == OrderClass.order_build_Soviet | orderClass == OrderClass.order_build_TitanSixtus;
        }


        private IEnumerable AllCommandCard()
        {
            if (gameUnit == null) return null;
            return gameUnit.commandCards.Select(x => new ValueDropdownItem(x.cardName, x.cardName));
        }

   
     

    }



}