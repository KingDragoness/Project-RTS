using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ProtoRTS
{
	public class UI_RTSCommandPanel : MonoBehaviour
	{

        private void Start()
        {
			RefreshUI();
        }

        public void RefreshUI()
		{
			if (RTSController.Instance.allSelectedUnits.Count == 1)
            {
				//display stats
				UI.UnitStats.panel.SetActive(true);
				UI.UnitSelection.panel.SetActive(false);
				UI.UnitStats.RefreshUI();
			}
            else
            {
				//show box selection units
				UI.UnitStats.panel.SetActive(false);
				UI.UnitSelection.panel.SetActive(true);
				UI.UnitSelection.RefreshUI();

			}
		}
	}
}