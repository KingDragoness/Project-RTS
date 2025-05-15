using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using UnityEngine.PlayerLoop;
using System;
using static UnityEngine.UI.CanvasScaler;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;
using Slider = UnityEngine.UI.Slider;
using Button = UnityEngine.UI.Button;
using UnityEngine.EventSystems;

namespace ProtoRTS.Game
{
	public class UI_UnitStats : MonoBehaviour
	{

		public GameObject panel;
		public Color color_labelBoldBlue;
        public Gradient gradientWireframeHP;
		public GameObject panel_UnitRanksUps;
        public GameObject panel_Training;
        [SerializeField] private Text label_UnitName;
		[SerializeField] private Text label_Rank;
		[SerializeField] private Text label_Hitpoint;
        [SerializeField] private Text label_Shield;
        [SerializeField] private Text label_Mana;
		[SerializeField] private Text label_Tags;
		[SerializeField] private Image image_Wireframe;
		[FoldoutGroup("Unit Training")] public List<Button_TrainUnit> buttons_TrainUnits;
        [FoldoutGroup("Unit Training")] public Slider slider_trainingTime;


        private void OnEnable()
        {
            Tick.OnTick += TickRefreshUI;
        }


        private void OnDisable()
        {
            Tick.OnTick -= TickRefreshUI;
        }

        private void Awake()
        {
			int index = 0;

            foreach(var button in buttons_TrainUnits)
			{
				button.index = index;
				index++;
            }
        }

        private void TickRefreshUI(int tick)
		{
			RefreshUI();

        }


        //constantly refresh in later update
        public void RefreshUI()
		{
			if (Selection.AllSelectedUnits.Count != 1) return;
			var unit = Selection.AllSelectedUnits[0]; if (unit == null) return;
			var SOunit = unit.Class;

			label_UnitName.text = SOunit.NameDisplay;
			label_Hitpoint.text = $"{unit.stat_HP}/{SOunit.MaxHP}";

            if (SOunit.HasEnergy)
            {
				label_Mana.gameObject.SetActive(true);
                label_Mana.text = $"{unit.stat_Energy}/{unit.Class.MaxMana()}";
            }
            else
            {
				label_Mana.gameObject.SetActive(false);
            }

            if (SOunit.HasShield)
            {
                label_Shield.gameObject.SetActive(true);
                label_Shield.text = $"{unit.stat_Shield}/{SOunit.MaxShield}";
            }
            else
            {
                label_Shield.gameObject.SetActive(false);
            }

            if (Selection.AllSelectedUnits.Count == 1 && unit.trainingQueue.Count > 0)
			{
				Update_TrainingUI(unit);
                panel_UnitRanksUps.gameObject.EnableGameobject(false);
                panel_Training.gameObject.EnableGameobject(true);
            }
			else
			{
                panel_UnitRanksUps.gameObject.EnableGameobject(true);
                panel_Training.gameObject.EnableGameobject(false);
            }

            float HP = (float)unit.stat_HP / (float)unit.Class.MaxHP;
			var colorHP = gradientWireframeHP.Evaluate(HP);
			image_Wireframe.sprite = SOunit.spriteWireframe;
			image_Wireframe.color = colorHP;
            label_Hitpoint.color = colorHP;	

            label_Tags.text = "";

			List<Unit.Tag> allTags = SOunit.AllUnitTags;
			allTags = allTags.OrderBy(x => x).ToList();

			int index = 0;

			foreach (var tag in allTags)
            {
				if (index >= allTags.Count - 1)
                {
					label_Tags.text += $"{tag}";
				}
				else
                {
					label_Tags.text += $"{tag} - ";
				}
				index++;
			}

            {
				string str_killCount = $"<color={color_labelBoldBlue.ToHex()}>Kills:</color> {unit.stat_KillCount}";
				string str_rank = $"<color={color_labelBoldBlue.ToHex()}>Rank:</color> {SOunit.Rank}";

				if (SOunit.Rank == "" | string.IsNullOrEmpty(SOunit.Rank))
                {
					str_rank = "";
					label_Rank.text = str_killCount;
                }
                else
                {
					label_Rank.text = str_killCount + "\n" + str_rank;
				}

			}
		}

		private void Update_TrainingUI(GameUnit unit)
		{
			var currentTrainUnit = unit.trainingQueue[0];
			var so_unitTraining = currentTrainUnit.gameUnitClass;

			slider_trainingTime.value = currentTrainUnit.timeTrained;
			slider_trainingTime.maxValue = so_unitTraining.BuildTime;

            for(int x = 0; x < buttons_TrainUnits.Count; x++)
			{
				var button = buttons_TrainUnits[x];
				bool isQueued = false;

				if (x < unit.trainingQueue.Count) isQueued = true;

				if (!isQueued)
				{
                    button.train_slot.gameObject.SetActive(false);
                    button.train_numberLabel.gameObject.SetActive(true);
                }
				else
				{
                    button.train_slot.gameObject.SetActive(true);
					button.train_slot.sprite = unit.trainingQueue[x].gameUnitClass.spriteWireframe;
                    button.train_numberLabel.gameObject.SetActive(false);
                }
          
            }

        }

		public void CancelTrain(int index)
		{
            var unit = Selection.AllSelectedUnits[0]; if (unit == null) return;
            var trainedUnit = unit.trainingQueue[index];

            unit.trainingQueue.RemoveAt(index);
            var myFactionSheet = SyntiosEngine.MyFactionSheet;
            myFactionSheet.Mineral += trainedUnit.gameUnitClass.MineralCost;
            myFactionSheet.Energy += trainedUnit.gameUnitClass.EnergyCost;
        }

        public void ShowTooltip(int index)
		{
            var unit = Selection.AllSelectedUnits[0]; if (unit == null) return;
            if (unit.trainingQueue.Count <= index) return;

			var trainedUnit = unit.trainingQueue[index];
            Tooltip.ShowTooltip(buttons_TrainUnits[index].train_slot.gameObject, $"Cancel {trainedUnit.gameUnitClass.NameDisplay}");
		}

		public void HideTooltip(int index)
		{
            var unit = Selection.AllSelectedUnits[0]; if (unit == null) return;
            if (unit.trainingQueue.Count <= index) return;

            Tooltip.HideTooltip();

        }

    }
}