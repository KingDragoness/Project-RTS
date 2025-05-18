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

        private float timer = 0;

        private void Start()
        {

        }

        private void Update()
        {
            if (timer < 0)
            {
                ClosePrompt();
            }
            else
            {
                timer -= Time.deltaTime;
            }
        }

        public void OpenPrompt(string prompt, float time = 9999)
		{
			label_prompt.gameObject.SetActive(true);
            timer = time;
            label_prompt.text = prompt;
		}

        public void ClosePrompt()
        {
            label_prompt.gameObject.SetActive(false);
        }

    }
}