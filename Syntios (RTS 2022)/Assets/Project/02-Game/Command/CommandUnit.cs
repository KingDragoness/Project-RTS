using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

namespace ProtoRTS.Game
{
	public class CommandUnit : MonoBehaviour
	{

		public bool DEBUG_DrawCircle = false;

		private Formation[] allFormations = new Formation[1];
		private bool input_rightButtonDown = false;



		private void Update()
		{
			input_rightButtonDown = false;

			//block if there is UI

			if (Input.GetMouseButtonDown(1))
			{
				if (MainUI.GetEventSystemRaycastResults().Count > 0)
				{
				}
                else
                {
					Handle_RightClick_OrderMove();
				}
			}

			if (DEBUG_DrawCircle) DEBUG_DrawFormation();

		}


		private void FixedUpdate()
		{

			if (input_rightButtonDown)
			{
			}


		}

		public void MoveUnitsHere(Vector3 target)
		{
			int index = 0;
			int count = Selection.AllSelectedUnits.Count;
			int rows_col = Mathf.RoundToInt(Mathf.Sqrt(count));
			float offset = rows_col / 2f;

			allFormations = new Formation[count];

			foreach (var unit in Selection.AllSelectedUnits)
			{
				var gameUnit = unit.GetComponent<GameUnit>();
				int curr_column = Mathf.FloorToInt(index / rows_col);
				int curr_row = (index % rows_col);
				Vector3 positionTarget = target;

				positionTarget.x -= offset * unit.Class.Radius * 2f;
                positionTarget.z -= offset * unit.Class.Radius * 2f;

                positionTarget.x += curr_row * unit.Class.Radius * 2f;
                positionTarget.z += curr_column * unit.Class.Radius * 2f;


                Formation f = new Formation(unit.Class.Radius, positionTarget);
                allFormations[index] = f;

				#region Unused
				//if (unit.followerEntity != null)
				//{
				//	positionTarget.x -= offset * unit.Class.Radius * 2f;
				//	positionTarget.z -= offset * unit.followerEntity.radius * 2f;

				//	positionTarget.x += curr_row * unit.followerEntity.radius * 2f;
				//	positionTarget.z += curr_column * unit.followerEntity.radius * 2f;


				//	Formation f = new Formation(unit.followerEntity.radius, positionTarget);
				//	allFormations[index] = f;
				//}
				//if (unit.ai != null)
				//{
				//	positionTarget.x -= offset * unit.ai.radius * 2f;
				//	positionTarget.z -= offset * unit.ai.radius * 2f;

				//	positionTarget.x += curr_row * unit.ai.radius * 2f;
				//	positionTarget.z += curr_column * unit.ai.radius * 2f;


				//	Formation f = new Formation(unit.ai.radius, positionTarget);
				//	allFormations[index] = f;
				//}
				#endregion

				gameUnit.target = positionTarget;

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

		private void Handle_RightClick_OrderMove()
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;

			if (Physics.Raycast(ray, out hit, 2048f))
			{
				//t_BallTarget.transform.position = hit.point;
			}

			MoveUnitsHere(hit.point);

			if (Selection.GetPortraitedUnit != null)
			{
				var unit = Selection.GetPortraitedUnit;

				SyntiosEvents.UI_OrderMove?.Invoke(unit);

			}
		}
	}
}