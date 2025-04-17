using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using static UnityEngine.UI.CanvasScaler;
using static UnityEngine.UI.Image;
using UnityEngine.UIElements;

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

            if (hittedUnit != null)
            {
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
            Orders.UnitOrder order_unit = null;

            if (targetSelector_commandOrder.abilityType == UnitButtonCommand.AbilityOrder.Move)
            {
                order_unit = new Orders.Order_Move(targetUnit, targetPos);
            }


            foreach (var gameUnit in Selection.AllSelectedUnits)
            {
                if (targetSelector_commandOrder.abilityType == UnitButtonCommand.AbilityOrder.Patrol) order_unit = new Orders.Order_Patrol(gameUnit.transform.position, targetPos);


                gameUnit.OrderHandler.OverrideCommandOrder(order_unit);
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

            UI.PromptHelp.OpenPrompt("Left click to select target.");       
            UI.AbilityUI.RefreshCommandCard(commandCard);
            UI.BoxSelect.disableBoxSelect = true;
            targetSelector_Opened = true;
        }

        public void CloseTargetSelector()
        {
            UI.PromptHelp.ClosePrompt();
            UI.BoxSelect.disableBoxSelect = false;
            UI.AbilityUI.ReloadCommandCard();
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
            if (commandOrder.abilityType == UnitButtonCommand.AbilityOrder.Move)
            {
                OpenTargetSelector(unit, new UnitAbility.TargetType[2] { UnitAbility.TargetType.Position, UnitAbility.TargetType.SingleUnit }, commandOrder);
            }
            if (commandOrder.abilityType == UnitButtonCommand.AbilityOrder.Stop)
            {
                foreach (var gunit in Selection.AllSelectedUnits) gunit.OrderHandler.OverrideCommandOrder(new Orders.Order_Stop());
            }
            if (commandOrder.abilityType == UnitButtonCommand.AbilityOrder.Patrol)
            {
                OpenTargetSelector(unit, new UnitAbility.TargetType[2] { UnitAbility.TargetType.Position, UnitAbility.TargetType.SingleUnit }, commandOrder);
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
            bool queueOrder = false;

            if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                queueOrder = true;
            }

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

            int index = 0;
			foreach(var unit in Selection.AllSelectedUnits)
			{
                if (unit.stat_faction != SyntiosEngine.CurrentFaction) continue;
				//set rally point
				if (unit.CheckFlag(Unit.Tag.Factory) && unit.CheckFlag(Unit.Tag.Structure) && hittedUnit == null)
				{
                    PrepareOrder_SetRallyPoint(unit, hit.point);
                }
				//move order
				else if (hittedUnit == null)
                {
                    PrepareOrder_Move(unit, null, hit.point, index);
                }
                //check whether to follow, attack, heal or enter unit
                else if (hittedUnit != null)
                {
					//friendly
                    if (hittedUnit.stat_faction == unit.stat_faction)
					{
						PrepareOrder_Move(unit, hittedUnit);
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
                        PrepareOrder_Attack(unit, hittedUnit, hit.point);
                    }
                }

                index++;
            }


            CreateCursor(hit2.point);

            //MoveUnitsHere(hit.point);

            if (Selection.GetPortraitedUnit != null)
            {
                var unit1 = Selection.GetPortraitedUnit;

                SyntiosEvents.UI_OrderMove?.Invoke(unit1);

            }
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


            Formation f = new Formation(origin.Class.Radius, positionTarget);
            allFormations[index] = f;

            //prepare formation
            Orders.Order_Move order_unit = new Orders.Order_Move(gameUnit, positionTarget);

            if (!isQueueingOrder) origin.OrderHandler.OverrideCommandOrder(order_unit);
            else origin.OrderHandler.AddCommandOrder(order_unit);
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