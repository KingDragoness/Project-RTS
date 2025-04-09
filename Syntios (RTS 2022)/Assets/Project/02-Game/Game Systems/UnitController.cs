using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ProtoRTS.Game
{
	public class UnitController : MonoBehaviour
	{

		public int limitUnitPerTick = 100;

		private int indexHeightClamping = 0;

        private void OnEnable()
        {
			Tick.OnTick += RefreshUnitModel;
        }

        private void OnDisable()
        {
			Tick.OnTick -= RefreshUnitModel;
		}

		private void Update()
		{

		}
		
		/// <summary>
		/// FOW
		/// </summary>
		private void RefreshUnitModel(int tick)
        {
			int index_0 = indexHeightClamping * limitUnitPerTick;
			int index_length = (indexHeightClamping + 1) * limitUnitPerTick;

			if (index_length >= SyntiosEngine.Instance.ListedGameUnits.Count)
			{
				index_length = SyntiosEngine.Instance.ListedGameUnits.Count;
			}

			for (int x = index_0; x < index_length; x++)
            {
				var unit = SyntiosEngine.Instance.ListedGameUnits[x];

                if (unit.stat_faction != SyntiosEngine.CurrentFaction)
                {
                    if (FOWScript.IsCoordRevealed(unit.transform.position))
                    {
                        unit.ShowModel();
                    }
                    else
                    {
                        unit.HideModel();
                    }
                }
                else
                {
					unit.ShowModel();
				}
			}

            indexHeightClamping++;
			if (index_length >= SyntiosEngine.Instance.ListedGameUnits.Count)
			{
				indexHeightClamping = 0;
			}


		}

	}
}