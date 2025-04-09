using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ProtoRTS
{
	public class Tick : MonoBehaviour
	{
        public static System.Action<int> OnTick;

        private int _tick = 0;

		[SerializeField] private int ticksPerSecond = 20;

        private float _timer = 0;

        public static Tick Instance;

        /// <summary>
        /// Best way to implement it is (Tick.Current % 4 == 0)
        /// </summary>
        public static int Current
        {
            get
            {
                return Instance._tick;
            }
        }

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            _timer -= Time.deltaTime;

            if (_timer <= 0f)
            {
                _timer = 1f / ticksPerSecond;
                OnTick?.Invoke(_tick);
                _tick++;
            }
        }

    }
}