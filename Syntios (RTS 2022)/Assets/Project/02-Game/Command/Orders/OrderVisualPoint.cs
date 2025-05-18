using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ProtoRTS.Game
{
	public class OrderVisualPoint : MonoBehaviour
	{

		public enum PointType
		{
			OrderQueue,
			RallyPoint
		}

		public Wire wire;
		public Transform circle;
		public PointType pointType;
		[Space]
        public GameUnit attachedUnit;
        public OrderUnit attachedOrder;
		public Vector3Int orderPosTarget;
	
	}
}