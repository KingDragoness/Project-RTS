using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using SuperSystems.UnityTools;

namespace ProtoRTS
{

	//can be used both in game and map editor.
	public class AbstractUnit : MonoBehaviour
	{
		[SerializeField] internal SO_GameUnit _class; //temporary system


		[FoldoutGroup("Game Stats")] [SerializeField] internal int stat_KillCount;
		[FoldoutGroup("Game Stats")] [SerializeField] internal int stat_HP = 25;
		[FoldoutGroup("Game Stats")] [SerializeField] internal int stat_Energy = 0;
        [FoldoutGroup("Game Stats")] [SerializeField] internal int stat_Shield = 0;
        [FoldoutGroup("Game Stats")] [SerializeField] internal bool stat_isHallucination = false;
        [FoldoutGroup("Game Stats")] [SerializeField] internal bool stat_isCloaking = false; //IF unit is not permanently cloaked
        [FoldoutGroup("Game Stats")] [SerializeField] internal Unit.Player stat_faction;

		public SO_GameUnit Class { get => _class; }

		private MeshRenderer circleOutline_Selected;
        private MeshRenderer circleOutline_Highlight;
		private Unit.TypePlayer tp_select;
		private Unit.TypePlayer tp_highlight;

        private bool isSelected = false;
        private bool isHighlight = false;

        public virtual void Awake()
		{
            //initialize circle outline
            {
                var selectedCircle = Instantiate(Selection.SelectedCirclePrefab, transform, false);
                selectedCircle.transform.localPosition = Vector3.zero + new Vector3(0f, 0.2f, 0f);
                selectedCircle.transform.localScale = Vector3.one * _class.Radius * 2.1f;
                selectedCircle.gameObject.SetActive(false);

                var autorotateScript = selectedCircle.GetComponent<AutoRotate>();
                circleOutline_Selected = selectedCircle.GetComponent<MeshRenderer>();
                autorotateScript.speed.y = 30f / Class.Radius;
            }

            {
                var highlightedCircle = Instantiate(Selection.SelectedCirclePrefab, transform, false);
                highlightedCircle.transform.localPosition = Vector3.zero + new Vector3(0f, 0.2f, 0f);
                highlightedCircle.transform.localScale = (Vector3.one * _class.Radius * 2f) + (Vector3.one * 0.5f) + (Vector3.one * _class.Radius * 0.25f);
                highlightedCircle.gameObject.SetActive(false);

                var autorotateScript = highlightedCircle.GetComponent<AutoRotate>();
                circleOutline_Highlight = highlightedCircle.GetComponent<MeshRenderer>();
                autorotateScript.speed.y = 30f / Class.Radius;
            }
        }


		
        public void SelectedUnit()
		{
			var factionStatus = FactionAlliance.Instance.GetFactionStatus(SyntiosEngine.CurrentFaction, stat_faction);
            if (factionStatus == tp_select && isSelected) return;


            if (!circleOutline_Selected.gameObject.activeSelf) circleOutline_Selected.gameObject.SetActive(true);

            var matCircle = Selection.GetCircleMaterial(factionStatus, _class.Radius, true);
            circleOutline_Selected.material = matCircle;

			isSelected = true;
			tp_select = factionStatus;
        }

		public void HighlightUnit()
		{
            var factionStatus = FactionAlliance.Instance.GetFactionStatus(SyntiosEngine.CurrentFaction, stat_faction);
            if (factionStatus == tp_highlight && isHighlight) return;


            if (!circleOutline_Highlight.gameObject.activeSelf) circleOutline_Highlight.gameObject.SetActive(true);

            var matCircle = Selection.GetCircleMaterial(factionStatus, _class.Radius, false);
            circleOutline_Highlight.material = matCircle;

            isHighlight = true;
            tp_highlight = factionStatus;
        }

        public void DehighlightUnit()
		{
            if (circleOutline_Highlight.gameObject.activeSelf) circleOutline_Highlight.gameObject.SetActive(false);

			if (isSelected)
			{
				SelectedUnit();
            }

            isHighlight = false;
        }

        public bool IsPlayerUnit()
		{
            var factionStatus = FactionAlliance.Instance.GetFactionStatus(SyntiosEngine.CurrentFaction, stat_faction);
			
			if (factionStatus == Unit.TypePlayer.Player) return true;

			return false;
        }

        public void DeselectUnit()
		{
            if (circleOutline_Selected.gameObject.activeSelf) circleOutline_Selected.gameObject.SetActive(false);
            isSelected = false;

        }

    }
}