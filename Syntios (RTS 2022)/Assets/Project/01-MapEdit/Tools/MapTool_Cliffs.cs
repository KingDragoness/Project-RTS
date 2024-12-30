using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ProtoRTS.MapEditor
{
	public class MapTool_Cliffs : MapToolScript
	{
		public enum Operation
		{
			None,
			Raise,
			Lower,
			SameLevel
		}

		/// <summary>
		/// Minimum 256 x 256
		/// 
		/// </summary>
		public Texture2D[] brushes;
		public Operation currentOperation;
		public bool isManmade = true;
		[Range(2, 32)] public int brushSize = 4;


		private void Start()
		{
			
		}

		private void Update()
		{
			
		}
		
		private void OnEnable()
		{
			
		}

        public override string GetBrushName()
        {
			return "Cliffs";
        }
    }
}