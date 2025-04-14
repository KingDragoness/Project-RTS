using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ProtoRTS.Game
{
	public class GameUnit_OrderController : MonoBehaviour
	{

		public GameUnit unit;

		public void DefaultStateBehaviour() 
		{ 
			if (unit.Class.AI_b_AttackOnSight)
			{

			}
            if (unit.Class.AI_b_AttackOnProvoked)
            {

            }
            if (unit.Class.AI_b_FleeOnProvoked)
            {

            }
        }

        #region Orders

        private void Order_MOVE_Ground() 
		{
			
		}

		private void Order_MOVE_Air()
		{

		}

		public void Order_MOVE()
		{

		}

		public void Order_ATTACK()
		{

		}

		public void Order_STOP()
		{

		}

		public void Order_PATROL()
		{ 
			
		}

		public void Order_HOLDPOS()
		{

		}

		public void Order_ExecuteAbility()
		{

		}

		public void Order_EnterExitUnit()
		{

		}

		public void Order_GatherResources()
		{

		}

		public void Order_Repair()
		{

		}

		public void Order_Constructing()
		{

		}

		public void Order_PlaceBuilding()
		{

		}

        #endregion


    }
}