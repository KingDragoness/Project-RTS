using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ProtoRTS
{
	public class FactionAlliance : MonoBehaviour
	{


		[System.Serializable]
		public class Force
		{
			public string ForceName = "Soviet";
            public List<Unit.Player> Factions;
        }

		[System.Serializable]
		public class FactionRules
		{
			public Unit.Player faction;
            public List<Unit.Player> enemyFactions;
            public Unit.Race race;
		}

		public List<Force> Forces = new List<Force>();
		public List<FactionRules> allFactionsRules = new List<FactionRules>();

		private static FactionAlliance _instance;

        public static FactionAlliance Instance { get => _instance; }

        private void Awake()
        {
            _instance = this;
        }

        public bool IsSameFaction(Unit.Player myPlayer, Unit.Player otherPlayer)
		{
			if (myPlayer == otherPlayer) 
			{ 
				//Debug.Log("IsSameFaction function is referencing the same faction!");
				return true; 
			} 


            foreach (var force in Forces)
            {
                if (force.Factions.Contains(myPlayer) && force.Factions.Contains(otherPlayer))
				{
					return true;
				}
            }

			return false;
        }

        public Unit.TypePlayer GetFactionStatus(Unit.Player myPlayer, Unit.Player otherPlayer)
        {
            if (myPlayer == otherPlayer)
            {
                //Debug.Log("IsEnemy function is referencing the same faction!");
                return Unit.TypePlayer.Player;
            }

            var myFaction = allFactionsRules.Find(x => x.faction == myPlayer);

            if (myFaction != null)
            {
                if (myFaction.enemyFactions.Contains(otherPlayer))
                {
                    return Unit.TypePlayer.Enemy;
                }
                else
                {
                    return Unit.TypePlayer.Neutral;
                }
            }

            return Unit.TypePlayer.Neutral;

        }

        public bool IsEnemy(Unit.Player myPlayer,Unit.Player otherPlayer)
		{
            if (myPlayer == otherPlayer)
            {
                //Debug.Log("IsEnemy function is referencing the same faction!");
                return false;
            }

			var myFaction = allFactionsRules.Find(x => x.faction == myPlayer);

			if (myFaction != null)
			{
				if (myFaction.enemyFactions.Contains(otherPlayer))
				{
					return true;
				}
			}

            return false;

        }

		/// <summary>
		/// If player's enemyFactions doesn't contain the targeted faction, then it is neutral.
		/// </summary>
		/// <param name="myPlayer"></param>
		/// <param name="otherPlayer"></param>
		/// <returns></returns>
        public bool IsNeutral(Unit.Player myPlayer, Unit.Player otherPlayer)
        {
            if (myPlayer == otherPlayer)
            {
                Debug.Log("IsEnemy function is referencing the same faction!");
                return true;
            }

            var myFaction = allFactionsRules.Find(x => x.faction == myPlayer);

            if (myFaction != null)
            {
                if (myFaction.enemyFactions.Contains(otherPlayer))
                {
                    return false;
                }
            }

            return true;

        }
    }
}