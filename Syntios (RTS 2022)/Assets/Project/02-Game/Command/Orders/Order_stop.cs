using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UIElements;

namespace ProtoRTS.Game
{
	public class Order_stop : OrderUnit
    {
        public override void Active()
        {
            gameUnit.move_Target = gameUnit.transform.position;
            gameUnit.RVO_LockWhenNotMoving(false);
            isCompleted = true;
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