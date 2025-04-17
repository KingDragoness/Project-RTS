using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using Pathfinding;

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
		public LayerMask layer_Terrain;
		public GameObject circle_Green;
        public GameObject circle_Yellow;
        public GameObject circle_Red;
		[FoldoutGroup("Building")] [SerializeField] private GameObject highlight_3dModel;

		private bool isBuildPlacement = false;

        private static Selection _instance;

        private void Awake()
        {
			_instance = this;
        }

		private GameUnit _portraitedUnit
        {
            get
            {
				var highestOrder = allSelectedUnits.OrderByDescending(x => x._class.port_Importance).ToList();

				if (highestOrder.Count > 0)
                {
					//process unit order here
					return highestOrder[0];
                }
                else
                {
					return null;
                }
            }
        }

		public static GameUnit GetPortraitedUnit
        {
            get { return _instance._portraitedUnit; }
        }

        public static List<GameUnit> AllSelectedUnits { get => _instance.allSelectedUnits; }


		[Button("Get All Units")]
		public void GetAllUnits()
		{
			allSelectedUnits = FindObjectsOfType<GameUnit>().ToList();

		}



		private void OnEnable()
		{

		}
        internal static GameObject GetCircle(Unit.TypePlayer typePlayer)
        {
			if (typePlayer == Unit.TypePlayer.Player)
			{
				return _instance.circle_Green;
			}
			else if (typePlayer == Unit.TypePlayer.Neutral)
            {
                return _instance.circle_Yellow;
            }
			else
            {
                return _instance.circle_Red;
            }
        }

        internal static void DeselectAllUnits()
		{
            Debug.Log("test");

            foreach (var unit in _instance.allSelectedUnits)
			{
				if (unit == null) continue;
				unit.DeselectUnit();
			}

			_instance.allSelectedUnits.Clear();
			SyntiosEvents.UI_DeselectAll?.Invoke();

		}

		internal static void SelectUnit(GameUnit unit)
		{
			if (_instance.allSelectedUnits.Contains(unit) == false)
			{
				_instance.allSelectedUnits.Add(unit);
			}


		}

		//Also when dying, call this function.
		internal static void RemoveUnit(GameUnit unit)
        {
			if (_instance.allSelectedUnits.Contains(unit) == true)
			{
				_instance.allSelectedUnits.Remove(unit);
				unit.DeselectUnit();
			}
            else
            {
				_instance.allSelectedUnits.RemoveAll(x => x == null);
            }
		}

		public Vector3 GetCursorPositionTerrain()
		{
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 512f, layer_Terrain))
            {
				return hit.point;
            }

            if (Physics.Raycast(ray, out hit, 512f))
            {
                return hit.point;
            }

            return Vector3.zero;
        }


		[FoldoutGroup("DEBUG")]
		[SerializeField] private SO_GameUnit DEBUG_buildingPlacement1;

        [FoldoutGroup("DEBUG")]
        [Button("BuildPlacement")]
		public void DEBUG_PlacementBuilding1()
		{
			BuildPlacement(DEBUG_buildingPlacement1);
		}

        [FoldoutGroup("DEBUG")]
		public void BuildPlacement(SO_GameUnit buildingSO)
		{
			if (highlight_3dModel != null) { Destroy(highlight_3dModel.gameObject); }

			GameObject go_New = new GameObject($"Highlight_{buildingSO.NameDisplay}");
			
			foreach(var model in buildingSO.basePrefab.modelView)
			{
				var new_model = Instantiate(model.gameObject);
				var navmeshCut = new_model.GetComponent<NavmeshCut>();
				if (navmeshCut != null) Destroy(navmeshCut);

                new_model.transform.SetParent(go_New.transform);
			}

			Vector3 cursorPos = GetCursorPositionTerrain();
			Vector3 placePos = Vector3.zero;
			placePos.x = Mathf.RoundToInt(cursorPos.x);
			placePos.y = Map.instance.GetPositionY_cliffLevel(cursorPos);
            placePos.z = Mathf.RoundToInt(cursorPos.z);
            go_New.transform.position = placePos;
			highlight_3dModel = go_New;
            isBuildPlacement = true;
        }

        [FoldoutGroup("DEBUG")]
        [Button("CancelPlacement")]
        public void CancelPlacement()
        {
            if (highlight_3dModel != null) { Destroy(highlight_3dModel.gameObject); }

            isBuildPlacement = false;
        }

        [FoldoutGroup("DEBUG")]
        [Button("Build here")]
        public void BuildBuilding()
		{
			var newUnit = Instantiate(DEBUG_buildingPlacement1.basePrefab);
			newUnit.transform.position = highlight_3dModel.transform.position;

        }

        private void Update()
        {
            if (isBuildPlacement)
			{
				if (highlight_3dModel != null)
				{
                    Vector3 cursorPos = GetCursorPositionTerrain();
                    Vector3 placePos = Vector3.zero;
                    placePos.x = Mathf.RoundToInt(cursorPos.x/2)*2;
                    placePos.y = Map.instance.GetPositionY_cliffLevel(cursorPos);
                    placePos.z = Mathf.RoundToInt(cursorPos.z/2)*2;
                    highlight_3dModel.transform.position = placePos;

                }
            }
        }
    }
}