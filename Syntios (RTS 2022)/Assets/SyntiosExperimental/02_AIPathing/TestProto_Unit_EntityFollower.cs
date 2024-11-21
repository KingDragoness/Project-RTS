using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using Sirenix.OdinInspector;

namespace ProtoRTS
{
	public class TestProto_Unit_EntityFollower : MonoBehaviour
	{

		public Vector3 target;
		public FollowerEntity followerEntity;

		void OnEnable()
		{
			target = transform.position;
		}

		void OnDisable()
		{
		}

		void Update()
		{
			followerEntity.SetDestination(target);
		}
	}
}