using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

namespace ProtoRTS
{
	[System.Serializable]
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


	public class Selection : MonoBehaviour
	{

		private List<GameUnit> allSelectedUnits = new List<GameUnit>();
		public bool DEBUG_DrawCircle = false;

		private Formation[] allFormations = new Formation[1];
		private bool input_rightButtonDown = false;


		private static Selection _instance;

        private void Awake()
        {
			_instance = this;
        }

        public GameUnit PortraitedUnit
        {
            get
            {
				if (allSelectedUnits.Count > 0)
                {
					//process unit order here
					return allSelectedUnits[0];
                }
                else
                {
					return null;
                }
            }
        }

        public static List<GameUnit> AllSelectedUnits { get => _instance.allSelectedUnits; }

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
			foreach (var unitPos in allFormations)
			{
				Debug.DrawCircle(unitPos.position, unitPos.radius, Color.red, Vector3.up, 32);
			}
		}

		[Button("Get All Units")]
		public void GetAllUnits()
		{
			allSelectedUnits = FindObjectsOfType<GameUnit>().ToList();

		}


		private void Update()
		{
			input_rightButtonDown = false;

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
				//t_BallTarget.transform.position = hit.point;
			}

			MoveUnitsHere(hit.point);

			if (PortraitedUnit != null)
            {
				var unit = PortraitedUnit;
				SyntiosEvents.UI_OrderMove?.Invoke(unit);

			}
		}

		private void OnEnable()
		{

		}

		internal static void DeselectAllUnits()
		{
			foreach (var unit in _instance.allSelectedUnits)
			{
				if (unit == null) continue;
				unit.DeselectUnit();
			}

			_instance.allSelectedUnits.Clear();
			SyntiosEvents.UI_DeselectAll?.Invoke();

		}

		internal static void DragSelect(GameUnit unit)
		{
			if (_instance.allSelectedUnits.Contains(unit) == false)
			{
				_instance.allSelectedUnits.Add(unit);
			}

			if (_instance.PortraitedUnit != null)
			{
				var unit1 = _instance.PortraitedUnit;

				SyntiosEvents.UI_NewSelection?.Invoke(unit1);
	
			}

		}

	}
}