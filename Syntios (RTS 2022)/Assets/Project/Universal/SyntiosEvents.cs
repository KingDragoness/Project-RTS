using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ProtoRTS
{
	public class SyntiosEvents : MonoBehaviour
	{

        public static System.Action<GameUnit> UI_NewSelection;
        public static System.Action UI_DeselectAll;
        public static System.Action<GameUnit> UI_OrderMove;



        private void OnEnable()
        {

        }

        private void OnDisable()
        {
            UI_NewSelection = null;
            UI_DeselectAll = null;
            UI_OrderMove = null;
        }

        private void Awake()
        {
           
        }

      

	}
}