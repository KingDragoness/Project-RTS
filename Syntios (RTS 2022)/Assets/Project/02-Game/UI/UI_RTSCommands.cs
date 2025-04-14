using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

namespace ProtoRTS.Game
{
	public class UI_RTSCommands : MonoBehaviour
	{

		public List<Button_CommandUnit> buttons;
        public RuntimeAnimatorController anim_White;
        public RuntimeAnimatorController anim_Blue;

        private GameUnit currentGameUnit;
        private string currentCommandCard;


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
            foreach (var button in buttons) { button.gameObject.SetActive(false); }
        }


        private void event_UI_ReselectUpdate()
        {
            if (Selection.AllSelectedUnits.Find(x => x == currentGameUnit) == null) 
            {
                foreach (var button in buttons) { button.gameObject.SetActive(false); }
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

            currentCommandCard = commandCard.cardName;
            currentGameUnit = unit;


            foreach (var command in commandCard.commands)
			{
				buttons[command.position].gameObject.SetActive(true);
				buttons[command.position].buttonIcon.sprite = command.button.sprite;
                buttons[command.position].label_Hotkey.text = Key(command.position);
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
            var commandCard = currentGameUnit.Class.commandCards.Find(x => x.cardName == currentCommandCard);

            if (commandCard == null) return;

            var command = commandCard.commands.Find(x => x.position == button.index);

            if (command.commandType == UnitOrder.Type.Submenu)
            {
                currentCommandCard = command.cardToOpen;
                var targetedCC = currentGameUnit.Class.commandCards.Find(x => x.cardName == currentCommandCard);

                RefreshCommandCard(targetedCC);
            }
        }

        public void HighlightButton(Button_CommandUnit button)
        {
            var commandCard = currentGameUnit.Class.commandCards.Find(x => x.cardName == currentCommandCard);
            if (commandCard == null) return;

            var command = commandCard.commands.Find(x => x.position == button.index);
            var abilityScript = currentGameUnit.Class.unitAbility.Find(x => x.name == command.abilityScriptName);

            GameUI_Tooltip_CommandCard.Instance.OpenTooltip(button.gameObject);
            GameUI_Tooltip_CommandCard.Instance.Tooltip_Text($"{command.button.displayName} (<color=white>{Key(command.position)}</color>)", $"{command.button.tooltip}");

            if (abilityScript != null) 
            {
                var action_placeBuilding = abilityScript.allActions.Find(x => x.type == UnitAbility.ActionType.PlaceBuilding);
                var action_queueUnit = abilityScript.allActions.Find(x => x.type == UnitAbility.ActionType.QueueUnit);

                if (action_placeBuilding != null)
                {
                    var buildingSO = action_placeBuilding.buildingSO;
                    if (buildingSO != null) 
                    {
                        if (buildingSO.MineralCost != 0) GameUI_Tooltip_CommandCard.Instance.Show_Minerals(buildingSO.MineralCost);
                        if (buildingSO.EnergyCost != 0) GameUI_Tooltip_CommandCard.Instance.Show_Gas(buildingSO.EnergyCost);
                        if (buildingSO.BuildTime != 0) GameUI_Tooltip_CommandCard.Instance.Show_Time(buildingSO.BuildTime);

                    }
                }

                if (action_queueUnit != null)
                {
                    var unitSO = action_queueUnit.gameunitSO;
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
			
		}
		
		private void OnEnable()
		{
			
		}

	
	}
}