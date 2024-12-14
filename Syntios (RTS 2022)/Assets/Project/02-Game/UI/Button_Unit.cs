using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

namespace ProtoRTS.Game
{
	public class Button_Unit : MonoBehaviour
	{

		public Image icon_Unit;
		public Button button;
		public GameUnit attachedGameUnit;

		/// <summary>
		/// Disables button to retain structure
		/// </summary>
		public void DeselectUnit()
        {
			Selection.RemoveUnit(attachedGameUnit);
			gameObject.SetActive(false);

			if (Selection.GetPortraitedUnit != null)
			{
				var unit1 = Selection.GetPortraitedUnit;
				SyntiosEvents.UI_NewSelection?.Invoke(unit1);
			}
		}
	}
}