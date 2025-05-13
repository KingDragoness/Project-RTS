using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

namespace ProtoRTS.Game
{
	public class GameHUD : MonoBehaviour
	{

		public UnitBar_HPSPMP_Element element_UnitBar;
        public RectTransform parentUnitBar;

        private List<UnitBar_HPSPMP_Element> pooled_UnitHPBar = new List<UnitBar_HPSPMP_Element>(); //pooled


        private void Awake()
        {
            SyntiosEvents.UI_ReselectUpdate += reselectUpdate;
            SyntiosEvents.UI_NewSelection += newSelection;

        }

        private void OnDestroy()
        {
            SyntiosEvents.UI_ReselectUpdate -= reselectUpdate;
            SyntiosEvents.UI_NewSelection -= newSelection;
        }

        private void newSelection(GameUnit unit)
        {
            foreach (var unitBar in pooled_UnitHPBar) { unitBar.attachedGameUnit = null; }

            Update_UnitBar();
        }

        private void reselectUpdate()
        {
            Update_UnitBar();
        }


        #region Unit Bar
        private void Update_UnitBar()
        {
            foreach (var selectedUnit in Selection.AllSelectedUnits)
            {
    
                if (IsUnitBarActive(selectedUnit)) continue;

                var unitBar = GetEmptyUnitBar(selectedUnit);
                unitBar.gameObject.SetActive(true);
                unitBar.InitializeElement(selectedUnit);
            }

            foreach (var unitBar in pooled_UnitHPBar)
            {
                if (!unitBar.gameObject.activeSelf) continue;
              

                if (Selection.AllSelectedUnits.Contains(unitBar.attachedGameUnit) == false)
                {
                    unitBar.gameObject.SetActive(false);
                }
            }
        }

        private bool IsUnitBarActive(GameUnit unit)
        {
            foreach (var unitBar in pooled_UnitHPBar)
            {
                if (unitBar.attachedGameUnit == unit && unitBar.gameObject.activeSelf)
                {
                    return true;
                }
            }

            return false;
        }

        private UnitBar_HPSPMP_Element GetEmptyUnitBar(GameUnit unit)
        {
            foreach (var unitBar in pooled_UnitHPBar)
            {
                if (!unitBar.gameObject.activeSelf)
                {
                    return unitBar;
                }
            }

            var newunitBar = CreateUnitBar(unit);
            pooled_UnitHPBar.Add(newunitBar);
            return newunitBar;
        }

        private UnitBar_HPSPMP_Element CreateUnitBar(GameUnit unit)
        {
            UnitBar_HPSPMP_Element newBar = Instantiate(element_UnitBar, parentUnitBar);
            newBar.attachedGameUnit = unit;
            newBar.gameObject.SetActive(true);
            return newBar;
        }

        #endregion
    }
}