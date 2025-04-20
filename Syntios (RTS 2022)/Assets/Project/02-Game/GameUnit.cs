using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using Pathfinding.RVO;
using Sirenix.OdinInspector;
using System;
using ProtoRTS.Game;

namespace ProtoRTS
{

	public class GameUnit : AbstractUnit
	{
		[ReadOnly] public string guid = "";
		public Vector3 move_Target;
        public GameUnit move_TargetUnit;
        public float targetY = 0;
		[FoldoutGroup("References")] public Renderer[] modelView;
		[FoldoutGroup("References")] public FollowerEntity followerEntity; //change to modular
		[FoldoutGroup("References")] public RVOController rvoController; //change to modular
		[FoldoutGroup("References")] public AIPath groundAIPath; //change to modular

		private GameUnit_OrderController _orderHandler;
		private bool _isVisibleFromFOW = false;
        private bool _isLoadedSaveFile = false;



        public float Radius
        {
            get
            {
				if (followerEntity != null) return followerEntity.radius;
				return groundAIPath.radius;
            }
        }

        public bool IsVisibleFromFOW { get => _isVisibleFromFOW;  }
        public GameUnit_OrderController OrderHandler { get => _orderHandler;  }


        public override void Awake()
        {
			base.Awake();
            _orderHandler = GetComponentInChildren<GameUnit_OrderController>();
        }

        private void Start()
		{
            if (_isLoadedSaveFile == false)
            {
                move_Target = transform.position;
				move_Target.y = Map.instance.GetPositionY_cliffLevel(move_Target);
            }

            SyntiosEngine.Instance.AddNewUnit(this);
			SetUnitStat();
            DynamicAssetStorage.Instance.RegisterCustomMaterial_GameUnit(this);
			DynamicAssetStorage.Instance.OverrideCustomMaterial_GameUnit(this);

			if (rvoController != null)
			{
				if (false) 
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
				
				if (Class.IsFlyUnit)
				{
                    rvoController.layer = RVOLayer.Layer9;
                    rvoController.collidesWith = RVOLayer.Layer9;

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

			if (followerEntity != null)
			{
			}
            if (groundAIPath) groundAIPath.onSearchPath += Update;
			Tick.OnTick += OnTickUnit;

		}

		void OnDisable()
		{
			if (followerEntity != null)
			{
			}
            if (groundAIPath) groundAIPath.onSearchPath -= Update;

		}

        private void OnDestroy()
        {
			SyntiosEngine.Instance.RemoveUnit(this);
        }


        public static GameUnit CreateUnit(SaveData.UnitData unitData, SO_GameUnit gameunitSO)
        {
            var unit = Instantiate(gameunitSO.basePrefab);
			if (unit == null) return null;

            unit.transform.position = unitData.unitPosition;
			unit.move_Target = unitData.move_TargetPos;
			unit.stat_faction = unitData.stat_Faction;
			unit.stat_HP = (int)unitData.stat_HP;
			unit.stat_Energy = (int)unitData.stat_Energy;
			unit.guid = unitData.guid;
			unit._isLoadedSaveFile = true;

			{
				unit.OrderHandler.orders.AddRange(unitData.allOrders);
			}

            return unit;
        }

		public bool CheckFlag(Unit.Tag tag)
		{
			return Class.AllUnitTags.Contains(tag);
		}

        void Update()
		{

			if (followerEntity != null)
			{
				//followerEntity.SetDestination(target);
			}

            if (groundAIPath && Class.IsFlyUnit)
            {
                Vector3 targetPos_fly = transform.position;
                Vector3 targetPos_fly_forward = transform.position + transform.forward * 4f;
                Vector3 targetPos_fly_backward = transform.position + transform.forward * -4f;

                targetPos_fly.y = Map.instance.GetPositionY_cliffLevel(targetPos_fly);
                targetPos_fly_forward.y = Map.instance.GetPositionY_cliffLevel(targetPos_fly_forward);
                targetPos_fly_backward.y = Map.instance.GetPositionY_cliffLevel(targetPos_fly_backward);

                //pick highest point
                if (targetPos_fly_forward.y > targetPos_fly.y) targetPos_fly.y = targetPos_fly_forward.y;
                if (targetPos_fly_backward.y > targetPos_fly.y) targetPos_fly.y = targetPos_fly_backward.y;

                float delta = Mathf.Abs(targetPos_fly.y - transform.position.y);
				delta = Mathf.Clamp(delta, 0.25f, 2.0f);

                transform.position = Vector3.MoveTowards(transform.position, targetPos_fly, 10f * delta * Time.deltaTime);
            }
        }

		private void OnTickUnit(int tick)
		{
			if (groundAIPath != null)
			{
               
                if (move_TargetUnit != null)
				{
					Vector3 pos_targetUnit = move_TargetUnit.transform.position;
                    groundAIPath.destination = pos_targetUnit;
                }
                else
				{
                    groundAIPath.destination = move_Target;
                }

				
            }
        }


		public void SetUnitStat()
        {
			stat_HP = _class.MaxHP;
        }

        [FoldoutGroup("DEBUG")]
		[Button("Kill Unit")]
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

        #region Functions

		public void RVO_LockWhenNotMoving(bool lockNotMoving)
		{
			if (rvoController == null) return;
			rvoController.lockWhenNotMoving = lockNotMoving;
			if (lockNotMoving == false) rvoController.locked = false;
		}

		//public bool IsUnitHasAbility(UnitButtonCommand.AbilityOrder abilityType)
		//{
		//	if ()
		//}

        #endregion
    }
}