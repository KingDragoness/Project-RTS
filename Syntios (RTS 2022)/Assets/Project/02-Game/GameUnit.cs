using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using Pathfinding.RVO;
using Sirenix.OdinInspector;

namespace ProtoRTS
{

	public class GameUnit : AbstractUnit
	{
		public Vector3 target;
		[FoldoutGroup("References")] public FollowerEntity followerEntity; //change to modular
		[FoldoutGroup("References")] public RVOController rvoController; //change to modular
		[FoldoutGroup("References")] public AIPath ai; //change to modular





		public float Radius
        {
            get
            {
				if (followerEntity != null) return followerEntity.radius;
				return ai.radius;
            }
        }


        private void Start()
		{
			SyntiosEngine.Instance.ListedGameUnits.Add(this);
			SetUnitStat();
		}

		void OnEnable()
		{
			target = transform.position;

			if (followerEntity != null)
			{
			}
			ai.onSearchPath += Update;

		}

		void OnDisable()
		{
			if (followerEntity != null)
			{
			}
			ai.onSearchPath -= Update;

		}

        private void OnDestroy()
        {
			SyntiosEngine.Instance.ListedGameUnits.Remove(this);
        }

        void Update()
		{
			if (followerEntity != null)
			{
				followerEntity.SetDestination(target);
			}
			ai.destination = target;


		}

		public void SetUnitStat()
        {
			stat_HP = _class.MaxHP;
        }




	}
}