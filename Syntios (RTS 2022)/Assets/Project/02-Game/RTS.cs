using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ProtoRTS.Game
{
	public class RTS : MonoBehaviour
	{
   

        public static RTS instance;

        private void Awake()
        {
            instance = this;
        }

        public static bool Exists
        {
            get { return instance != null ? true : false; }

        }
    }
}