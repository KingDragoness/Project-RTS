using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using ProtoRTS.Game;

namespace ProtoRTS
{

    [CreateAssetMenu(fileName = "SR-71", menuName = "Syntios/Weapon (vanilla)", order = 1)]

    public class SO_Weapon : ScriptableObject
    {

        [System.Serializable]
        public class WeaponModifierBonus
        {
            public Unit.Tag tag = Unit.Tag.Armored;
            public float modifierDamage = 0.85f;
        }

        public string ID = "Sixtus_SR-71";
        public string NameDisplay = "SR-71 Rifle";
        public Sprite WeaponIcon;
        public int BaseDamage = 7;
        public float WeaponSpeed = 0.6f;
        public bool groundAttack = true;
        public bool airAttack = true;
        public List<WeaponModifierBonus> modifierDamage = new List<WeaponModifierBonus>();

    }
}