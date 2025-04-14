using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Sirenix.OdinInspector;
using Newtonsoft.Json;
using System.IO.Compression;
using System.Text;

namespace ProtoRTS.MapEditor
{
	


	//MapEdit modifies terrain. To refresh terrain use Map.cs
	public class MapEdit : MonoBehaviour
	{


        [FoldoutGroup("References")] public BoxSelectionUnit boxSelector;
        [FoldoutGroup("References")] public MapToolBrush brush;
        [FoldoutGroup("Map Tools")] [SerializeField] private MapTool_BrushTexture _brushTexture;
        [FoldoutGroup("Map Tools")] [SerializeField] private MapTool_Cliffs _brushCliffs;
        [Space]
		public static readonly string MapEditorPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "/My Games/Syntios/_protoTerrain";
        [ShowInInspector] private MapToolScript _currentBrush;

		public static MapEdit instance;

        public MapTool_BrushTexture BrushTexture { get => _brushTexture; }
        public MapTool_Cliffs BrushCliffs { get => _brushCliffs; }
        public MapToolScript CurrentBrush { get => _currentBrush; }


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
        public void SaveGame(string name = "")
        {
            string pathSave = "";

            pathSave = MapEditorPath + $"/{name}.map";


            string jsonTypeNameAll = JsonConvert.SerializeObject(Map.TerrainData, Formatting.Indented, JsonSettings());

            byte[] compressedJsonData = CompressJsonData(jsonTypeNameAll);
            Debug.Log($"Original file: {jsonTypeNameAll.Length} Bytes");
            Debug.Log($"Compressed (GZip): {compressedJsonData.Length} Bytes");
            Debug.Log($"{name}.map | Map file saved: {pathSave}");

            File.WriteAllBytes(pathSave, compressedJsonData);

            //File.WriteAllText(pathSave, jsonTypeNameAll);
        }

        private static byte[] CompressJsonData(string jsonData)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(jsonData);

            using (var memoryStream = new MemoryStream())
            {
                using (var gzipStream = new GZipStream(memoryStream, System.IO.Compression.CompressionLevel.Optimal))
                {
                    gzipStream.Write(byteArray, 0, byteArray.Length);
                }
                return memoryStream.ToArray();
            }
        }

        private static byte[] DecompressJsonData(byte[] bytes)
        {
            using (var memoryStream = new MemoryStream(bytes))
            {

                using (var outputStream = new MemoryStream())
                {
                    using (var decompressStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                    {
                        decompressStream.CopyTo(outputStream);
                    }
                    return outputStream.ToArray();
                }
            }
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

            if (f_cooldownx > (1/10f)) //checks 10 times every second
            {
                CheckToolActive();
                f_cooldownx = 0;
            }
        }
        public void CheckToolActive()
        {
        }

        public void SetCurrentTool(int type)
        {
            if (type == 0)
            {
                _currentBrush = null;
            }
            else if (type == 1)
            {
                _currentBrush = _brushTexture;
            }
            else if (type == 2)
            {
                _currentBrush = _brushCliffs;
            }

            if (type == 1)
            {
                _brushTexture.enabled = true;
            }
            else
            {
                _brushTexture.enabled = false;
            }

            if (type == 2)
            {
                _brushCliffs.enabled = true;
            }
            else
            {
                _brushCliffs.enabled = false;
            }
        }

        #endregion
    }
}