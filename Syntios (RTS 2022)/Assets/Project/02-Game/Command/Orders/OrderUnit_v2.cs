using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Newtonsoft.Json;

namespace ProtoRTS.Game
{

    [System.Serializable]
    public abstract class OrderUnit_v2
	{
        [JsonIgnore] public GameUnit targetGU;
        public string targetGU_guid;
        public Vector3 pos1;
        public Vector3 pos2;
        public GameUnitBehavior.Class GUB_class = GameUnitBehavior.Class.GUB_Move;

    }

    //TRAINING A UNIT IS NOT AN ORDER!
    [System.Serializable]
    public class TrainingQueue
    {
        [JsonIgnore] public SO_GameUnit gameUnitClass;
        public string gameUnitID;
        public float timeTrained = 0;

        public void Save()
        {
            gameUnitID = gameUnitClass.ID;
        }
    }
}