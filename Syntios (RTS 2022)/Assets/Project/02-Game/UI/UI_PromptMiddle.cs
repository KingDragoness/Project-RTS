using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

namespace ProtoRTS.Game
{
	public class UI_PromptMiddle : MonoBehaviour
	{

		public Text label_prompt;


        private void Start()
        {
            label_prompt.gameObject.SetActive(false);
        }


        public void OpenPrompt(string prompt)
		{
			label_prompt.gameObject.SetActive(true);
			label_prompt.text = prompt;
		}

        public void ClosePrompt()
        {
            label_prompt.gameObject.SetActive(false);
        }

    }
}