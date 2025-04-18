using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ProtoRTS.Game
{
	public class OrderVisualPoint : MonoBehaviour
	{

		public Wire wire;
        public Orders.UnitOrder prevUnitOrder;
        public Orders.UnitOrder attachedUnitOrder;
		public GameUnit attachedUnit;
		public List<Orders.UnitOrder> bigWireOrders = new List<Orders.UnitOrder>();
		public Vector3 bigWire_MultipleUnitPos;
        public bool currentOrder = false;
		public bool bigWire = false;

		[Button("Count big wire order")]
		public void BigWireCount()
		{
			Debug.Log(bigWireOrders.Count);
		}
	}
}