using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using static UnityEngine.UI.CanvasScaler;

namespace ProtoRTS.Game
{
	public class Order_holdPosition : OrderUnit
	{

		public override void Active()
        {
            //never completes
            //if enemy exists, attack
            gameUnit.move_Target = gameUnit.transform.position;
            gameUnit.move_TargetUnit = null;
            gameUnit.RVO_LockWhenNotMoving(true);
        }

        public override void CheckBackground()
        {
             
        }

        public override bool IgnoreQueue()
        {
            return true;
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