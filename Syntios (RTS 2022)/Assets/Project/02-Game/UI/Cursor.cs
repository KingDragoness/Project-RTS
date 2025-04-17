using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ProtoRTS.Game
{
	public class Cursor : MonoBehaviour
	{

		public enum Type
		{
			Normal,
			SelectTarget
		}

		public Texture2D cursor_Normal;
		public Texture2D cursor_Target;

		private static Cursor _instance;

        public static Cursor Instance { get => _instance; }

        private void Awake()
        {
            _instance = this;	
        }

        public static void Change(Type _type)
		{
			var texture2d = _instance.cursor_Normal;

            if (_type == Type.Normal) UnityEngine.Cursor.SetCursor(texture2d, Vector2.zero, CursorMode.Auto);
            if (_type == Type.SelectTarget) UnityEngine.Cursor.SetCursor(_instance.cursor_Target, new Vector2(0.5f,0.5f), CursorMode.Auto);

        }
    }
}