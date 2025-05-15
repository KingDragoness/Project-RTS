using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using UnityEngine.InputSystem;

namespace ProtoRTS
{
	public class Tooltip : MonoBehaviour
	{

        public Text label;
        public RectTransform rt;
		private GameObject taggedGO;

        private static Tooltip _instance;

        public static Tooltip Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<Tooltip>(true);
                }

                return _instance;
            }
        }

        private void Awake()
        {
            var i = Instance;
            HideTooltip();
        }

        public static void ShowTooltip(GameObject _attachedGO, string text)
		{
            Instance._showtooltip(_attachedGO, text);
        }

        private void _showtooltip(GameObject _attachedGO, string text)
        {
            _instance.gameObject.SetActive(true);
            _instance.taggedGO = _attachedGO;
            //_instance.rt.anchoredPosition = Input.mousePosition;
            _instance.label.text = text;
        }

        public static void HideTooltip()
        {
            Instance._hidetooltip();
        }

        private void _hidetooltip()
        {
            gameObject.SetActive(false);
            taggedGO = null;
        }

        private void Update()
        {
            if (taggedGO != null)
            {
                var mousePos = Input.mousePosition;
                Vector3 v3 = new Vector3();
                v3.x = Mathf.RoundToInt(mousePos.x) + 12;
                v3.y = Mathf.RoundToInt(mousePos.y) + 5;
                v3.z = Mathf.RoundToInt(mousePos.z);

                _instance.rt.anchoredPosition = v3;

                if (!taggedGO.activeInHierarchy)
                {
                    HideTooltip();
                }
            }


        }

    }
}