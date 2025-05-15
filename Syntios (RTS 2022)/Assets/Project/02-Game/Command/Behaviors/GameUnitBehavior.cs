using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ProtoRTS.Game
{
	public class GameUnitBehavior : MonoBehaviour
	{

		public enum Class
		{
			Null,
            GUB_GenericAbility,
            GUB_Move = 10,
			GUB_Attack,
			GUB_AttackMove,
			GUB_Stop,
			GUB_HoldPosition,
			GUB_Patrol,
			GUB_MineResources,
			GUB_ReturnCargo,
			GUB_Repair,
			GUB_TrainUnit,
			GUB_TrainHangar,
			GUB_MorphUnit,
			GUB_LiftoffBuilding,
			GUB_NuclearPrepare,
			GUB_NuclearLaunch,
			GUB_Transport_EnterOne = 90,
			GUB_Transport_ExitOne,
			GUB_Transport_ExitAll,
			GUB_Build_Soviet = 100,  //terran
			GUB_Build_Dionarian, //WC3 humans
			GUB_Build_Mobius, //Drone sacrfice
			GUB_Build_Sixtus_Titan, //Probe protoss build
			GUB_ConstructingBuild, //Soviet

			//Special Attacks
			GUB_Attack_SovDestroyer = 500,
			GUB_Attack_Seaver,

			//Special Abilities
			GUB_Cloak = 1000,
			GUB_Hallucination
		}

		public float time = 0;

        public virtual void OnEnable()
        {
			Tick.OnTick += OnTick;
        }

        public virtual void OnDisable()
        {
            Tick.OnTick -= OnTick;
        }

        public void OnTick(int tick)
		{
            float deltaTime = 1f / (float)Tick.TicksPerSecond;

			time += deltaTime;

        }

		public void ResetTime()
		{
			time = 0f;

        }

		public virtual void Active()
		{

		}

		public virtual void CheckBackground()
		{

		}

	}
}