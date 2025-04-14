using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ProtoRTS
{

    [CreateAssetMenu(fileName = "Move", menuName = "Syntios/Button", order = 1)]

    public class CommandButton : ScriptableObject
    {
        public string displayName = "Move";
        public Sprite sprite;
        [TextArea(2,4)] public string tooltip = "Orders selected units to move to the target area or follow the target unit. Moving units will not engage enemies.";
        public bool allowTint = false;
    }
}