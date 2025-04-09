using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ProtoRTS
{

    public enum Gamemode
    {
        Mainmenu,
        Game,
        MapEdit
    }

    /// <summary>
    /// Listing of factions' units.
    /// </summary>
    [System.Serializable]
    public class FactionSheet
    {
        [ShowInInspector] public List<GameUnit> ListedGameUnits = new List<GameUnit>();
        public Unit.Player Faction;
        public int Mineral = 0;
        public int Energy = 0;

        private int _supplyCount = 0;

        public FactionSheet(Unit.Player faction)
        {
            Faction = faction;
        }

        public int Supply
        {
            get 
            {
                _supplyCount = 0;
                foreach (var unit in ListedGameUnits)
                {
                    _supplyCount += unit.Class.SupplyCount;
                }

                return _supplyCount; 
            }
        }

        public SaveData.FactionSheetData GetSaveData()
        {
            SaveData.FactionSheetData dat = new SaveData.FactionSheetData();
            dat.Faction = Faction;
            dat.Mineral = Mineral;
            dat.Energy = Energy;

            var fow = FOWScript.GetFOW(Faction);

            if (fow != null)
            {
                dat.exploredPoints = fow.exploredPoints;
                dat.activePoints = fow.activePoints;
            }


            return dat;
        }

    
    }

    public class SyntiosEngine : MonoBehaviour
	{

		public List<GameUnit> ListedGameUnits = new List<GameUnit>();
        public Gamemode CurrentGamemode;
        [SerializeField] private SaveData _saveDat;
        private List<FactionSheet> allFactions = new List<FactionSheet>();
        [SerializeField] private Unit.Player currentFaction;

        public static SyntiosEngine Instance;

        public static List<FactionSheet> AllFactions { get => Instance.allFactions; }
        public static FactionSheet MyFactionSheet { get => Instance.GetFactionSheet(CurrentFaction); }


        public static Unit.Player CurrentFaction { get => Instance.currentFaction; }
        public static Gamemode CurrentMode { get => Instance.CurrentGamemode; }
        public static SaveData SaveData { get => Instance._saveDat; set => Instance._saveDat = value; }
        public static readonly string SavePath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "/My Games/Syntios/Saves";

        private void Awake()
        {
            Instance = this;

        }

        private void Start()
        {


        }

        public void AddNewUnit(GameUnit unit)
        {
            ListedGameUnits.Add(unit);
            var faction = GetFactionSheet(unit.stat_faction);
            faction.ListedGameUnits.Add(unit);
            SyntiosEvents.UI_ReselectUpdate?.Invoke();

        }

        public void RemoveUnit(GameUnit unit)
        {
            ListedGameUnits.Remove(unit);
            var faction = GetFactionSheet(unit.stat_faction);
            faction.ListedGameUnits.Remove(unit);
            Selection.AllSelectedUnits.Remove(unit);
            SyntiosEvents.UI_ReselectUpdate?.Invoke();

        }

        public FactionSheet GetFactionSheet(Unit.Player factionId)
        {
            return allFactions.Find(x => x.Faction == factionId);
        }


        private void OnDisable()
        {
            Unit.Clear();
        }

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.BackQuote))
            {
                var console = DevConsole.Instance.consoleInputObject;
                console.SetActive(!console.gameObject.activeSelf);
            }
        }


    }
}