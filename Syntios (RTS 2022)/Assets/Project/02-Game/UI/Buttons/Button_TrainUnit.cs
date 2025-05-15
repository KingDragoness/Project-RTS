using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

namespace ProtoRTS.Game
{
	public class Button_TrainUnit : MonoBehaviour
	{
        public int index = 0;
        public Image train_slot;
        public Text train_numberLabel;
        public Button button_train;
        public UI_UnitStats uI_UnitStats;

        public void ClickButton()
        {
            uI_UnitStats.CancelTrain(index);
        }

        public void Highlight()
        {
            uI_UnitStats.ShowTooltip(index);
        }

        public void Dehighlight()
        {
            uI_UnitStats.HideTooltip(index);
        }
    }
}