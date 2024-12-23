using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ProtoRTS.MapEditor
{
    public class MapTool_BrushTexture : MapToolScript
    {

        /// <summary>
        /// Minimum 256 x 256
        /// 
        /// </summary>
        public Texture2D[] brushes;

        [Range(0f,1f)] public float hardness = 0.9f;
        [Range(0, 255)] public int clampOpacity = 200;
        [Range(2, 32)] public int brushSize = 4;
        public int refreshRate = 5;
        [Range(-1,7)] public int brushCurrent = -1;
        
        [FoldoutGroup("Circle Draw")] public bool isMaskByDistance = false; //mouse
        [FoldoutGroup("Circle Draw")] public int circle_cutoff = 30;
        [FoldoutGroup("Circle Draw")] public int brushStrength = 45;

        [Header("References")]
        public Transform brushProjector;
        public Transform DEBUG_originBrush;
        public Projector projector;

        private Vector3 brushPosition = new Vector3();
        private float _refreshTime = 1f;

        private void Update()
        {
            _refreshTime -= Time.deltaTime;
            projector.orthographicSize = brushSize / 2f;

            Update_BrushProjector();

            if (_refreshTime <= 0f)
            {
                if (Input.GetMouseButton(0) && MainUI.GetEventSystemRaycastResults().Count == 0)
                {
                    Update_BrushingTexture();
                }

                _refreshTime = 1f / refreshRate;
            }

            if (Input.GetMouseButtonUp(0))
            {
                Map.UpdateTerrainMap();
            }
        }

        private void Update_BrushProjector()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100))
            {
                Vector3 pos = hit.point;
                pos.y = 50f;

                brushPosition = hit.point;
                brushProjector.transform.position = pos;
            }
        }


        private void Update_BrushingTexture()
        {
            //if brush size 1 = 2 splatmap pixel

            int pixelWidthBrush = brushSize * 2;
            int totalLength = (brushSize * 2) * (brushSize * 2);
            float halfSize = brushSize / 2f;
            Vector3 posOrigin = brushPosition - new Vector3(halfSize, 0, halfSize);
            Vector2Int pixelPosCenter = WorldPosToPixelPos(brushPosition);
            Vector2Int pixelPosOrigin = new Vector2Int();

            pixelPosOrigin = WorldPosToPixelPos(posOrigin);
            DEBUG_originBrush.transform.position = posOrigin;

            int countDebug = 0;

            //had a more fundamental problem with pixel!
            for (int i = 0; i < totalLength; i++)
            {
                int x = i % pixelWidthBrush;
                int y = Mathf.FloorToInt(i / pixelWidthBrush);
                Vector2Int pixelPos = new Vector2Int((pixelPosOrigin.x + halfSize).ToInt(), (pixelPosOrigin.y + halfSize).ToInt());
                pixelPos.x += x/2;
                pixelPos.y += y/2;

                if (pixelPos.x >= 1024) continue;
                if (pixelPos.x < 0) continue;
                if (pixelPos.y >= 1024) continue;
                if (pixelPos.y < 0) continue;

                int currentIndex = PixelPosToIndex(pixelPos);
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
                    countDebug++;
                }

                BrushTerrain(currentIndex, brushCurrent, strength_i);
            }

            //Map.UpdateTerrainMap();
            Map.PartialUpdateTerrainMap(pixelPosOrigin.x, pixelPosOrigin.y, pixelWidthBrush, pixelWidthBrush);

        }

        public void BrushTerrain(int currentIndex, int brush, byte strength)
        {
            //Map.TerrainData.terrain_layer1[currentIndex] = 0;
            //Map.TerrainData.terrain_layer2[currentIndex] = 0;
            //Map.TerrainData.terrain_layer3[currentIndex] = 0;
            //Map.TerrainData.terrain_layer4[currentIndex] = 0;
            //Map.TerrainData.terrain_layer5[currentIndex] = 0;
            //Map.TerrainData.terrain_layer6[currentIndex] = 0;
            //Map.TerrainData.terrain_layer7[currentIndex] = 0;
            //Map.TerrainData.terrain_layer8[currentIndex] = 0;

            float delta = 10 + (brushStrength / refreshRate);
            float current = Map.TerrainData.GetTerrainLayer_str(brush, currentIndex);
            float target = Map.TerrainData.GetTerrainLayer_str(brush, currentIndex) + strength;

            float f = current;

            if (current > target)
            {
                f -= delta;
            }
            if (current < target)
            {
                f += delta;
            }
            f = Mathf.Clamp(f, 0, clampOpacity);

            //everything else reduce opacity
            if (f >= clampOpacity / 2f)
            {
                for(int x = 0; x < 8; x++)
                {
                    if (x == brush) continue;
                    float f2 = Map.TerrainData.GetTerrainLayer_str(x, currentIndex);

                    f2 -= delta;
                    f2 = Mathf.Clamp(f2, 0, clampOpacity);
                    Map.TerrainData.SetTerrainLayer(x, currentIndex, f2);

                }
            }
            else if (brush == -1)
            {
                for (int x = 0; x < 8; x++)
                {
                    if (x == brush) continue;
                    float f2 = Map.TerrainData.GetTerrainLayer_str(x, currentIndex);

                    f2 -= delta;
                    f2 = Mathf.Clamp(f2, 0, clampOpacity);
                    Map.TerrainData.SetTerrainLayer(x, currentIndex, f2);

                }
            }

            Map.TerrainData.SetTerrainLayer(brush, currentIndex, f);

         

        }

        private void FixedUpdate()
        {

        }

        public Vector2Int WorldPosToPixelPos(Vector3 worldPos)
        {
            Vector2Int pos_1 = new Vector2Int();
            pos_1.x = Mathf.RoundToInt(worldPos.x * 2);
            pos_1.y = Mathf.RoundToInt(worldPos.z * 2);

            return pos_1;
        }

        public int PixelPosToIndex(Vector2Int pos)
        {
            int index = (pos.y * 1024) + (pos.x);
            return index;
        }
    }


}