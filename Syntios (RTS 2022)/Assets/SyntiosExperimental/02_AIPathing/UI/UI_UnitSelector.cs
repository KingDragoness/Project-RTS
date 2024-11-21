using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using ToolBox.Pools;

namespace ProtoRTS
{
	public class UI_UnitSelector : MonoBehaviour
	{

        public GameObject panel;
        public Button_Unit prefab;
        public Button[] allGroupButtons;
        public Transform parentButton;
        public int maxUnitInGroup = 24;

        [HideInEditorMode] private List<Button_Unit> pooledButtons = new List<Button_Unit>();

        private void Awake()
        {
        }

        private void Start()
        {
            prefab.gameObject.Populate(24);

        }

        public void RefreshUI()
        {
            foreach (var button in pooledButtons)
            {


            }

            pooledButtons.ReleasePoolObject();

            int selectedIndex = -1;
            int index = 0;

            int count = RTSController.Instance.allSelectedUnits.Count; if (count > maxUnitInGroup) count = maxUnitInGroup;
            int totalButtonCount = 1 + Mathf.FloorToInt(RTSController.Instance.allSelectedUnits.Count / maxUnitInGroup);

            if (totalButtonCount > allGroupButtons.Length)
            {
                totalButtonCount = allGroupButtons.Length;
            }

            for (int x = 0; x < count; x++)
            {
                var unit = RTSController.Instance.allSelectedUnits[x];
                var button = prefab.gameObject.Reuse<Button_Unit>(); //Instantiate(prefab, parentButton);
                if (button.transform.parent != parentButton) button.transform.SetParent(parentButton);
                button.gameObject.SetActive(true);
                button.icon_Unit.sprite = unit.Class.spriteWireframe;
                button.transform.localPosition = Vector3.zero;
                button.transform.localScale = Vector3.one;
                index++;
                pooledButtons.Add(button);
            }

            foreach(var button in allGroupButtons)
            {
                button.gameObject.SetActive(false);
            }

            if (totalButtonCount > 1)
            {
                for (int z = 0; z < totalButtonCount; z++)
                {
                    allGroupButtons[z].gameObject.SetActive(true);
                }
            }

        }
    }
}