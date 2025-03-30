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
        public List<GameUnit> ListedGameUnits = new List<GameUnit>();
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
    }

    public class SyntiosEngine : MonoBehaviour
	{

		public List<GameUnit> ListedGameUnits = new List<GameUnit>();
        public Gamemode CurrentGamemode;
        private List<FactionSheet> allFactions = new List<FactionSheet>();
        [SerializeField] private Unit.Player currentFaction;

        public static SyntiosEngine Instance;

        public static List<FactionSheet> AllFactions { get => Instance.allFactions; }
        public static FactionSheet MyFactionSheet { get => Instance.GetFactionSheet(CurrentFaction); }


        public static Unit.Player CurrentFaction { get => Instance.currentFaction; }
        public static Gamemode CurrentMode { get => Instance.CurrentGamemode; }


        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            allFactions.Add(new FactionSheet(Unit.Player.neutral));
            allFactions.Add(new FactionSheet(Unit.Player.Player1));
            allFactions.Add(new FactionSheet(Unit.Player.Player2));
            allFactions.Add(new FactionSheet(Unit.Player.Player3));
            allFactions.Add(new FactionSheet(Unit.Player.Player4));
            allFactions.Add(new FactionSheet(Unit.Player.Player5));
            allFactions.Add(new FactionSheet(Unit.Player.Player6));
            allFactions.Add(new FactionSheet(Unit.Player.Player7));
            allFactions.Add(new FactionSheet(Unit.Player.Player8));
            allFactions.Add(new FactionSheet(Unit.Player.Player9));

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