using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using static UnityEngine.UI.CanvasScaler;


namespace ProtoRTS.Game
{

    public class GameUnit_OrderController : MonoBehaviour
	{

		//public GameUnit unit;


  //      private void Awake()
  //      {
		//	Tick.OnTick += event_OnTick;
  //      }

  //      private void OnDestroy()
  //      {
  //          Tick.OnTick -= event_OnTick;
  //      }

  //      private void event_OnTick(int tick)
  //      {
		//	//runs the order
		//	if (orders.Count > 0)
		//	{
		//		var unitOrder = orders[0];

		//		if (unitOrder != null)
		//		{
  //                  unitOrder.Run(unit);
  //                  if (unitOrder.IsObjectiveAchieved(unit))
		//			{
		//				var order_to_delete = orders[0];
  //                      orders.RemoveAt(0);
  //                  }


  //              }
  //          }
  //      }

  //      //if there is no orders
  //      public void DefaultStateBehaviour() 
		//{ 
		//	if (unit.Class.AI_b_AttackOnSight)
		//	{

		//	}
  //          if (unit.Class.AI_b_AttackOnProvoked)
  //          {

  //          }
  //          if (unit.Class.AI_b_FleeOnProvoked)
  //          {

  //          }
  //      }


  //      #region Orders
		///// <summary>
		///// 
		///// </summary>
		///// <param name="unitOrder">Insert the implemented class order</param>
		//private void AddCommandOrder(UnitOrder unitOrder)
		//{
  //          orders.Add(unitOrder);

  //      }

  //      private void OverrideCommandOrder(UnitOrder unitOrder)
  //      {
		//	orders.Clear();
  //          orders.Add(unitOrder);

  //      }

		//public UnitOrder GetCurrentOrder()
		//{
		//	if (orders.Count == 0) return null;
		//	return orders[0];
		//}

		//public bool OrderStillExist(UnitOrder unitOrder)
		//{
		//	return orders.Contains(unitOrder);
		//}

  //      public virtual void AddOrder_Move(Vector3 targetPos, GameUnit targetUnit, bool overrideOrder = true)
  //      {
  //          bool valid = false;
  //          foreach (var cc in unit.Class.commandCards)
  //          {
  //              foreach (var button in cc.commands)
  //              {
  //                  if (button.abilityType == UnitButtonCommand.AbilityOrder.Move && button.commandType == UnitButtonCommand.Type.AbilityCommand)
  //                  {
  //                      valid = true;
  //                      break;
  //                  }

  //              }
  //          }
  //          if (!valid) return;
            
  //          var order_unit = new Orders.Order_Move(targetUnit, targetPos);

  //          if (overrideOrder) OverrideCommandOrder(order_unit);
  //          else AddCommandOrder(order_unit);
  //      }

  //      public virtual void GiveOrder(Orders.UnitOrder unitOrder, bool overrideOrder = true)
  //      {
  //          if (overrideOrder) OverrideCommandOrder(unitOrder);
  //          else AddCommandOrder(unitOrder);
  //      }




    }
}