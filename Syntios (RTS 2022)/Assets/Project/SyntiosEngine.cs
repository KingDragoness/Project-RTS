using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ProtoRTS
{
	public class SyntiosEngine : MonoBehaviour
	{

		public List<GameUnit> ListedGameUnits = new List<GameUnit>();
        public Unit.Player CurrentFaction;

		public static SyntiosEngine Instance;

        private void Awake()
        {
            Instance = this;
        }

    }
}