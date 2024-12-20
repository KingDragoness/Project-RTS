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


		public static readonly string MapEditorPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "/My Games/Syntios/_protoTerrain";

		public static MapEdit instance;



		private void Awake()
        {
			instance = this;
			BuildFolders();
        }

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
	}
}