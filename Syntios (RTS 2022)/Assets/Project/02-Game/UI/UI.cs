using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

namespace ProtoRTS.Game
{
	public class UI : MonoBehaviour
	{

		[SerializeField] private UI_UnitSelector _unitSelection;
        [SerializeField] private UI_RTSCommandPanel _commandPanel;
        [SerializeField] private UI_UnitStats _unitStat;
        [SerializeField] private Canvas _canvas_MainUI;

        public static UI instance;

        private void Awake()
        {
            instance = this;
        }

        public static UI_UnitSelector UnitSelection { get { return instance._unitSelection; } }

        public static UI_RTSCommandPanel CommandPanel { get { return instance._commandPanel; } }
        public static UI_UnitStats UnitStats { get { return instance._unitStat; } }

        public static Canvas CanvasMainUI { get { return instance._canvas_MainUI; } }

    }
}