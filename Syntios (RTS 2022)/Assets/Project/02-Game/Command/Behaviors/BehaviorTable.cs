using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ProtoRTS.Game
{
	public class BehaviorTable : MonoBehaviour
	{

		public List<GameUnitBehavior> allBehaviors = new List<GameUnitBehavior>();
		public GameUnit gameUnit;

	}
}