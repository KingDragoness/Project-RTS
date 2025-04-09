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
		[FoldoutGroup("References")] public Renderer[] modelView;
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

			if (rvoController != null)
			{
				//if (false) 
				{
					int layerNeutralPlayer = 1 << 10;
                    int layerPlayer1 = 1 << 11;
                    int layerPlayer2 = 1 << 12;
                    int layerPlayer3 = 1 << 13;
                    int layerPlayer4 = 1 << 14;
                    int layerPlayer5 = 1 << 15;
                    int layerPlayer6 = 1 << 16;
                    int layerPlayer7 = 1 << 17;
                    int layerPlayer8 = 1 << 18;

                    int layerObstacles = 1 << 1;

                    if (stat_faction == Unit.Player.neutral) { rvoController.layer = RVOLayer.Layer10; }
                    if (stat_faction == Unit.Player.Player1) { rvoController.layer = RVOLayer.Layer11; }
                    if (stat_faction == Unit.Player.Player2) { rvoController.layer = RVOLayer.Layer12; }
                    if (stat_faction == Unit.Player.Player3) { rvoController.layer = RVOLayer.Layer13; }
                    if (stat_faction == Unit.Player.Player4) { rvoController.layer = RVOLayer.Layer14; }
                    if (stat_faction == Unit.Player.Player5) { rvoController.layer = RVOLayer.Layer15; }
                    if (stat_faction == Unit.Player.Player6) { rvoController.layer = RVOLayer.Layer16; }
                    if (stat_faction == Unit.Player.Player7) { rvoController.layer = RVOLayer.Layer17; }
                    if (stat_faction == Unit.Player.Player8) { rvoController.layer = RVOLayer.Layer18; }


                    int finalMask = layerNeutralPlayer | layerPlayer1 | layerPlayer2 | layerPlayer3 | layerPlayer4 | layerPlayer5 | layerPlayer6 | layerPlayer7 | layerPlayer8 | layerObstacles;

                    rvoController.collidesWith = (RVOLayer)finalMask;
                }
				

            }
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
            if (ai) ai.onSearchPath += Update;
			Tick.OnTick += OnTickUnit;

		}

		void OnDisable()
		{
			if (followerEntity != null)
			{
			}
            if (ai) ai.onSearchPath -= Update;

		}

        private void OnDestroy()
        {
			SyntiosEngine.Instance.RemoveUnit(this);
        }

        void Update()
		{

			if (followerEntity != null)
			{
				//followerEntity.SetDestination(target);
			}

		}

		private void OnTickUnit(int tick)
		{
			if (ai) ai.destination = target;
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