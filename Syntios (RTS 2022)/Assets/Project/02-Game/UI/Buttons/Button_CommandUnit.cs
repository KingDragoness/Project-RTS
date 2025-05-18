using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

namespace ProtoRTS.Game
{
	public class Button_CommandUnit : MonoBehaviour
	{

		public UI_AbilityCommand UI_RTSCommands;
		public Animator buttonAnim;
		public Image buttonIcon;
		public Image buttonActiveGlow;
        public Text label_Hotkey;
		public bool emptyButton = false;
		public UnitButtonCommand.Type type;
		public CommandButtonSO CommandButtonSO;
		public string buttonID = "";
        public int index = 0;

		public void OnClick()
		{
            UI_RTSCommands.ClickButton(this);
		}

		public void Highlight()
		{
            UI_RTSCommands.HighlightButton(this);

        }

        public void Dehighlight()
        {
            UI_RTSCommands.DehighlightButton(this);

        }

		public void CommandRunning()
		{
            buttonActiveGlow.gameObject.SetActive(true);

        }

		public void NotRunning()
		{
            buttonActiveGlow.gameObject.SetActive(false);

        }
    }
}