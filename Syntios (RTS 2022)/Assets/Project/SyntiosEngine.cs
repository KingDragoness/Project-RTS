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
        private int _supplyProvider = 0;

        public FactionSheet(Unit.Player faction)
        {
            Faction = faction;
        }

        public int Supply
        {
            get 
            {
                return _supplyCount; 
            }
        }

        public int SupplyProvider
        {
            get
            {
                return _supplyProvider;
            }
        }

        internal void CalcSupply()
        {
            _supplyCount = 0;
            _supplyProvider = 0;

            foreach (var unit in ListedGameUnits)
            {
                if (unit.CheckFlag(Unit.Tag.SupplyProvider) == false)
                {
                    _supplyCount += unit.Class.SupplyCount;

                } 
                else
                {
                    _supplyProvider += unit.Class.SupplyProvide;
                }
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

        [Tooltip("targets frame per second (Best played at 60 FPS)")] public int FPSTarget = 60;
        public bool engine_PrintErrorMissingBehavior = false;
		public List<GameUnit> ListedGameUnits = new List<GameUnit>();
        public int UnitIncrementGUID = 0;
        public Gamemode CurrentGamemode;

        [SerializeField] private SaveData _saveDat;
        private List<FactionSheet> allFactions = new List<FactionSheet>();
        [SerializeField] private Unit.Player currentFaction;
        [FoldoutGroup("Base Stats")] private float multiplierTrainingSpeed = 1f;

        public static SyntiosEngine Instance;

        public static List<FactionSheet> AllFactions { get => Instance.allFactions; }
        public static FactionSheet MyFactionSheet { get => Instance.GetFactionSheet(CurrentFaction); }


        public static Unit.Player CurrentFaction { get => Instance.currentFaction; set => Instance.currentFaction = value; }
        public static Gamemode CurrentMode { get => Instance.CurrentGamemode; }
        public static SaveData SaveData { get => Instance._saveDat; set => Instance._saveDat = value; }
        public static float MultiplierTrainingSpeed { get => Instance.multiplierTrainingSpeed; set => Instance.multiplierTrainingSpeed = value; }

        public static readonly string SavePath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "/My Games/Syntios/Saves";

        private void Awake()
        {
            Instance = this;

        }

        private void Start()
        {
            //Application.targetFrameRate = FPSTarget;

        }

        public void AddNewUnit(GameUnit unit)
        {
            ListedGameUnits.Add(unit);
            var faction = GetFactionSheet(unit.stat_faction);
            faction.ListedGameUnits.Add(unit);
            faction.CalcSupply();
            SyntiosEvents.UI_ReselectUpdate?.Invoke();
            unit.guid = UnitIncrementGUID.ToString();
            UnitIncrementGUID++;
        }

        public void RemoveUnit(GameUnit unit)
        {
            ListedGameUnits.Remove(unit);
            var faction = GetFactionSheet(unit.stat_faction);
            faction.ListedGameUnits.Remove(unit);
            faction.CalcSupply();
            Selection.AllSelectedUnits.Remove(unit);
            SyntiosEvents.UI_ReselectUpdate?.Invoke();


        }

        public static GameUnit GetUnit (string guid)
        {
            return Instance.ListedGameUnits.Find(x => x.guid == guid);
        }

        public FactionSheet GetFactionSheet(Unit.Player factionId)
        {
            return allFactions.Find(x => x.Faction == factionId);
        }

        public bool CheckMineralEnough(int mineralCost)
        {
            return MyFactionSheet.Mineral >= mineralCost;
        }
        public bool CheckEnergyEnough(int energyCost)
        {
            return MyFactionSheet.Energy >= energyCost;
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