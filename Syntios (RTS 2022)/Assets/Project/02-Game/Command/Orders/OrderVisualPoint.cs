using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ProtoRTS.Game
{
	public class OrderVisualPoint : MonoBehaviour
	{

		public Wire wire;
		public Transform circle;
		[Space]
        public GameUnit attachedUnit;
        public Orders.UnitOrder attachedOrder;
		public Vector3Int orderPosTarget;
	
	}
}