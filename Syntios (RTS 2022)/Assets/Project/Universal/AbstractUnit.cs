using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ProtoRTS
{

	//can be used both in game and map editor.
	public class AbstractUnit : MonoBehaviour
	{
		[SerializeField] internal SO_GameUnit _class; //temporary system


		[FoldoutGroup("Game Stats")] [SerializeField] internal int stat_KillCount;
		[FoldoutGroup("Game Stats")] [SerializeField] internal int stat_HP = 25;
		[FoldoutGroup("Game Stats")] [SerializeField] internal int stat_Energy = 0;
		[FoldoutGroup("Game Stats")] [SerializeField] internal Unit.Player stat_faction;

		public SO_GameUnit Class { get => _class; }

		private Transform circleOutline;

		public void SelectedUnit()
		{
			if (circleOutline != null) Destroy(circleOutline.gameObject);

			//consult "Force"
			var factionStatus = FactionAlliance.Instance.GetFactionStatus(SyntiosEngine.CurrentFaction, stat_faction);

            Transform prefab = null;
            prefab = Selection.GetCircle(factionStatus).transform;

            Transform t1 = Instantiate(prefab, transform, false);
			t1.transform.localPosition = Vector3.zero + new Vector3(0f, 0.2f, 0f);
			t1.transform.localScale = Vector3.one * _class.Radius * 2.1f;
			t1.gameObject.SetActive(true);
			circleOutline = t1;
		}

		public bool IsPlayerUnit()
		{
            var factionStatus = FactionAlliance.Instance.GetFactionStatus(SyntiosEngine.CurrentFaction, stat_faction);
			
			if (factionStatus == Unit.TypePlayer.Player) return true;

			return false;
        }

        public void DeselectUnit()
		{
			if (circleOutline != null) Destroy(circleOutline.gameObject);

		}

	}
}