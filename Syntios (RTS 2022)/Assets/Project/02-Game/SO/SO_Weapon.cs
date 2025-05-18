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
        public int DescriptionOnly_Multiplier = 1; // Titanbots can shoot two rockets which deals 10 damage in total. But the base damage is only 5. Hence, we double to 2
        public bool groundAttack = true;
        public bool airAttack = true;
        public bool canTargetMultiple = false;
        [ShowIf("canTargetMultiple")] public int multipleTargetCount = 2; //allows target more than one unit
        public List<WeaponModifierBonus> modifierDamage = new List<WeaponModifierBonus>();

    }
}