using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using Pathfinding.RVO;
using Sirenix.OdinInspector;
using System;

namespace ProtoRTS
{

	public class GameUnit : AbstractUnit
	{
		public Vector3 target;
		public float targetY = 0;
		[FoldoutGroup("References")] public MeshRenderer[] modelView;
		[FoldoutGroup("References")] public FollowerEntity followerEntity; //change to modular
		[FoldoutGroup("References")] public RVOController rvoController; //change to modular
		[FoldoutGroup("References")] public AIPath ai; //change to modular

		private bool _isVisibleFromFOW = false;



		public float Radius
        {
            get
            {
				if (followerEntity != null) return followerEntity.radius;
				return ai.radius;
            }
        }

        public bool IsVisibleFromFOW { get => _isVisibleFromFOW;  }

        private void Start()
		{
			SyntiosEngine.Instance.AddNewUnit(this);
			SetUnitStat();
            DynamicAssetStorage.Instance.RegisterCustomMaterial_GameUnit(this);
			DynamicAssetStorage.Instance.OverrideCustomMaterial_GameUnit(this);
		}

  
		public string ID
		{
			get
			{
				return Class.ID;
			}
		}

        void OnEnable()
		{
			target = transform.position;

			if (followerEntity != null)
			{
			}
			ai.onSearchPath += Update;
			Tick.OnTick += OnTickUnit;

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
			SyntiosEngine.Instance.RemoveUnit(this);
        }

        void Update()
		{

			if (followerEntity != null)
			{
				followerEntity.SetDestination(target);
			}

		}

		private void OnTickUnit()
		{
			ai.destination = target;
		}


		public void SetUnitStat()
        {
			stat_HP = _class.MaxHP;
        }

		public void KillUnit()
        {
			stat_HP = 0;
			Destroy(gameObject);
        }

		public void HideModel()
        {
			_isVisibleFromFOW = false;
			foreach (var meshRrndr in modelView) meshRrndr.enabled = false;

		}

		public void ShowModel()
        {
			_isVisibleFromFOW = true;
			foreach (var meshRrndr in modelView) meshRrndr.enabled = true;

		}

		[FoldoutGroup("DEBUG")]
		[Button("Collect Mesh Renderers")]
		public void DEBUG_CollectMeshRenderers()
        {
			var meshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>();

			modelView = meshRenderers;

		}
	}
}