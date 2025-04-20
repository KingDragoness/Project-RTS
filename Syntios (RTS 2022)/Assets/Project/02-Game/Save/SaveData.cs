using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using ProtoRTS.Game;

namespace ProtoRTS
{

	[System.Serializable]
	public class SaveData
	{

        [System.Serializable]
		public class UnitData
		{
			public string guid = "";
			public string unitID = "Seaver"; //custom unit has prefix: custom_[NAME OF CUSTOM UNIT]
			public Vector3 unitPosition;

            public System.UInt32 stat_HP = 50;
            public System.UInt32 stat_Energy = 190;
            public System.UInt16 stat_KillCount = 0;
			public Unit.Player stat_Faction = Unit.Player.Player1;
            public List<Orders.UnitOrder> allOrders = new List<Orders.UnitOrder>();

            public Vector3 move_TargetPos;
			public System.UInt32 move_TargetUnit_guid; //when loading game, read from SaveData

        }

        [System.Serializable]
        public class FactionSheetData
        {
            public Unit.Player Faction;
            public int Mineral = 0;
            public int Energy = 0;
            public bool[,] activePoints;
            public bool[,] exploredPoints;

            public FactionSheet DatToFS()
            {
                FactionSheet fs = new FactionSheet(Faction);
                fs.Faction = Faction;
                fs.Mineral = Mineral;
                fs.Energy = Energy;
                return fs;
            }
        }

        public Unit.Player playerFaction = Unit.Player.Player1;
        public int IncrementUnitGUID = 0;
        public List<UnitData> allUnits = new List<UnitData>();
        public List<FactionSheetData> allFactions = new List<FactionSheetData>();

        [FoldoutGroup("Terrain")] public SyntiosTerrainData terrain;
        [FoldoutGroup("Terrain")] public string terrain_PresetID = "Earth";

        public Vector3 cameraPosition;

    }
}