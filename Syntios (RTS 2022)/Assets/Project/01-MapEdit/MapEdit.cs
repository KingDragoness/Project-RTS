using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Sirenix.OdinInspector;
using Newtonsoft.Json;

namespace ProtoRTS.MapEditor
{
	


	//MapEdit modifies terrain. To refresh terrain use Map.cs
	public class MapEdit : MonoBehaviour
	{
        [FoldoutGroup("References")] public BoxSelectionUnit boxSelector;
        [FoldoutGroup("Map Tools")] [SerializeField] private MapTool_BrushTexture _brushTexture;
        [Space]
		public static readonly string MapEditorPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "/My Games/Syntios/_protoTerrain";

		public static MapEdit instance;

        public MapTool_BrushTexture BrushTexture { get => _brushTexture; }

        private void Awake()
        {
			instance = this;
			BuildFolders();
        }

        private void Start()
        {
  

            boxSelector.enabled = false;
        }


        #region Folder and save
        public void BuildFolders()
		{
			Directory.CreateDirectory(MapEditorPath);

		}

		[FoldoutGroup("DEBUG")] [Button("Save Game")]
        public void SaveGame(string path = "")
        {
            string pathSave = path;
            bool isSameLevel = true;

            if (path == "") pathSave = MapEditorPath + "/TEST_terrainPreAlpha.map";


            string jsonTypeNameAll = JsonConvert.SerializeObject(Map.TerrainData, Formatting.None, JsonSettings());


            File.WriteAllText(pathSave, jsonTypeNameAll);
   

        }

        public static JsonSerializerSettings JsonSettings()
		{
			JsonSerializerSettings settings = new JsonSerializerSettings
			{
				TypeNameHandling = TypeNameHandling.All,
				ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
				MissingMemberHandling = MissingMemberHandling.Ignore
			};

			return settings;

		}
        #endregion

        #region Tooling System

        private float f_cooldownx = 0;

        private void Update()
        {
            f_cooldownx += Time.deltaTime;

            if (f_cooldownx > (1/5f)) //checks 5 times every second
            {
                CheckToolActive();
                f_cooldownx = 0;
            }
        }
        public void CheckToolActive()
        {

        }

        #endregion
    }
}