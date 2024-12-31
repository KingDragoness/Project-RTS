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


		public Vector2Int WorldPosToPixelPos(Vector3 worldPos)
		{
			Vector2Int pos_1 = new Vector2Int();
			pos_1.x = Mathf.RoundToInt(worldPos.x * 2);
			pos_1.y = Mathf.RoundToInt(worldPos.z * 2);

			return pos_1;
		}

		public static Vector2Int WorldPosToCliffmapPos(Vector3 worldPos)
		{
			Vector2Int pos_1 = new Vector2Int();
			pos_1.x = Mathf.RoundToInt(worldPos.x / 2);
			pos_1.y = Mathf.RoundToInt(worldPos.z / 2);

			return pos_1;
		}

		public int HeightmapPosToIndex(Vector2Int pos)
		{
			int index = (pos.y * Map.TerrainData.size_x) + (pos.x);
			return index;
		}

		public int PixelPosToIndex(Vector2Int pos)
		{
			int index = (pos.y * 1024) + (pos.x);
			return index;
		}

	}
}