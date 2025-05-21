using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using static UnityEngine.UI.CanvasScaler;
using System.Xml;
using System.Linq;

namespace ProtoRTS.Game
{

	[System.Serializable]
	public class GhostModelSaveData
	{
		public Vector3 position;
		public int guid = 0;
	}

	public class UnitController : MonoBehaviour
	{

		public List<Ghost3dModel> ghostModels;
		public int limitUnitPerTick = 100;

		private int indexHeightClamping = 0;

        private void OnEnable()
        {
			Tick.OnTick += RefreshUnitModel;
			Tick.OnTick += RefreshGhostModel;
        }

        private void OnDisable()
        {
			Tick.OnTick -= RefreshUnitModel;
            Tick.OnTick -= RefreshGhostModel;
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
					bool isVisible = false;

					isVisible = FOWScript.IsCoordRevealed(unit.transform.position, SyntiosEngine.CurrentFaction);

					//second check for radius
					if (!isVisible)
					{
						bool dir1 = FOWScript.IsCoordRevealed(unit.transform.position + new Vector3(unit.Class.Radius, 0, unit.Class.Radius), SyntiosEngine.CurrentFaction);
                        bool dir2 = FOWScript.IsCoordRevealed(unit.transform.position + new Vector3(unit.Class.Radius, 0, -unit.Class.Radius), SyntiosEngine.CurrentFaction);
                        bool dir3 = FOWScript.IsCoordRevealed(unit.transform.position + new Vector3(unit.Class.Radius, 0, -unit.Class.Radius), SyntiosEngine.CurrentFaction);
                        bool dir4 = FOWScript.IsCoordRevealed(unit.transform.position + new Vector3(-unit.Class.Radius, 0, -unit.Class.Radius), SyntiosEngine.CurrentFaction);

						if (dir1 |  dir2 | dir3 | dir4)
						{
							isVisible = true;
						}
                    }


                    if (isVisible)
                    {
                        unit.ShowModel();
                        
                    }
                    else
                    {
						//if there is ghost model, spawns
						if (unit.Class.ghost3dModelPrefab != null && unit.IsVisible_1_Tick_ago)
						{
							//create ghost model
							CreateGhostModel(unit);

                        }
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

		private void RefreshGhostModel(int tick)
        {
			for (int i = 0; i < ghostModels.Count; i++)
			{
				var ghostModel = ghostModels[i];

                if (ghostModel == null) continue;
				//if (ghostModel.attachedUnit == null)
				//{
				//    DeleteGhostModel(ghostModel);
				//    continue;
				//}

				if (ghostModel.attachedUnit != null)
				{
					if (ghostModel.attachedUnit.IsVisibleFromFOW)
					{
						DeleteGhostModel(ghostModel);
						continue;
					}
				}
				else
				{
                    bool isVisible = false;

                    isVisible = FOWScript.IsCoordRevealed(ghostModel.transform.position, SyntiosEngine.CurrentFaction);

					//no need for high accuracy for revealing killed unit
					if (false)
					{
                        bool dir1 = FOWScript.IsCoordRevealed(ghostModel.transform.position + new Vector3(ghostModel.attachedUnitSO.Radius, 0, ghostModel.attachedUnitSO.Radius), SyntiosEngine.CurrentFaction);
                        bool dir2 = FOWScript.IsCoordRevealed(ghostModel.transform.position + new Vector3(ghostModel.attachedUnitSO.Radius, 0, -ghostModel.attachedUnitSO.Radius), SyntiosEngine.CurrentFaction);
                        bool dir3 = FOWScript.IsCoordRevealed(ghostModel.transform.position + new Vector3(ghostModel.attachedUnitSO.Radius, 0, -ghostModel.attachedUnitSO.Radius), SyntiosEngine.CurrentFaction);
                        bool dir4 = FOWScript.IsCoordRevealed(ghostModel.transform.position + new Vector3(-ghostModel.attachedUnitSO.Radius, 0, -ghostModel.attachedUnitSO.Radius), SyntiosEngine.CurrentFaction);

                        if (dir1 | dir2 | dir3 | dir4)
                        {
                            isVisible = true;
                        }
                    }

                    if (isVisible)
                    {
                        DeleteGhostModel(ghostModel);
                        continue;
                    }
                }

            }


            //ghostModels.RemoveAll(x => x == null);
			//allGhosted3dModels = allGhosted3dModels.Where(x => x.Key != null).ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        private void CreateGhostModel(GameUnit unit)
		{
            if (unit.Class.ghost3dModelPrefab == null)
            {
				return;
            }
			if (AlreadyExistGhost3d(unit))
			{
				return;
			}


            var newModel = Instantiate(unit.Class.ghost3dModelPrefab, unit.transform);
			newModel.attachedUnit = unit;
			newModel.attachedUnitSO = unit.Class;
            newModel.gameObject.SetActive(true);
			newModel.transform.SetParent(transform);
            newModel.transform.position = unit.transform.position;
            allGhosted3dModels.TryAdd(unit, newModel);
            ghostModels.Add(newModel);

        }

		private void DeleteGhostModel(Ghost3dModel ghost3DModel)
		{
			ghostModels.Remove(ghost3DModel);
			allGhosted3dModels.Remove(ghost3DModel.attachedUnit);
            Destroy(ghost3DModel.gameObject);

        }

		private Dictionary<GameUnit, Ghost3dModel> allGhosted3dModels = new Dictionary<GameUnit, Ghost3dModel> ();

		private bool AlreadyExistGhost3d(GameUnit unit)
		{
			if (allGhosted3dModels.ContainsKey(unit))
			{
				return true;
			}

			return false;
		}

	}
}