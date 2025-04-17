using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using static UnityEngine.UI.CanvasScaler;
using UnitOrder = ProtoRTS.Game.Orders.UnitOrder;


namespace ProtoRTS.Game
{

    public class GameUnit_OrderController : MonoBehaviour
	{

		public GameUnit unit;
		[SerializeReference] public List<UnitOrder> orders = new List<UnitOrder>();


        private void Awake()
        {
			Tick.OnTick += event_OnTick;
        }

        private void OnDestroy()
        {
            Tick.OnTick -= event_OnTick;
        }

        private void event_OnTick(int tick)
        {
			//runs the order
			if (orders.Count > 0)
			{
				var unitOrder = orders[0];

				if (unitOrder != null)
				{
                    unitOrder.Run(unit);
                    if (unitOrder.IsObjectiveAchieved(unit))
					{
						var order_to_delete = orders[0];
                        orders.RemoveAt(0);
                    }


                }
            }
        }

        //if there is no orders
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
		/// <summary>
		/// 
		/// </summary>
		/// <param name="unitOrder">Insert the implemented class order</param>
		public void AddCommandOrder(UnitOrder unitOrder)
		{
            orders.Add(unitOrder);

        }

        public void OverrideCommandOrder(UnitOrder unitOrder)
        {
			orders.Clear();
            orders.Add(unitOrder);

        }

		public UnitOrder GetCurrentOrder()
		{
			if (orders.Count == 0) return null;
			return orders[0];
		}

		public bool OrderStillExist(UnitOrder unitOrder)
		{
			return orders.Contains(unitOrder);
		}

        //SCRAP EVERYTHING BELOW

        private void Order_MOVE_Ground(GameUnit target, Vector3 positionTarget) 
		{

            if (target != null)
			{
				unit.move_TargetUnit = target;
            }
			else
			{
				unit.move_Target = positionTarget;
                unit.move_TargetUnit = null;
            }
        }

		private void Order_MOVE_Air(GameUnit target, Vector3 positionTarget)
		{

		}

		public void Order_MOVE(GameUnit target, Vector3 positionTarget)
		{
			if (unit.Class.IsFlyUnit)
			{
                Order_MOVE_Air(target, positionTarget);
            }
			else
			{
                Order_MOVE_Ground(target, positionTarget);
            }
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

		public void Order_SetRallyPoint()
		{

		}

        #endregion


    }
}