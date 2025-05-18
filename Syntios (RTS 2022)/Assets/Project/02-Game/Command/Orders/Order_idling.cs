using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ProtoRTS.Game
{
	public class Order_idling : OrderUnit
	{

		public override void Active()
        {
            GameUnit anyEnemyInDistance = gameUnit.closest_attackableUnit;

            if (anyEnemyInDistance != null)
            {
                float dist = Vector3.Distance(gameUnit.transform.position, anyEnemyInDistance.transform.position);
                int acquireDist = Mathf.Clamp(gameUnit.Class.LineOfSight - 2, 2, 12);

                if (dist > acquireDist)
                {
                    //if it can move, also move
                    if (gameUnit.behaviorTable.HasOrder(OrderClass.order_move))
                    {
                        gameUnit.move_Target = anyEnemyInDistance.transform.position;
                        //play animation
                    }

                    gameUnit.weaponHandler.Attacking();
                }
                else
                {

                }
            }
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
            return Vector3.zero;
        }

        public override UnitAbility.TargetType[] TargetCriteria()
        {
            return new UnitAbility.TargetType[0];
        }

	}
}