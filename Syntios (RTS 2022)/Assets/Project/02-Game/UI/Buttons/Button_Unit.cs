using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

namespace ProtoRTS.Game
{
	public class Button_Unit : MonoBehaviour
	{

		public UI_UnitSelector unitSelector;
		public Image icon_Unit;
		public Button button;
		public GameUnit attachedGameUnit;

		/// <summary>
		/// Disables button to retain structure
		/// </summary>
		public void ClickButton()
        {
			//remove all non-similar units
			if (Input.GetKey(KeyCode.LeftControl))
			{
				unitSelector.DeselectNonSimilarUnits(this);

            }
			else
			{
                unitSelector.DeselectOneUnit(this);

            }
		}
	}
}