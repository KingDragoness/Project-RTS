using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using static UnityEngine.UI.CanvasScaler;
using System.Net;

namespace ProtoRTS.Game
{
	public class Order_patrol : OrderUnit
	{

        public class OrderEntry_patrol : OrderEntry
        {
            public bool direction_a = false;
        }


        private bool dir_a = false;


        public override void Active()
        {
            gameUnit.move_TargetUnit = null;

            Vector3 targetPoint = targetPos;

            if (dir_a)
            {
                targetPoint = targetPos;
            }
            else
            {
                targetPoint = targetPos2;
            }

            float distance = Vector3.Distance(gameUnit.transform.position, targetPoint);

            if (distance < gameUnit.Class.Radius + 0.5f)
            {
                dir_a = !dir_a;
            }


            gameUnit.move_Target = targetPoint;
            gameUnit.RVO_LockWhenNotMoving(false);
        }

        public override OrderEntry NewOrder(OrderClass orderClass, string buttonID, GameUnit targetUnit, Vector3 targetPosition)
        {
            OrderEntry_patrol newOrder = new OrderEntry_patrol();
            newOrder.targetPosition = targetPosition;
            newOrder.targetUnit = targetUnit;
            newOrder.orderClass = orderClass;
            newOrder.buttonID = buttonID;
            newOrder.targetPosition2 = gameUnit.transform.position;
            return newOrder;
        }

        public override void CheckBackground()
        {
             
        }

        public override bool IsButtonAllowed()
        {
            return true;
        }

        public override Vector3 TargetPosition()
        {
            Vector3 targetPoint = targetPos;

            if (dir_a)
            {
                targetPoint = targetPos;
            }
            else
            {
                targetPoint = targetPos2;
            }

            targetPoint += new Vector3(0, 0.07f, 0);

            return targetPoint;
        }

        public override OrderEntry Save()
        {
            OrderEntry_patrol saveDat = new OrderEntry_patrol();
            saveDat.PassData(this);
            saveDat.direction_a = dir_a;
            return saveDat;
        }

        public override void LoadData(OrderEntry saveData)
        {
            OrderEntry_patrol entryData = saveData as OrderEntry_patrol;
            dir_a = entryData.direction_a;

        }

        public override UnitAbility.TargetType[] TargetCriteria()
        {
            return new UnitAbility.TargetType[2] { UnitAbility.TargetType.Position, UnitAbility.TargetType.SingleUnit };
        }

    }
}