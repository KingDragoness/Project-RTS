using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

namespace ProtoRTS.Game
{
	public class UI_ResourceStat : MonoBehaviour
	{

        public Text label_Mineral;
        public Text label_Energy;
        public Text label_Supply;


        private void OnEnable()
        {
            Tick.OnTick += UpdateUI;
        }

        private void OnDisable()
        {
            Tick.OnTick -= UpdateUI;
        }

        private void UpdateUI()
        {
            var myFactionSheet = SyntiosEngine.MyFactionSheet;

            label_Mineral.text = myFactionSheet.Mineral.ToString();
            label_Supply.text = myFactionSheet.Supply.ToString();

        }

    }
}