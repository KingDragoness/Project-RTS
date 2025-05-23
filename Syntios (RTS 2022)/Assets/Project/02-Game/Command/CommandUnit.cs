using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using static UnityEngine.UI.CanvasScaler;
using static UnityEngine.UI.Image;
using UnityEngine.UIElements;
using UnityEngine.EventSystems;

namespace ProtoRTS.Game
{
	public class CommandUnit : MonoBehaviour
	{

		public Transform lastCursorT;
        public CommandCard commandCard;
        public LayerMask layerMask_terrain;
		public bool DEBUG_DrawCircle = false;

		private Formation[] allFormations = new Formation[1];
        private bool input_rightButtonDown = false;

        private GameUnit targetSelector_currentGameUnit;
        private UnitAbility.TargetType[] targetSelector_targetType;
        private UnitButtonCommand targetSelector_commandOrder;
        private bool targetSelector_Opened = false;

        public static CommandUnit Instance;

        private void Awake()
        {
            Instance = this;
            SyntiosEvents.UI_ReselectUpdate += event_UI_ReselectUpdate;
            Tick.OnTick += event_OnTick;
        }


        private void OnDestroy()
        {
            SyntiosEvents.UI_ReselectUpdate -= event_UI_ReselectUpdate;
            Tick.OnTick -= event_OnTick;
        }


        private void event_OnTick(int tick)
        {

        }

        private void event_UI_ReselectUpdate()
        {

            if (Selection.AllSelectedUnits.Find(x => x == targetSelector_currentGameUnit) == null && targetSelector_Opened)
            {
                if (Selection.AllSelectedUnits.Count == 0 && targetSelector_Opened)
                {
                    //really no unit left
                    CloseTargetSelector();
                    return;

                }
                while (targetSelector_currentGameUnit == null)
                {
                    targetSelector_currentGameUnit = Selection.GetPortraitedUnit;
                }
                //foreach (var button in buttons) { button.gameObject.SetActive(false); }
            }
            else
            {

            }

            if (targetSelector_currentGameUnit == null && targetSelector_Opened)
            {
                //CloseTargetSelector();
            }
        }

        bool isQueueingOrder = false;

        private void Update()
		{
			input_rightButtonDown = false;

			//block if there is UI
			if (Input.GetMouseButtonDown(1))
			{

                if (MainUI.GetEventSystemRaycastResults().Count > 0)
				{
				}
                else
                {
					Handle_RightClick();
				}
			}

            if (Input.GetKey(KeyCode.LeftControl))
            {
                isQueueingOrder = true;
            }
            else isQueueingOrder = false;


            if (targetSelector_Opened)
            {
                UpdateTargetSelector();
            }
            else
            {

            }

			if (DEBUG_DrawCircle) DEBUG_DrawFormation();

		}

        public void MinimapClickDrop(PointerEventData eventData, Vector3 worldPos)
        {
            if (targetSelector_Opened && eventData.button == PointerEventData.InputButton.Left)
            {
                ExecuteOrder(worldPos, null);
                CloseTargetSelector();
            }
            if (!targetSelector_Opened && eventData.button == PointerEventData.InputButton.Right)
            {
                QuickOrder(worldPos, null);
            }
        }

        #region Target Selector

        private GameUnit _previewedGameUnit;

        public void UpdateTargetSelector()
        {
            GameUnit hittedUnit = null;


            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 2048f))
            {
                if (hit.collider.gameObject.CompareTag("Unit"))
                {
                    var parentGameunit = hit.collider.gameObject.GetComponentInParent<GameUnit>();

                    if (parentGameunit != null)
                    {
                        hittedUnit = parentGameunit;
                    }
                }
            }

            UI.BoxSelect.disableBoxSelectTime = 1f;


            if (hittedUnit != null)
            {
                if (hittedUnit.IsVisibleFromFOW)
                    hittedUnit.HighlightUnit();
            }

            if (_previewedGameUnit != hittedUnit && _previewedGameUnit != null)
            {
                _previewedGameUnit.DehighlightUnit();
            }

            _previewedGameUnit = hittedUnit;

            Cursor.Change(Cursor.Type.SelectTarget);

            if (Input.GetKeyUp(KeyCode.Escape))
            {
                CloseTargetSelector();
            }

            if (Input.GetMouseButtonUp(0) && MainUI.GetEventSystemRaycastResults().Count <= 0)
            {
                ExecuteOrder(hit.point, hittedUnit);
                CloseTargetSelector();
            }
        }

        public void ExecuteOrder(Vector3 targetPos, GameUnit targetUnit)
        {
            OrderUnit order_unit = null;

            if (targetSelector_commandOrder.orderClass == OrderClass.order_move)
            {

            }


            foreach (var gameUnit in Selection.AllSelectedUnits)
            {
                string buttonID = gameUnit.Class.GetSimilarButton(targetSelector_commandOrder.buttonID);
                gameUnit.behaviorTable.IssueOrder(targetSelector_commandOrder.orderClass, buttonID, targetUnit, targetPos);
                //if (targetSelector_commandOrder.orderClass == OrderClass.order_move) gameUnit.behaviorTable.IssueOrder(, targetPos, targetUnit);
                //if (targetSelector_commandOrder.abilityType == UnitButtonCommand.AbilityOrder.Patrol) gameUnit.OrderHandler.GiveOrder(new Orders.Order_Patrol(gameUnit.transform.position, targetPos), true);
                //if (targetSelector_commandOrder.abilityType == UnitButtonCommand.AbilityOrder.HoldPosition) gameUnit.OrderHandler.GiveOrder(new Orders.Order_HoldPosition(), true);

            }

            if (Selection.GetPortraitedUnit != null)
            {
                var unit1 = Selection.GetPortraitedUnit;

                SyntiosEvents.UI_OrderMove?.Invoke(unit1);

            }

            CreateCursor(targetPos);
        }

        public void OpenTargetSelector(GameUnit gameUnit, UnitAbility.TargetType[] _targetType, UnitButtonCommand commandOrder)
        {
            targetSelector_currentGameUnit = gameUnit;
            targetSelector_targetType = _targetType;
            targetSelector_commandOrder = commandOrder;

            if (commandOrder.orderClass == OrderClass.order_move) UI.PromptHelp.OpenPrompt("Left click to where move/follow unit.");
            if (commandOrder.orderClass == OrderClass.order_patrol) UI.PromptHelp.OpenPrompt("Left click to where patrol.");

            UI.AbilityUI.RefreshCommandCard(commandCard);
            UI.BoxSelect.disableBoxSelectTime = 1f;
            UI.CommandMap.minimap.DisableInput = true;
            targetSelector_Opened = true;
        }

        public void CloseTargetSelector()
        {
            UI.PromptHelp.ClosePrompt();
            UI.BoxSelect.disableBoxSelectTime = 0.2f;
            UI.AbilityUI.ReloadCommandCard();
            UI.CommandMap.minimap.DisableInput = false;

            if (_previewedGameUnit != null)
            {
                _previewedGameUnit.DehighlightUnit();
            }

            targetSelector_currentGameUnit = null;
            targetSelector_Opened = false;
            Cursor.Change(Cursor.Type.Normal);
        }

        #endregion



        /// <summary>
        /// Guaranteed ability command
        /// </summary>
        /// <param name="commandOrder"></param>
        public void  CommandUI_ExecuteCommand(GameUnit unit, UnitButtonCommand commandOrder)
        {
            var pseudoClass = OrderUnit.NewClassUnit(commandOrder.orderClass);

            if (pseudoClass == null)
            {
                string s_debug = $"Game still under development. Missing class: {commandOrder.orderClass}";
                Debug.LogError(s_debug);
                DevConsole.Instance.SendConsoleMessage(s_debug);
            }

            var criteriaTarget = pseudoClass.TargetCriteria();

            if (criteriaTarget.Length > 0)
            {
                OpenTargetSelector(unit, criteriaTarget, commandOrder);
            }
            else
            {
                foreach (var gunit in Selection.AllSelectedUnits) gunit.behaviorTable.IssueOrder(commandOrder.orderClass, commandOrder.buttonID);
            }

        }


        /// <summary>
        /// There were several possibilities for right click:
        /// MOVE				STACKABLE	(if there is nothing or target unit is friendly)	
        /// ATTACK				STACKABLE	(if unit has attack ability and target unit is enemy)
        /// HEAL							(if unit has heal ability ALSO "repair" too)
        /// SET RALLY POINT					(if building has flag "factory")
        /// ENTER UNIT (from non-transport)	(if unit has flag "enterable", then do check passes)
        /// ENTER UNIT (from transport)		(if target unit has flag "enterable", then do check passes)
        /// </summary>



        private void Handle_RightClick()
        {
			GameUnit hittedUnit = null;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            RaycastHit hit2;

            if (Physics.Raycast(ray, out hit, 2048f))
            {
                if (hit.collider.gameObject.CompareTag("Unit"))
                {
                    var parentGameunit = hit.collider.gameObject.GetComponentInParent<GameUnit>();

                    if (parentGameunit != null)
                    {
                        hittedUnit = parentGameunit;
                    }
                }
            }

            Ray ray2 = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit2, 2048f, layerMask_terrain))
            {

            }

            int count = Selection.AllSelectedUnits.Count;

            var mainSelectedUnit = Selection.GetPortraitedUnit;
            allFormations = new Formation[count];


            if (targetSelector_Opened == false && Selection.AllSelectedUnits.Count > 0)
            {
                QuickOrder(hit.point, hittedUnit);
                CreateCursor(hit2.point);

            }


        }

        public void QuickOrder(Vector3 pos, GameUnit hittedUnit)
        {
            int index = 0;
            foreach (var unit in Selection.AllSelectedUnits)
            {
                if (unit.stat_faction != SyntiosEngine.CurrentFaction) continue;
                //set rally point
                if (unit.CheckFlag(Unit.Tag.Factory) && unit.CheckFlag(Unit.Tag.Structure) && hittedUnit == null)
                {
                    PrepareOrder_SetRallyPoint(unit, pos);
                }
                //move order
                else if (hittedUnit == null)
                {
                    PrepareOrder_Move(unit, null, pos, index);
                }
                //check whether to follow, attack, heal or enter unit
                else if (hittedUnit != null)
                {
                    //friendly
                    if (hittedUnit.stat_faction == unit.stat_faction)
                    {
                        PrepareOrder_Move(unit, hittedUnit, pos);
                    }
                    //heal
                    else if (hittedUnit.stat_faction == unit.stat_faction && unit.CheckFlag(Unit.Tag.Healer))
                    {
                        PrepareOrder_Heal(unit, hittedUnit);
                    }
                    //enter/exit uni
                    else if (hittedUnit.stat_faction == unit.stat_faction && unit.CheckFlag(Unit.Tag.Enterable))
                    {
                        PrepareOrder_Enterable(unit, hittedUnit);
                    }
                    //enemy
                    else if (hittedUnit.stat_faction != unit.stat_faction)
                    {
                        PrepareOrder_Attack(unit, hittedUnit, pos);
                    }
                }

                //index++;
            }

            if (Selection.GetPortraitedUnit != null)
            {
                var unit1 = Selection.GetPortraitedUnit;

                SyntiosEvents.UI_OrderMove?.Invoke(unit1);

            }

            CreateCursor(pos);

        }

        public void CreateCursor(Vector3 position)
        {
            lastCursorT.gameObject.SetActive(true);
            {
                var pss = lastCursorT.GetComponentsInChildren<ParticleSystem>();
                foreach (var ps in pss)
                {
                    ps.Play();
                }

                float yCliff = Map.instance.GetPositionY_cliffLevel(position);
                Vector3 pos_hit1 = position;
                pos_hit1.y = yCliff;
                lastCursorT.transform.position = pos_hit1;


            }
        }

		private void PrepareOrder_Move(GameUnit origin, GameUnit gameUnit, Vector3 posTarget = new Vector3(), int index = 0)
		{
            int count = Selection.AllSelectedUnits.Count;
            int rows_col = Mathf.RoundToInt(Mathf.Sqrt(count));
            float offset = rows_col / 2f;

            int curr_column = Mathf.FloorToInt(index / rows_col);
            int curr_row = (index % rows_col);
            Vector3 positionTarget = posTarget;

            //positionTarget.x -= offset * origin.Class.Radius * 2f;
            //positionTarget.z -= offset * origin.Class.Radius * 2f;

            //positionTarget.x += curr_row * origin.Class.Radius * 2f;
            //positionTarget.z += curr_column * origin.Class.Radius * 2f;


            //Formation f = new Formation(origin.Class.Radius, positionTarget);
            //allFormations[index] = f;

            //prepare formation


            if (!isQueueingOrder) 
                origin.behaviorTable.IssueOrder(OrderClass.order_move, "order_move", gameUnit, positionTarget, QueueOrder.Override);
            else
            {
                var currBehavior = origin.behaviorTable.GetCurrentBehavior();

                if (currBehavior != null)
                {
                   
                    if (currBehavior.IgnoreQueue() == false)
                    {
                        origin.behaviorTable.IssueOrder(OrderClass.order_move, "order_move", gameUnit, positionTarget, QueueOrder.Additive);
                    }
                    else
                    {
                        //Overrides for specific use-case: Hold Position (.IgnoreQueue() == true in holdPosition)
                        origin.behaviorTable.IssueOrder(OrderClass.order_move, "order_move", gameUnit, positionTarget, QueueOrder.Override);
                    }
                }
                else
                {
                    origin.behaviorTable.IssueOrder(OrderClass.order_move, "order_move", gameUnit, positionTarget, QueueOrder.Additive);
                }
            }
            //origin.OrderHandler.Order_MOVE(gameUnit, positionTarget);

		}

        private void PrepareOrder_Heal(GameUnit origin, GameUnit gameUnit)
        {

        }

        private void PrepareOrder_Attack(GameUnit origin, GameUnit gameUnit, Vector3 posTarget = new Vector3())
        {

        }

        private void PrepareOrder_Enterable(GameUnit origin, GameUnit enterableUnit)
        {

        }

        private void PrepareOrder_SetRallyPoint(GameUnit origin, Vector3 posTarget)
        {
            origin.behaviorTable.IssueOrder(OrderClass.order_setRallyPoint, "order_setRallyPoint", null, posTarget, QueueOrder.Override);

        }


        private void FixedUpdate()
		{

			if (input_rightButtonDown)
			{
			}


		}

		private void Old_MoveUnitsHere(Vector3 target)
		{
			int index = 0;
			int count = Selection.AllSelectedUnits.Count;
			int rows_col = Mathf.RoundToInt(Mathf.Sqrt(count));
			float offset = rows_col / 2f;

			allFormations = new Formation[count];

			foreach (var unit in Selection.AllSelectedUnits)
			{
				if (SyntiosEngine.CurrentFaction != unit.stat_faction)
				{
					continue;
				}

				var gameUnit = unit.GetComponent<GameUnit>();
				int curr_column = Mathf.FloorToInt(index / rows_col);
				int curr_row = (index % rows_col);
				Vector3 positionTarget = target;

				positionTarget.x -= offset * unit.Class.Radius * 2f;
                positionTarget.z -= offset * unit.Class.Radius * 2f;

                positionTarget.x += curr_row * unit.Class.Radius * 2f;
                positionTarget.z += curr_column * unit.Class.Radius * 2f;


                Formation f = new Formation(unit.Class.Radius, positionTarget);
                allFormations[index] = f;

				gameUnit.move_Target = positionTarget;
                index++;
			}
		}

		private void DEBUG_DrawFormation()
		{
			foreach (var unitPos in allFormations)
			{
				Debug.DrawCircle(unitPos.position, unitPos.radius, Color.red, Vector3.up, 32);
			}
		}

	}
}