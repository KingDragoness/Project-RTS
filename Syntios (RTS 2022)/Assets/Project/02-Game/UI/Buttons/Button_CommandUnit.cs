using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

namespace ProtoRTS.Game
{
	public class Button_CommandUnit : MonoBehaviour
	{

		public UI_RTSCommands UI_RTSCommands;
		public Animator buttonAnim;
		public Image buttonIcon;
		public Text label_Hotkey;
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
    }
}