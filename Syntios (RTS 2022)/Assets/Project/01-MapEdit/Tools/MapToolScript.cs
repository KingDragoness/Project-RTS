using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ProtoRTS.MapEditor
{


	public abstract class MapToolScript : MonoBehaviour
	{

		public MapToolBrush Brush;

		public abstract string GetBrushName();

		public virtual void OnDisable()
		{
			Brush.DisableBrush();
		}

	}
}