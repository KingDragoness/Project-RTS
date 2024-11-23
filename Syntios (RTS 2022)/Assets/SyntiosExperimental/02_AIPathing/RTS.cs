using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ProtoRTS
{
	public class RTS : MonoBehaviour
	{
        [SerializeField] private Camera mainCamera;
        [SerializeField] private RTSCamera rtsCamera;
        [SerializeField] private RTSController controller;
        [SerializeField] private Selection selection;

        public static RTS instance;

        private void Awake()
        {
            instance = this;
        }

        public static Camera MainCamera { get { return instance.mainCamera; } }
        public static RTSCamera RTSCamera { get { return instance.rtsCamera; } }
        public static RTSController Controller { get { return instance.controller; } }
        public static Selection Selection { get { return instance.selection; } }

    }
}