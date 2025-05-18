using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Utilities;

namespace ProtoRTS.Game
{
    public class Order_move : OrderUnit
    {
        public override void Active()
        {
            gameUnit.move_Target = TargetPosition();

            float dist = Vector3.Distance(TargetPosition(), gameUnit.transform.position);

            if (dist < gameUnit.Class.Radius + 0.9f)
            {
                isCompleted = true;
            }

            gameUnit.RVO_LockWhenNotMoving(false);

        }

        public override void CheckBackground()
        {         

        }

  

        public override bool IsButtonAllowed()
        {
            return true;
        }


        public override UnitAbility.TargetType[] TargetCriteria()
        {
            return new UnitAbility.TargetType[2] { UnitAbility.TargetType.Position, UnitAbility.TargetType.SingleUnit };
        }

        public override Vector3 TargetPosition()
        {
            if (targetUnit != null)
            {
                return targetUnit.transform.position;
            }
            else
                return targetPos;
        }
    }
}