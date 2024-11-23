using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using Pathfinding.RVO;
using Sirenix.OdinInspector;

namespace ProtoRTS
{
	public class GameUnit : MonoBehaviour
	{
		public Vector3 target;
		[FoldoutGroup("References")] public FollowerEntity followerEntity; //change to modular
		[FoldoutGroup("References")] public RVOController rvoController; //change to modular
		[FoldoutGroup("References")] public AIPath ai; //change to modular

		[Header("Game Stats")]
		[SerializeField] internal int stat_KillCount;
		[SerializeField] internal int stat_HP = 25;
		[SerializeField] internal int stat_Energy = 0;
		[SerializeField] internal Unit.Player stat_faction;
		[SerializeField] private SO_GameUnit _class; //temporary system



		public SO_GameUnit Class { get => _class; }

		private Transform circleOutline;

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

		public void SelectedUnit(Transform prefab)
        {
			if (circleOutline != null) Destroy(circleOutline.gameObject);

			Transform t1 = Instantiate(prefab, transform, false);
			t1.transform.localPosition = Vector3.zero + new Vector3(0f, 0.2f,0f);
			t1.transform.localScale = Vector3.one * Radius * 2.1f;
			t1.gameObject.SetActive(true);
			circleOutline = t1;
		}

		public void DeselectUnit()
		{
			if (circleOutline != null) Destroy(circleOutline.gameObject);

		}


	}
}