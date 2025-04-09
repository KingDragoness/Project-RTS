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
            SyntiosEvents.UI_DeselectAll += event_UI_DeselectAll;

        }


        private void OnDestroy()
        {
            SyntiosEvents.UI_NewSelection -= event_UI_NewSelection;
            SyntiosEvents.UI_DeselectAll -= event_UI_DeselectAll;
        }

        private void event_UI_DeselectAll()
        {
            foreach (var button in buttons) { button.gameObject.SetActive(false); }
        }

        private void event_UI_NewSelection(GameUnit unit)
        {
			foreach (var button in buttons) { button.gameObject.SetActive(false); }
            if (unit.stat_faction != SyntiosEngine.CurrentFaction) return;
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


        private void Update()
		{
			
		}
		
		private void OnEnable()
		{
			
		}

	
	}
}