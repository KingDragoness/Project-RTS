using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ProtoRTS.Game
{
	public class Order_setRallyPoint : OrderUnit
    {

        public override void Active()
        {
            gameUnit.trainRallyPoint = targetPos;
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
            return new UnitAbility.TargetType[1] { UnitAbility.TargetType.Position };
        }
    }
}