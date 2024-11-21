using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ProtoRTS
{
	public class AnimationSystem : MonoBehaviour
	{

        private static AnimationSystem Instance;

        private void OnEnable()
        {
            Instance = this;
        }

        public static void E()
        {

        }

    }
}