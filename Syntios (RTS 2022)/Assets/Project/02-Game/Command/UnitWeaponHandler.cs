using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ProtoRTS.Game
{

    /// <summary>
    /// Only handle attacking. Do not process movement here
    /// </summary>
	public class UnitWeaponHandler : MonoBehaviour
	{

		public GameUnit gameUnit;

		private float _timerLastAttackCommand = 0.15f;

        private void Update()
        {
            if (_timerLastAttackCommand < 0)
            {
                //do not process attack
            }
            else
            {
                _timerLastAttackCommand += Time.deltaTime;
                //process attack
            }
        }

        public void Attacking()
        {
            _timerLastAttackCommand = 0.15f;
        }


        public SO_Weapon GroundWeapon()
        {
            if (HasAnyWeapon() == false) return null;
            //need to be changed
            return gameUnit.Class.allWeapons.Find(x => x.groundAttack);
        }

        public SO_Weapon AirWeapon()
        {
            if (HasAnyWeapon() == false) return null;
            //need to be changed
            return gameUnit.Class.allWeapons.Find(x => x.airAttack);
        }

        public bool HasAnyWeapon()
        {
            if (gameUnit.Class.allWeapons.Count == 0) return false;
            return true;
        }
    }
}