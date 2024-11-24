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



		private static Selection _instance;

        private void Awake()
        {
			_instance = this;
        }

		private GameUnit _portraitedUnit
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

		internal static void SelectUnit(GameUnit unit)
		{
			if (_instance.allSelectedUnits.Contains(unit) == false)
			{
				_instance.allSelectedUnits.Add(unit);
			}


		}

	}
}