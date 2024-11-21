using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using System;

namespace ProtoRTS
{
	[Serializable]
	public struct Formation
    {
		public float radius;
		public Vector3 position;

        public Formation(float radius, Vector3 position)
        {
            this.radius = radius;
            this.position = position;
        }
    }

	public class RTSController : MonoBehaviour
	{

        [SerializeField] private Vector2 mapSize = new Vector2(256, 256);
        public List<GameUnit> allSelectedUnits = new List<GameUnit>();
		public Text label_Unit;
		public Transform t_BallTarget;
		public bool DEBUG_DrawCircle = false;


		private Formation[] allFormations = new Formation[1];

		public static RTSController Instance;

        private void Awake()
        {
			Instance = this;
        }

        public void MoveUnitsHere(Vector3 target)
        {
			int index = 0;
			int count = allSelectedUnits.Count;
			int rows_col = Mathf.RoundToInt(Mathf.Sqrt(count));
			float offset = rows_col / 2f;

			allFormations = new Formation[count];

			foreach (var unit in allSelectedUnits)
            {
				int curr_column = Mathf.FloorToInt(index / rows_col);
				int curr_row = (index % rows_col);
				Vector3 positionTarget = target;

				if (unit.followerEntity != null)
				{
					positionTarget.x -= offset * unit.followerEntity.radius * 2f;
					positionTarget.z -= offset * unit.followerEntity.radius * 2f;

					positionTarget.x += curr_row * unit.followerEntity.radius * 2f;
					positionTarget.z += curr_column * unit.followerEntity.radius * 2f;


					Formation f = new Formation(unit.followerEntity.radius, positionTarget);
					allFormations[index] = f;
				}
				if (unit.ai != null)
                {
					positionTarget.x -= offset * unit.ai.radius * 2f;
					positionTarget.z -= offset * unit.ai.radius * 2f;

					positionTarget.x += curr_row * unit.ai.radius * 2f;
					positionTarget.z += curr_column * unit.ai.radius * 2f;


					Formation f = new Formation(unit.ai.radius, positionTarget);
					allFormations[index] = f;
				}
				unit.target = positionTarget;

				index++;
            }
        }

		private void DEBUG_DrawFormation()
        {
			foreach(var unitPos in allFormations)
            {
				Debug.DrawCircle(unitPos.position, unitPos.radius, Color.red, Vector3.up, 32);
            }
        }

		[Button("Get All Units")]
		public void GetAllUnits()
        {
			allSelectedUnits = FindObjectsOfType<GameUnit>().ToList();

		}

		private void Start()
		{
			
		}

		private bool input_rightButtonDown = false;

        public Vector2 MapSize { get => mapSize; set => mapSize = value; }

		public static Vector3 MapSize1
        {
			get
            {
				return Instance.mapSize;
            }
        }

        private void Update()
		{
			input_rightButtonDown = false;
			label_Unit.text = $"Units selected: {allSelectedUnits.Count}";

			if (Input.GetMouseButtonDown(1))
			{
				Handle_RightClick_OrderMove();
			}

			if (DEBUG_DrawCircle) DEBUG_DrawFormation();

		}


        private void FixedUpdate()
        {

			if (input_rightButtonDown)
			{
			}


		}

		private void Handle_RightClick_OrderMove()
        {

			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;

			if (Physics.Raycast(ray, out hit, 2048f))
			{

				t_BallTarget.transform.position = hit.point;
			}

			MoveUnitsHere(hit.point);
		}

        private void OnEnable()
		{
			
		}

		internal void DeselectAllUnit()
        {
			foreach(var unit in allSelectedUnits)
            {
				if (unit == null) continue;
				unit.DeselectUnit();
            }
			allSelectedUnits.Clear();
			UI.CommandPanel.RefreshUI();
		}

		internal void DragSelect(GameUnit unit)
        {
            if (allSelectedUnits.Contains(unit) == false)
            {
				allSelectedUnits.Add(unit);
            }


		}


    }
}