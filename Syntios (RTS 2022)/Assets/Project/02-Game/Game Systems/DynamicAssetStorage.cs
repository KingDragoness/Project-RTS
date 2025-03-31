using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UIElements;

namespace ProtoRTS
{

	[System.Serializable]
	public class MaterialGameUnit
	{
        public Material mat_factionNeutral;
        public Material mat_faction1;
		public Material mat_faction2;
		public Material mat_faction3;
		public Material mat_faction4;
		public Material mat_faction5;
		public Material mat_faction6;
		public Material mat_faction7;
		public Material mat_faction8;
		public string unitID;
		public string materialID;

        public Material GetMaterial(Unit.Player faction)
        {
            if (faction == Unit.Player.Player1) return mat_faction1;
            if (faction == Unit.Player.Player2) return mat_faction2;
            if (faction == Unit.Player.Player3) return mat_faction3;
            if (faction == Unit.Player.Player4) return mat_faction4;
            if (faction == Unit.Player.Player5) return mat_faction5;
            if (faction == Unit.Player.Player6) return mat_faction6;
            if (faction == Unit.Player.Player7) return mat_faction7;
            if (faction == Unit.Player.Player8) return mat_faction8;

            return mat_factionNeutral;
        }
	}


	public class DynamicAssetStorage : MonoBehaviour
	{

		public Shader gameUnitShader;
		public List<string> registered_CustomMat_GameUnitIDs = new List<string>(); //Game unit's ID
		public List<MaterialGameUnit> allMaterialGameUnit = new List<MaterialGameUnit>();

        private static DynamicAssetStorage _instance;
		public static DynamicAssetStorage Instance { get { return _instance; } }

        private void Awake()
        {
            _instance = this;
        }

        private void Start()
		{
			
		}

		private void Update()
		{
			
		}
		
		private void OnEnable()
		{
			
		}

        #region Custom material
        public void RegisterCustomMaterial_GameUnit(GameUnit gameunit)
		{
			if (registered_CustomMat_GameUnitIDs.Contains(gameunit.ID)) { return; }
			registered_CustomMat_GameUnitIDs.Add(gameunit.ID);

            foreach (var meshRendr in gameunit.modelView)
			{
				foreach(var material in meshRendr.sharedMaterials)
				{
					if (allMaterialGameUnit.Find(x => x.materialID == material.name) != null) continue;

					//Let's create material
					if (material.shader.name == gameUnitShader.name)
					{
                        MaterialGameUnit matDat = new MaterialGameUnit();
                        matDat.materialID = material.name;

                        var newMat = new Material(material);
                        newMat.SetColor("_FactionColor", Map.instance.GetColorFaction(Unit.Player.neutral));
                        newMat.name += "_neutral";
                        matDat.mat_factionNeutral = newMat;

                        //ordered from player 1-8          
                        {
                            var mat_1 = new Material(material);
                            mat_1.SetColor("_FactionColor", Map.instance.GetColorFaction(Unit.Player.Player1));
                            mat_1.name += "_Player1";
                            matDat.mat_faction1 = mat_1;
                        }
                        {
                            var mat_2 = new Material(material);
                            mat_2.SetColor("_FactionColor", Map.instance.GetColorFaction(Unit.Player.Player2));
                            mat_2.name += "_Player2";
                            matDat.mat_faction2 = mat_2;
                        }
                        {
                            var mat_3 = new Material(material);
                            mat_3.SetColor("_FactionColor", Map.instance.GetColorFaction(Unit.Player.Player3));
                            mat_3.name += "_Player3";
                            matDat.mat_faction3 = mat_3;
                        }
                        {
                            var mat_4 = new Material(material);
                            mat_4.SetColor("_FactionColor", Map.instance.GetColorFaction(Unit.Player.Player4));
                            mat_4.name += "_Player4";
                            matDat.mat_faction4 = mat_4;
                        }
                        {
                            var mat_5 = new Material(material);
                            mat_5.SetColor("_FactionColor", Map.instance.GetColorFaction(Unit.Player.Player5));
                            mat_5.name += "_Player5";
                            matDat.mat_faction5 = mat_5;
                        }
                        {
                            var mat_6 = new Material(material);
                            mat_6.SetColor("_FactionColor", Map.instance.GetColorFaction(Unit.Player.Player6));
                            mat_6.name += "_Player6";
                            matDat.mat_faction6 = mat_6;
                        }
                        {
                            var mat_7 = new Material(material);
                            mat_7.SetColor("_FactionColor", Map.instance.GetColorFaction(Unit.Player.Player7));
                            mat_7.name += "_Player7";
                            matDat.mat_faction7 = mat_7;
                        }
                        {
                            var mat_8 = new Material(material);
                            mat_8.SetColor("_FactionColor", Map.instance.GetColorFaction(Unit.Player.Player8));
                            mat_8.name += "_Player8";
                            matDat.mat_faction8 = mat_8;
                        }

                        matDat.unitID = gameunit.ID;

                        allMaterialGameUnit.Add(matDat);	
                    }
                    else continue;
				}
			}
		}
		public void OverrideCustomMaterial_GameUnit(GameUnit gameUnit)
		{
            foreach (var meshRendr in gameUnit.modelView)
            {
                var demArrays = meshRendr.sharedMaterials;
                for (var i = 0; i < demArrays.Length; i++)
                {
                    var material = demArrays[i];
                    MaterialGameUnit matDat = allMaterialGameUnit.Find(x => x.materialID == material.name);
                    if (matDat == null) { continue; }

                    demArrays[i] = matDat.GetMaterial(gameUnit.stat_faction);
                }

                meshRendr.sharedMaterials = demArrays;
            }
        }
        #endregion
    }
}