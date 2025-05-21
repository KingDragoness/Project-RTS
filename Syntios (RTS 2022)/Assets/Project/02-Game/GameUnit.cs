using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Pathfinding;
using Pathfinding.RVO;
using Sirenix.OdinInspector;
using System;
using ProtoRTS.Game;
using System.Linq;
using UnityEngine.UIElements;
using System.Runtime.CompilerServices;
using static UnityEngine.UI.CanvasScaler;

namespace ProtoRTS
{

    public enum UnitAnimationType
    {
        Idle,
        Moving,
        Attack,
        CastSpell,
        Building_DoingSomething,
        Building_Upgrade,
        Custom1 = 100,
        Custom2,
        Custom3,
        Custom4,
        Custom5
    }

	public class GameUnit : AbstractUnit
	{
		[ReadOnly] public string guid = "";
		public Vector3 move_Target;
        public GameUnit move_TargetUnit;
        public Vector3 trainRallyPoint;
        public GameUnit closest_attackableUnit; //Assign by targetSelector (we delegate to system because this is heavy)
        public BehaviorTable behaviorTable;
        public float targetY = 0;
        [FoldoutGroup("Training")] public List<TrainingQueue> trainingQueue = new List<TrainingQueue>();

        [FoldoutGroup("References")] public Animator mainBodyAnimator;
        [FoldoutGroup("References")] public Animator upperBodyAnimator;
        [FoldoutGroup("References")] public UnitWeaponHandler weaponHandler;
        [FoldoutGroup("References")] public Renderer[] modelView;
        [FoldoutGroup("References")] public FollowerEntity followerEntity; //change to modular
		[FoldoutGroup("References")] public RVOController rvoController; //change to modular
		[FoldoutGroup("References")] public AIPath groundAIPath; //change to modular

		private bool _isVisibleFromFOW = false;
        private bool _isVisible_1TickAgo = false;
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
        public bool IsVisible_1_Tick_ago { get => _isVisible_1TickAgo; }


        public override void Awake()
        {
			base.Awake();

            //set up behavior table
            {
                InitializeEverything();
                
            }
            SyntiosEngine.Instance.AddNewUnit(this);

        }

		public string ID
		{
			get
			{
				return Class.ID;
			}
		}

        #region Initialization and Destroy
        private void Start()
        {
            if (_isLoadedSaveFile == false)
            {
                move_Target = transform.position;
                move_Target.y = Map.instance.GetPositionY_cliffLevel(move_Target);
                trainRallyPoint = transform.position;
            }

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
            Tick.OnTick -= OnTickUnit;

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
            unit.trainRallyPoint = unitData.trainRallyPoint;
            unit.stat_faction = unitData.stat_Faction;
            unit.stat_HP = (int)unitData.stat_HP;
            unit.stat_Energy = (int)unitData.stat_Energy;
            unit.stat_Shield = (int)unitData.stat_Shield;
            unit.stat_isHallucination = unitData.stat_isHallucination;
            unit.stat_isCloaking = unitData.stat_isCloaked;
            unit.guid = unitData.guid;
            unit._isLoadedSaveFile = true;
            unit.trainingQueue.AddRange(unitData.trainingQueue);
            foreach (var queue in unit.trainingQueue) { queue.ResolveReference(); }
            unit.behaviorTable.allQueuedOrders = unitData.allOrders;
            unit.behaviorTable.LoadBehaviorData();
            //unit.InitializeEverything();

            return unit;
        }

        public void SecondPass_LoadedUnit(SaveData.UnitData unitData, SO_GameUnit gameunitSO)
        {
            foreach (var orderDat in unitData.allOrders)
            {
                var orderinUnit = behaviorTable.allOrdersAvailable.Find(x => x.GetClassType() == orderDat.orderClass);
                if (orderinUnit == null) continue;

                var targetUnit = SyntiosEngine.GetUnit(orderDat.targetUnitID);

                orderinUnit.targetUnit = targetUnit;
                orderinUnit.targetPos = orderDat.targetPosition;
            }

            //behaviorTable.allQueuedOrders.AddRange(unitData.allOrders);
        }

        public void InitializeEverything()
        {
            var goBehaviorTable = new GameObject("Behaviors");
            goBehaviorTable.transform.SetParent(transform);
            goBehaviorTable.transform.position = transform.position;
            goBehaviorTable.AddComponent<BehaviorTable>();
            behaviorTable = goBehaviorTable.GetComponent<BehaviorTable>();
            behaviorTable.enabled = true;
            behaviorTable.gameUnit = this;
        }

        #endregion


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


        #region TICKS
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
            float deltaTime = 1f / (float)Tick.TicksPerSecond;

            //if spellcaster
            if (Class.HasEnergy)
            {

                if (stat_Energy < Class.MaxMana() && tick % 20 == 0)
                {
                    stat_Energy++;
                }
            }

            if (Class.HasShield)
            {

                if (stat_Shield < Class.MaxShield && tick % 40 == 0)
                {
                    stat_Shield++;
                }
            }

            //handle training here
            Tick_TRAIN();
       
        }

        private void Tick_TRAIN()
        {
            if (trainingQueue.Count == 0)
            {
                return;
            }

            float deltaTime = 1f / (float)Tick.TicksPerSecond;
            var currentTrainQueue = trainingQueue[0];

            if (currentTrainQueue.gameUnitClass == null)
            {
                trainingQueue.RemoveAt(0);
                Debug.Log("EMPTY CLASS!");
                return;
            }

            currentTrainQueue.timeTrained += deltaTime * SyntiosEngine.MultiplierTrainingSpeed;

            if (currentTrainQueue.timeTrained > currentTrainQueue.gameUnitClass.BuildTime)
            {
                //Create unit
                Vector3 spawnPos = transform.position;
                if (spawnPos.z > Class.Radius + 1)
                {
                    spawnPos.z -= 1f + (Class.Radius * 1f);
                }
                else
                {
                    spawnPos.z += 1f + (Class.Radius * 1f);
                }

                spawnPos.x += UnityEngine.Random.Range(-1f, 1f);

                var newUnit = currentTrainQueue.gameUnitClass.basePrefab.TrainSpawnUnit();
                newUnit.transform.position = spawnPos;
                newUnit.behaviorTable.IssueOrder(OrderClass.order_move, buttonID: "order_move", targetPosition: trainRallyPoint, isQueueing: QueueOrder.Override);
                trainingQueue.RemoveAt(0);
            }

        }

        #endregion

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

        #region Functions
        

        public void HideModel()
        {
            _isVisibleFromFOW = false;
            _isVisible_1TickAgo = false;
            foreach (var meshRrndr in modelView) meshRrndr.enabled = false;

        }

        public void ShowModel()
        {
            _isVisibleFromFOW = true;
            _isVisible_1TickAgo = true;
            foreach (var meshRrndr in modelView) meshRrndr.enabled = true;

        }

        [FoldoutGroup("DEBUG")]
        [Button("Collect Mesh Renderers")]
        public void DEBUG_CollectMeshRenderers()
        {
            var meshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>();

            modelView = meshRenderers;

        }


        public GameUnit TrainSpawnUnit()
        {
            var newUnit = Instantiate(this, transform.position, transform.rotation);
            newUnit.gameObject.SetActive(true);

            return newUnit;
        }

        public void RVO_LockWhenNotMoving(bool lockNotMoving)
		{
			if (rvoController == null) return;
			rvoController.lockWhenNotMoving = lockNotMoving;
			if (lockNotMoving == false) rvoController.locked = false;
		}

        public void AnimationPlay(UnitAnimationType animType)
        {

        }

        //public bool IsUnitHasAbility(UnitButtonCommand.AbilityOrder abilityType)
        //{
        //	if ()
        //}

        #endregion

        #region Get Unit Properties/Datas


        public FactionSheet Unit_FactionSheet()
        {
            return SyntiosEngine.Instance.GetFactionSheet(stat_faction);
        }

        public bool CheckFlag(Unit.Tag tag)
        {
            return Class.AllUnitTags.Contains(tag);
        }

        internal bool IsAir()
        {
            //Liftoff unit is in other state
            return Class.IsFlyUnit;
        }

        #endregion

    }
}