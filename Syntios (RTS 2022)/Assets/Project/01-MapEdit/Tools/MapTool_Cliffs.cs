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
		[Range(1,10)] public byte cliffLevelTarget = 1;
		[Range(0f, 1f)] public float hardness = 0.9f;
		[Range(1, 32)] public int brushSize = 4;
		[Range(5, 128)] public int brushStrength = 45;
		public int refreshRate = 5;

		public bool isMaskByDistance = false;
		public int circle_cutoff = 30;
		[FoldoutGroup("DEBUG")] public GameObject DEBUG_start;
		[FoldoutGroup("DEBUG")] public GameObject DEBUG_end;


		private float _refreshTime = 1f;
		private bool _allowMouseToEdit = false;
		private byte original_pixelPos_cliffTarget;



		private void Start()
		{
	
		}



		private void Update()
		{
			_refreshTime -= Time.deltaTime;

			if (currentOperation != Operation.None)
			{
				Brush.EnableBrush();
				Brush.isCliff = true;
			}
			else
			{
				Brush.DisableBrush();
			}


			if (Input.GetMouseButtonDown(0))
			{
				if (MainUI.GetEventSystemRaycastResults().Count == 0)
				{
					var v3_pos = WorldPosToCliffmapPos(Brush.BrushPosition);
					original_pixelPos_cliffTarget = SampleCurrentCliffmapHeight(v3_pos);

					_allowMouseToEdit = true;
				}
				else
					_allowMouseToEdit = false;
			}

			if (_refreshTime <= 0f)
			{
				if (Input.GetMouseButton(0) && _allowMouseToEdit
					&& currentOperation != Operation.None)
				{
					Update_BrushingCliff();
				}

				_refreshTime = 1f / refreshRate;
			}

			if (Input.GetMouseButtonUp(0))
			{
				//Map.UpdateTerrainMap();
				//Map.instance.UpdateCliffMap();
			}


			if (isMaskByDistance)
			{
				Brush.targetShape = MapToolBrush.Shape.Circle;
				Brush.targetBrushSize = brushSize * 3f;
			}
			else
			{
				Brush.targetShape = MapToolBrush.Shape.Square;
				Brush.targetBrushSize = brushSize * 4;
			}
		}


		private void Update_BrushingCliff()
		{
			int pixelWidthBrush = brushSize;
			int totalLength = (brushSize) * (brushSize);
			float halfSize = brushSize / 2f;
			Vector3 posOrigin = Brush.BrushPosition - new Vector3(halfSize / 2f, 0, halfSize / 2f);
			Vector2Int pixelPosCenter = WorldPosToCliffmapPos(Brush.BrushPosition);
			Vector2Int pixelPosOrigin = WorldPosToCliffmapPos(posOrigin);

			byte targetHeight = original_pixelPos_cliffTarget;

			if (currentOperation == Operation.SameLevel)
            {
				
            }
			else if (currentOperation == Operation.Raise)
			{
				if (targetHeight < 16)
                {
					targetHeight++;
				}
			}
			else if (currentOperation == Operation.Lower)
			{
				if (targetHeight > 0)
				{
					targetHeight--;
				}
			}

			int countDebug = 0;

			//Debug.Log($"Size: {totalLength} | {pixelPosOrigin}");
			//Debug.Log($"Cliffmap origin: {pixelPosOrigin} | worldOrigin: {posOrigin}");


			for (int i = 0; i < totalLength; i++)
			{
				int x = i % pixelWidthBrush;
				int y = Mathf.FloorToInt(i / pixelWidthBrush);
				Vector2Int pixelPos = new Vector2Int(pixelPosOrigin.x, pixelPosOrigin.y);

				if (halfSize >= 1)
				{
					pixelPos.x -= halfSize.ToInt();
					pixelPos.y -= halfSize.ToInt();
				}
				else
                {
					pixelPos.x -= 1;
					pixelPos.y -= 1;
				}
			
				pixelPos.x += x;
				pixelPos.y += y;

				if (pixelPos.x >= Map.TerrainData.size_x) continue;
				if (pixelPos.x < 0) continue;
				if (pixelPos.y >= Map.TerrainData.size_y) continue;
				if (pixelPos.y < 0) continue;

				int currentIndex = HeightmapPosToIndex(pixelPos);
				byte strength_i = 255;

				if (isMaskByDistance)
				{
					Vector2Int midPosLocal = new Vector2Int(pixelPosCenter.x, pixelPosCenter.y);
					float dist = Vector2Int.Distance(pixelPos, midPosLocal);
					float widthBrush_f = (float)brushSize;
					float cLength = (widthBrush_f * widthBrush_f) + (widthBrush_f * widthBrush_f);
					cLength = Mathf.Sqrt(cLength);

					float rawValue = dist / (float)widthBrush_f;
					rawValue = 1 - rawValue;
					rawValue *= hardness;
					rawValue *= 255f;
					rawValue = Mathf.Clamp(rawValue, 0, 255);
					if (rawValue < circle_cutoff) rawValue = 0f; //CUT OFF
					strength_i = (byte)rawValue;
				}

				countDebug++;

				BrushCliff(currentIndex, targetHeight);


			}


            //draw pyramid underneath/add on the edges
            {

            }

			{
				//bigger refresh
				int refreshRadius = ((brushSize + 2) * 2) + 4;
				float refresh_halfSize = refreshRadius / 2f;
				Vector3 refreshPosOrigin_v3 = Brush.BrushPosition - new Vector3(refresh_halfSize, 0, refresh_halfSize);
				Vector3 refreshPosCenter = Brush.BrushPosition;
				Vector2Int refreshPosOrigin = WorldPosToCliffmapPos(refreshPosOrigin_v3);


				//divides /2 for world conversion
				Map.instance.UpdateCliffMap(refreshPosOrigin.x, refreshPosOrigin.y, refreshRadius/2, refreshRadius/2);
				//Debug.Log($"Origin: {refreshPosOrigin}  Center: {refreshPosCenter}  | radius: {refreshRadius}  | halfsize: {refresh_halfSize}");

				DEBUG_start.transform.position = refreshPosOrigin_v3;
				DEBUG_end.transform.position = refreshPosOrigin_v3 + new Vector3(refreshRadius,0,refreshRadius);

			}

		}

		public void BrushCliff(int currentIndex, byte height)
		{
			Map.TerrainData.cliffLevel[currentIndex] = height;
			Map.TerrainData.manmadeCliffs[currentIndex] = isManmade;
		}

		public byte SampleCurrentCliffmapHeight(Vector2Int pixelPos)
        {
			int index_cm = HeightmapPosToIndex(pixelPos);

			return Map.TerrainData.cliffLevel[index_cm];
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