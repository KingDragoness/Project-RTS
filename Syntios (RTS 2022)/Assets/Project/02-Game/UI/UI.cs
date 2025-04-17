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
        [SerializeField] private UI_AbilityCommand _abilityCommandPanel;
        [SerializeField] private UI_UnitStats _unitStat;
        [SerializeField] private UI_PromptMiddle _promptHelp;
        [SerializeField] private GameUI_CommandMap _commandMap;
        [SerializeField] private BoxSelectionUnit _boxSelector;
        [SerializeField] private Canvas _canvas_MainUI;

        public static UI instance;

        private void Awake()
        {
            instance = this;
        }

        public static UI_UnitSelector UnitSelection { get { return instance._unitSelection; } }

        public static UI_RTSCommandPanel CommandPanel { get { return instance._commandPanel; } }
        public static UI_AbilityCommand AbilityUI { get { return instance._abilityCommandPanel; } }
        public static UI_UnitStats UnitStats { get { return instance._unitStat; } }
        public static UI_PromptMiddle PromptHelp { get { return instance._promptHelp; } }
        public static GameUI_CommandMap CommandMap { get { return instance._commandMap; } }
        public static BoxSelectionUnit BoxSelect { get { return instance._boxSelector; } }


        public static Canvas CanvasMainUI { get { return instance._canvas_MainUI; } }

    }
}