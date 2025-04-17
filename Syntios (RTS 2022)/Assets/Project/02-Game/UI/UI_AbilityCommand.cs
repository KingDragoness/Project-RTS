using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

namespace ProtoRTS.Game
{
	public class UI_AbilityCommand : MonoBehaviour
	{

		public List<Button_CommandUnit> buttons;
        public RuntimeAnimatorController anim_White;
        public RuntimeAnimatorController anim_Blue;

        private GameUnit currentGameUnit;
        private CommandCard currentCommandCard;


        private void Awake()
        {
			SyntiosEvents.UI_NewSelection += event_UI_NewSelection;
            SyntiosEvents.UI_ReselectUpdate += event_UI_ReselectUpdate;
            SyntiosEvents.UI_DeselectAll += event_UI_DeselectAll;
            Tick.OnTick += event_OnTick;

        }

        private void OnDestroy()
        {
            SyntiosEvents.UI_NewSelection -= event_UI_NewSelection;
            SyntiosEvents.UI_ReselectUpdate -= event_UI_ReselectUpdate;
            SyntiosEvents.UI_DeselectAll -= event_UI_DeselectAll;
            Tick.OnTick -= event_OnTick;

        }

        private void event_UI_DeselectAll()
        {
            if (Selection.AllSelectedUnits.Find(x => x == currentGameUnit) == null)
            {
                if (Selection.AllSelectedUnits.Count == 0)
                {
                    //really no unit left
                    foreach (var button in buttons) { button.gameObject.SetActive(false); }
                    currentGameUnit = null;
                    currentCommandCard = null;
                    return;
                }

                while (currentGameUnit == null)
                {
                    currentGameUnit = Selection.GetPortraitedUnit;
                    currentCommandCard = currentGameUnit.Class.DefaultCard;
                }
                //foreach (var button in buttons) { button.gameObject.SetActive(false); }
            }
            else
            {

            }
        }


        private void event_UI_ReselectUpdate()
        {
            if (Selection.AllSelectedUnits.Find(x => x == currentGameUnit) == null) 
            {
                if (Selection.AllSelectedUnits.Count == 0)
                {
                    //really no unit left
                    foreach (var button in buttons) { button.gameObject.SetActive(false); }
                    currentGameUnit = null;
                    currentCommandCard = null;
                    return;
                }

                while (currentGameUnit == null)
                {
                    currentGameUnit = Selection.GetPortraitedUnit;
                    currentCommandCard = currentGameUnit.Class.DefaultCard;
                }
                //foreach (var button in buttons) { button.gameObject.SetActive(false); }
            }
            else
            {

            }
        }

        private void event_OnTick(int tick)
        {

        }


        private void event_UI_NewSelection(GameUnit unit)
        {
			foreach (var button in buttons) { button.gameObject.SetActive(false); }
            if (unit.stat_faction != SyntiosEngine.CurrentFaction) return;
            if (unit == null) { return; }
            var commandCard = unit.Class.commandCards[0];

            currentCommandCard = commandCard;
            currentGameUnit = unit;


            foreach (var command in commandCard.commands)
			{
				buttons[command.position].gameObject.SetActive(true);
				buttons[command.position].buttonIcon.sprite = command.button.sprite;
                buttons[command.position].label_Hotkey.text = Key(command.position);
                buttons[command.position].type = command.commandType;
                buttons[command.position].CommandButtonSO = command.button;

                if (command.button.allowTint)
                {
                    buttons[command.position].buttonAnim.runtimeAnimatorController = anim_Blue;
                }
                else
                {
                    buttons[command.position].buttonAnim.runtimeAnimatorController = anim_White;
                }
            }
        }

        public void RefreshCommandCard(CommandCard commandCard)
        {
            foreach (var button in buttons) { button.gameObject.SetActive(false); }

            foreach (var command in commandCard.commands)
            {
                buttons[command.position].gameObject.SetActive(true);
                buttons[command.position].buttonIcon.sprite = command.button.sprite;
                buttons[command.position].label_Hotkey.text = Key(command.position);
                buttons[command.position].type = command.commandType;
                buttons[command.position].CommandButtonSO = command.button;

                if (command.button.allowTint)
                {
                    buttons[command.position].buttonAnim.runtimeAnimatorController = anim_Blue;
                }
                else
                {
                    buttons[command.position].buttonAnim.runtimeAnimatorController = anim_White;
                }

            }
        }

        public void ReloadCommandCard()
        {
            RefreshCommandCard(currentCommandCard);
        }

		public string Key(int keyCode)
		{
			if (keyCode == 0) return "Q";
            if (keyCode == 1) return "W";
            if (keyCode == 2) return "E";
            if (keyCode == 3) return "R";
            if (keyCode == 4) return "A";
            if (keyCode == 5) return "S";
            if (keyCode == 6) return "D";
            if (keyCode == 7) return "F";
            if (keyCode == 8) return "Z";
            if (keyCode == 9) return "X";
            if (keyCode == 10) return "C";
            if (keyCode == 11) return "V";

            return "NULL";
		}

        private void Start()
		{
            int _i = 0;
            foreach (var button in buttons) 
            { 
                button.gameObject.SetActive(false); 
                button.index = _i;
                _i++;
            }

        }

        public void ClickButton(Button_CommandUnit button)
        {
            if (button.type == UnitButtonCommand.Type.CancelTarget) 
            {
                RTS.instance.commandUnit.CloseTargetSelector();
                RefreshCommandCard(currentCommandCard);
                return;
            }

            var commandCard = currentGameUnit.Class.commandCards.Find(x => x.cardName == currentCommandCard.cardName);

            if (commandCard == null) return;

            var command = commandCard.commands.Find(x => x.position == button.index);

            if (command.commandType == UnitButtonCommand.Type.Submenu)
            {
                var nextCC = command.cardToOpen;
                currentCommandCard = currentGameUnit.Class.commandCards.Find(x => x.cardName == nextCC);
                RefreshCommandCard(currentCommandCard);
            }

            if (command.commandType == UnitButtonCommand.Type.AbilityCommand)
            {
                CommandUnit.Instance.CommandUI_ExecuteCommand(currentGameUnit, command);
            }

        }

        public void HighlightButton(Button_CommandUnit button)
        {
            if (button.type == UnitButtonCommand.Type.CancelTarget)
            {
                GameUI_Tooltip_CommandCard.Instance.OpenTooltip(button.gameObject);

                //HARD-CODED
                GameUI_Tooltip_CommandCard.Instance.Tooltip_Text($"{button.CommandButtonSO.displayName} (<color=white>{Key(11)}</color>)", $"{button.CommandButtonSO.tooltip}");
                return;
            }

            var commandCard = currentGameUnit.Class.commandCards.Find(x => x.cardName == currentCommandCard.cardName);
            if (commandCard == null) return;

            var command = commandCard.commands.Find(x => x.position == button.index);

            GameUI_Tooltip_CommandCard.Instance.OpenTooltip(button.gameObject);
            GameUI_Tooltip_CommandCard.Instance.Tooltip_Text($"{command.button.displayName} (<color=white>{Key(command.position)}</color>)", $"{command.button.tooltip}");

            if (command != null) 
            {               

                if (command.abilityType == UnitButtonCommand.AbilityOrder.BuildBuilding)
                {
                    var buildingSO = command.buildingSO;
                    if (buildingSO != null) 
                    {
                        if (buildingSO.MineralCost != 0) GameUI_Tooltip_CommandCard.Instance.Show_Minerals(buildingSO.MineralCost);
                        if (buildingSO.EnergyCost != 0) GameUI_Tooltip_CommandCard.Instance.Show_Gas(buildingSO.EnergyCost);
                        if (buildingSO.BuildTime != 0) GameUI_Tooltip_CommandCard.Instance.Show_Time(buildingSO.BuildTime);

                    }
                }

                if (command.abilityType == UnitButtonCommand.AbilityOrder.TrainUnit)
                {
                    var unitSO = command.unitSO;
                    if (unitSO != null)
                    {
                        if (unitSO.MineralCost != 0) GameUI_Tooltip_CommandCard.Instance.Show_Minerals(unitSO.MineralCost);
                        if (unitSO.EnergyCost != 0) GameUI_Tooltip_CommandCard.Instance.Show_Gas(unitSO.EnergyCost);
                        if (unitSO.SupplyCount != 0) GameUI_Tooltip_CommandCard.Instance.Show_Supply(unitSO.SupplyCount);
                        if (unitSO.BuildTime != 0) GameUI_Tooltip_CommandCard.Instance.Show_Time(unitSO.BuildTime);

                    }
                }
            }
        }

        public void DehighlightButton(Button_CommandUnit button)
        {
            GameUI_Tooltip_CommandCard.Instance.CloseTooltip();
        }



        private void Update()
		{
            if (currentCommandCard == null) return;
            if (currentGameUnit == null) return;

            if (Input.GetKeyUp(KeyCode.Q))
            {
                if (currentCommandCard.CheckHasCommandAtIndex(0) == true) { ClickButton(buttons[0]); }
            }
            if (Input.GetKeyUp(KeyCode.W))
            {
                if (currentCommandCard.CheckHasCommandAtIndex(1) == true) { ClickButton(buttons[1]); }
            }
            if (Input.GetKeyUp(KeyCode.E))
            {
                if (currentCommandCard.CheckHasCommandAtIndex(2) == true) { ClickButton(buttons[2]); }
            }
            if (Input.GetKeyUp(KeyCode.R))
            {
                if (currentCommandCard.CheckHasCommandAtIndex(3) == true) { ClickButton(buttons[3]); }
            }

            if (Input.GetKeyUp(KeyCode.A))
            {
                if (currentCommandCard.CheckHasCommandAtIndex(4) == true) { ClickButton(buttons[4]); }
            }
            if (Input.GetKeyUp(KeyCode.S))
            {
                if (currentCommandCard.CheckHasCommandAtIndex(5) == true) { ClickButton(buttons[5]); }
            }
            if (Input.GetKeyUp(KeyCode.D))
            {
                if (currentCommandCard.CheckHasCommandAtIndex(6) == true) { ClickButton(buttons[6]); }
            }
            if (Input.GetKeyUp(KeyCode.F))
            {
                if (currentCommandCard.CheckHasCommandAtIndex(7) == true) { ClickButton(buttons[7]); }
            }
        }
		
		private void OnEnable()
		{
			
		}

	
	}
}