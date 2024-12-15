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

	public class SyntiosEngine : MonoBehaviour
	{

		public List<GameUnit> ListedGameUnits = new List<GameUnit>();
        public Gamemode CurrentGamemode;
        [SerializeField] private Unit.Player currentFaction;

        public static SyntiosEngine Instance;

        public static Unit.Player CurrentFaction { get => Instance.currentFaction; }

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

        }


    }
}