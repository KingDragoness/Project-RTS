using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ProtoRTS.Game
{

	[System.Serializable]
	public class UnitAbility
	{


		public enum ActionType
        {
			PlayAnimation,
			MoveByNavmesh,
			MovePatrolNavmesh,
			MoveByAir,
			MovePatrolAir,
			MoveFree,
			MovePatrolFree,
			SetFlag,
			ModifyHP,
			ModifyMP,
			ModifyShield,
			AddStatusEffect,
			ChangeUnitState,
			CreateUnit,
			RemoveSelf,
			Enter_Unload_Unit,
			ConvertFaction,
			ExecuteGameScript,
			PlaceBuilding,
			QueueUnit,
			Attack,
			Construction,
			CancelConstruction

        }

        [System.Serializable]

        public class UnitCondition
        {

            public enum Type
            {
                Always,
                DistanceToTarget,
                Timer,
                EnergyCost,
                TargetType

            }

			public Type type;

        }
		
		public enum OrderCategory
		{
			None,
			DEFAULT,
			MOVE = 5,
			ATTACK,
			PATROL,
			ABILITY,
			CARGO,

		}

		public enum TargetType
		{
			Position,
			SingleUnit,
			BuildingPlacement,
			Self
		}

        [System.Serializable]
        public class Action
		{
            private bool TargetablePosition => 
				type == ActionType.MoveByAir || 
				type == ActionType.MoveByNavmesh || 
				type == ActionType.MoveFree ||
                type == ActionType.MovePatrolAir ||
                type == ActionType.MovePatrolNavmesh ||
                type == ActionType.MovePatrolFree;

            public ActionType type;
			[ShowIf(nameof(TargetablePosition))] public TargetType targetType;
			[ShowIf("type", ActionType.PlaceBuilding)] public SO_GameUnit buildingSO;

        }


        //[Title("$name")]
        [FoldoutGroup("$name")] public string name = "default";
        [FoldoutGroup("$name")] public List<Action> allActions = new List<Action>();
        [FoldoutGroup("$name")] public OrderCategory orderCategory;

	}


}