using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Sirenix.OdinInspector;

namespace ProtoRTS.Game
{
	public class GameUI_CommandMap : MonoBehaviour, IPointerClickHandler
	{

        public CommandUnit commandUnit;
		public UI_Minimap minimap;

        public void OnPointerClick(PointerEventData eventData)
        {
            //if (eventData.button == PointerEventData.InputButton.Right)
            {
                var minimapPos = minimap.GetMousePosInMinimap();
                var worldPos = minimap.ConvertMinimapPosToWorldPos(minimapPos);
                RTS.instance.commandUnit.MinimapClickDrop(eventData, worldPos);
                //commandUnit.Old_MoveUnitsHere(worldPos);

                //if (Selection.GetPortraitedUnit != null)
                //{
                //    var unit = Selection.GetPortraitedUnit;

                //    SyntiosEvents.UI_OrderMove?.Invoke(unit);

                //}
            }
        }
    }
}