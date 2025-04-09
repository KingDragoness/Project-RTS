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

        private int last_mineralValue = 0;
        private int last_energyValue = 0;

        private void OnEnable()
        {
            Tick.OnTick += UpdateUI;
        }

        private void OnDisable()
        {
            Tick.OnTick -= UpdateUI;
        }

        private void UpdateUI(int tick)
        {
            if (tick % 2 == 0) return;

            var myFactionSheet = SyntiosEngine.MyFactionSheet;


            label_Supply.text = myFactionSheet.Supply.ToString();

        }

        private void Update()
        {
            var myFactionSheet = SyntiosEngine.MyFactionSheet;

            last_mineralValue = (int)Mathf.MoveTowards(last_mineralValue, myFactionSheet.Mineral, Time.deltaTime * 271);
            last_energyValue = (int)Mathf.MoveTowards(last_energyValue, myFactionSheet.Energy, Time.deltaTime * 271);

            label_Mineral.text = last_mineralValue.ToString();
            label_Energy.text = last_energyValue.ToString();
        }

    }
}