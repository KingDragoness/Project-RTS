using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using UnityEngine.PlayerLoop;
using System;
using static UnityEngine.UI.CanvasScaler;

namespace ProtoRTS.Game
{
	public class UI_UnitStats : MonoBehaviour
	{

		public GameObject panel;
		public Color color_labelBoldBlue;
        public Gradient gradientWireframeHP;
        [SerializeField] private Text label_UnitName;
		[SerializeField] private Text label_Rank;
		[SerializeField] private Text label_Hitpoint;
		[SerializeField] private Text label_Mana;
		[SerializeField] private Text label_Tags;
		[SerializeField] private Image image_Wireframe;

        private void OnEnable()
        {
            Tick.OnTick += TickRefreshUI;
        }


        private void OnDisable()
        {
            Tick.OnTick -= TickRefreshUI;
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
				label_Mana.text = $"200/200";
			}
            else
            {
				label_Mana.gameObject.SetActive(false);
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


    }
}