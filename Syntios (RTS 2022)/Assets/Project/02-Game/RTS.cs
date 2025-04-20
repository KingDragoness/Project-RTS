using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;
using Sirenix.OdinInspector;
using Newtonsoft.Json;
using Pathfinding;
using System.Text;
using System;
using System.Linq;

namespace ProtoRTS.Game
{
	public class RTS : MonoBehaviour
	{


        public RTSCamera rtsCamera;
        public Map map;
        public FOWScript fowScript;
        public CommandUnit commandUnit;

        public static RTS instance;

        public static bool IsLoadFromSaveFile = false;
        public static SaveData cachedLoadedSave;

        private void Awake()
        {
            instance = this;
            BuildFolders();
            InitializeGame();
        }

        private void Start()
        {
            if (IsLoadFromSaveFile)
            {
                fowScript.LaunchLoadedGame();


                //create game units
                {
                    var allGameUnitSOs = Resources.LoadAll<SO_GameUnit>("GameUnits").ToList();

                    foreach (var unitDat in cachedLoadedSave.allUnits)
                    {
                        var gameUnitSO = allGameUnitSOs.Find(x => x.ID == unitDat.unitID);
                        if (gameUnitSO == null)
                        {
                            continue;
                        }
                        var newUnit = GameUnit.CreateUnit(unitDat, gameUnitSO);
                    }
                }
            }

            IsLoadFromSaveFile = false;
        }

        //implement in map edit too
        public void InitializeGame()
        {
            if (IsLoadFromSaveFile)
            {
                Awake_GameLoaded();
            }
            else
            {
                 SyntiosEngine.AllFactions.Add(new FactionSheet(Unit.Player.neutral));
                 SyntiosEngine.AllFactions.Add(new FactionSheet(Unit.Player.Player1));
                 SyntiosEngine.AllFactions.Add(new FactionSheet(Unit.Player.Player2));
                 SyntiosEngine.AllFactions.Add(new FactionSheet(Unit.Player.Player3));
                 SyntiosEngine.AllFactions.Add(new FactionSheet(Unit.Player.Player4));
                 SyntiosEngine.AllFactions.Add(new FactionSheet(Unit.Player.Player5));
                 SyntiosEngine.AllFactions.Add(new FactionSheet(Unit.Player.Player6));
                 SyntiosEngine.AllFactions.Add(new FactionSheet(Unit.Player.Player7));
                 SyntiosEngine.AllFactions.Add(new FactionSheet(Unit.Player.Player8));
                 SyntiosEngine.AllFactions.Add(new FactionSheet(Unit.Player.Player9));
            }
        }

        public static bool Exists
        {
            get { return instance != null ? true : false; }

        }

        public void BuildFolders()
        {
            Directory.CreateDirectory(SyntiosEngine.SavePath);

        }


        #region Load

        public void Awake_GameLoaded()
        {
            Debug.Log("Awake_GameLoaded");
            Map.instance.DEBUG_dontInitializeData = true;
            Map.instance.LoadGame_LoadSyntiosTerrainDat(cachedLoadedSave.terrain);
            rtsCamera.transform.position = cachedLoadedSave.cameraPosition;
            SyntiosEngine.CurrentFaction = cachedLoadedSave.playerFaction;
            SyntiosEngine.Instance.UnitIncrementGUID = cachedLoadedSave.IncrementUnitGUID;

            //faction sheet
            {
                foreach(var FSDat in cachedLoadedSave.allFactions)
                {
                    SyntiosEngine.AllFactions.Add(FSDat.DatToFS());
                }
            }

            //clear game unit (this is for editor; in final release, there is no unit left on 02-ProtoRTSsystem.scene)
            {
                var allGameUnits = FindObjectsByType<GameUnit>(FindObjectsInactive.Include, FindObjectsSortMode.None);

                foreach (var unit in allGameUnits)
                {
                    Destroy(unit.gameObject);
                }
            }

        }

        [FoldoutGroup("DEBUG")]
        [DisableInEditorMode]
        [Button("DEBUG - Load developer save file")]
        public void DEVELOPER_LoadDevSave()
        {
            var savefile1 = UnpackSaveFile_Uncompressed(SyntiosEngine.SavePath + $"/{DEBUG_SaveFilePathname}.TESTSAVE");
            Debug.Log($"Attempt load developer's save file: " + SyntiosEngine.SavePath + $"/{DEBUG_SaveFilePathname}.TESTSAVE");
            DevConsole.Instance.SendConsoleMessage($"Attempt load developer's save file: " + SyntiosEngine.SavePath + $"/{DEBUG_SaveFilePathname}.TESTSAVE");

            if (savefile1 != null)
            {
                IsLoadFromSaveFile = true;
                cachedLoadedSave = savefile1;
                Application.LoadLevel(Application.loadedLevel);
                Debug.Log("Loaded.");
            }
        }


        public void LoadGame(string savefilename)
        {
            var savefile1 = UnpackSaveFile_Gz(SyntiosEngine.SavePath + $"/{savefilename}.save");
            Debug.Log($"Attempt load save file: " + SyntiosEngine.SavePath + $"/{savefilename}.save");
            DevConsole.Instance.SendConsoleMessage($"Attempt load save file: " + SyntiosEngine.SavePath + $"/{savefilename}.save");

            if (savefile1 != null)
            {
                IsLoadFromSaveFile = true;
                cachedLoadedSave = savefile1;
                Application.LoadLevel(Application.loadedLevel);
                Debug.Log("Loaded.");
            }
        }

        public SaveData UnpackSaveFile_Uncompressed(string path)
        {
            SaveData result = null;
            JsonSerializerSettings settings = JsonSettings();

            try
            {
                result = JsonConvert.DeserializeObject<SaveData>(File.ReadAllText(path), settings);
            }
            catch
            {
                Debug.LogError("Failed load savefile!");
                DevConsole.Instance.SendConsoleMessage("Savefile cannot be loaded!");
            }

            return result;
        }

        

        [FoldoutGroup("DEBUG")]
        [Button("DEBUG_TestDecompressLoad")]
        public void DEBUG_TestDecompressLoad()
        {
            var savefile1 = UnpackSaveFile_Gz(SyntiosEngine.SavePath + $"/{DEBUG_SaveFilePathname}.save");
            Debug.Log($"{savefile1.playerFaction} - {savefile1.allUnits.Count}");
        }

        public SaveData UnpackSaveFile_Gz(string path)
        {
            SaveData result = null;
            JsonSerializerSettings settings = JsonSettings();

            try
            {
                var compressedDat = File.ReadAllBytes(path);
                var decompressedData = DecompressJsonData(compressedDat);
                string deCompressedString = Encoding.UTF8.GetString(decompressedData);

                result = JsonConvert.DeserializeObject<SaveData>(deCompressedString, settings);
            }
            catch (Exception e)
            {
                Debug.LogError("Failed load savefile!");
                Debug.LogError(e.Message);
                Debug.LogError(e.StackTrace);
                DevConsole.Instance.SendConsoleMessage("Savefile cannot be loaded!");
            }

            return result;
        }

        #endregion

        #region Save
        [FoldoutGroup("DEBUG")]
        public string DEBUG_SaveFilePathname = "D";

        [FoldoutGroup("DEBUG")]
        [DisableInEditorMode]
        [Button("Save Game")]
        public void DEBUG_SaveGame()
        {
            SaveGame(DEBUG_SaveFilePathname);
        }

        public void SaveGame(string saveName)
        {
            if (saveName == "")
            {
                DevConsole.Instance.SendConsoleMessage("Save failed! Need name!");
                return;
            }

            SyntiosEngine.SaveData = PackSaveData();

            string pathSave = SyntiosEngine.SavePath + $"/{saveName}.save";
            string jsonTypeNameAll = JsonConvert.SerializeObject(SyntiosEngine.SaveData, Formatting.Indented, JsonSettings());


            // Compress JSON data using GZip
            byte[] compressedJsonData = CompressJsonData(jsonTypeNameAll);
            Debug.Log($"Original file: {jsonTypeNameAll.Length} Bytes");
            Debug.Log($"Compressed (GZip): {compressedJsonData.Length} Bytes");
            Debug.Log($"{saveName}.save | Save game file saved: {pathSave}");

            File.WriteAllBytes(pathSave, compressedJsonData);

            //File.WriteAllText(pathSave, jsonTypeNameAll);
        }

        [FoldoutGroup("DEBUG")]
        [DisableInEditorMode]
        [Button("DEBUG - UncompressedSave")]
        public void DEVELOPER_UncompressedSave()
        {
            string saveName = DEBUG_SaveFilePathname;

            SyntiosEngine.SaveData = PackSaveData();

            string pathSave = SyntiosEngine.SavePath + $"/{saveName}.TESTSAVE";
            string jsonTypeNameAll = JsonConvert.SerializeObject(SyntiosEngine.SaveData, Formatting.Indented, JsonSettings());
            Debug.Log($"{saveName}.TESTSAVE | DEVELOPER save game file saved: {pathSave}");

            File.WriteAllText(pathSave, jsonTypeNameAll);
        }

        public static JsonSerializerSettings JsonSettings()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
            };

            return settings;
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


        private SaveData PackSaveData()
        {
            var saveDat = new SaveData();

            saveDat.terrain = Map.TerrainData;
            saveDat.terrain_PresetID = Map.TerrainData.presetID;
            saveDat.playerFaction = SyntiosEngine.CurrentFaction;
            saveDat.IncrementUnitGUID = SyntiosEngine.Instance.UnitIncrementGUID;
            saveDat.allUnits.Clear();
            saveDat.cameraPosition = RTSCamera.Instance.transform.position;

            foreach (var unit in SyntiosEngine.Instance.ListedGameUnits)
            {
                var unitDat = SaveUnitData(unit);
                saveDat.allUnits.Add(unitDat);
            }

            foreach (var faction in SyntiosEngine.AllFactions)
            {
                var dat = faction.GetSaveData();
                saveDat.allFactions.Add(dat);
            }

            return saveDat;
        }

        public SaveData.UnitData SaveUnitData(GameUnit unit)
        {
            SaveData.UnitData unitData = new SaveData.UnitData();

            unitData.unitPosition = unit.transform.position;
            unitData.unitID = unit.Class.ID;
            unitData.stat_HP = (System.UInt32)unit.stat_HP;
            unitData.stat_Faction = unit.stat_faction;
            unitData.stat_Energy = (System.UInt32)unit.stat_Energy;
            unitData.stat_KillCount = (System.UInt16)unit.stat_KillCount;
            unitData.move_TargetPos = unit.move_Target;
            unitData.guid = unit.guid;

            //orders
            {
                foreach (var e in unit.OrderHandler.orders) e.Save();
                unitData.allOrders = unit.OrderHandler.orders;
            }

            return unitData;
        }

        #endregion
    }
}