using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ProtoRTS.MapEditor
{
    public class MapTool_BrushTexture : MapToolScript
    {

        public int brushSize = 4;
        public int refreshRate = 5;
        [Range(-1,7)] public int brushCurrent = -1;

        [Header("References")]
        public Transform brushProjector;
        public Transform DEBUG_originBrush;
        public Projector projector;

        private Vector3 brushPosition = new Vector3();
        private float _refreshTime = 1f;

        private void Update()
        {
            _refreshTime -= Time.deltaTime;
            projector.orthographicSize = brushSize;

            Update_BrushProjector();

            if (_refreshTime <= 0f)
            {
                if (Input.GetMouseButton(0) && MainUI.GetEventSystemRaycastResults().Count == 0)
                {
                    Update_BrushingTexture();
                }

                _refreshTime = 1f / refreshRate;
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
            int halfSize = brushSize;
            Vector3 posOrigin = brushPosition - new Vector3Int(halfSize, 0, halfSize);
            Vector2Int pixelPosOrigin = new Vector2Int();

            pixelPosOrigin = WorldPosToPixelPos(posOrigin);
            DEBUG_originBrush.transform.position = posOrigin;

            for (int i = 0; i < totalLength; i++)
            {
                int x = i % pixelWidthBrush;
                int y = Mathf.FloorToInt(i / pixelWidthBrush);
                Vector2Int pixelPos = new Vector2Int(pixelPosOrigin.x + halfSize, pixelPosOrigin.y + halfSize);
                pixelPos.x += x;
                pixelPos.y += y;

                if (pixelPos.x >= 1024) continue;
                if (pixelPos.x < 0) continue;
                if (pixelPos.y >= 1024) continue;
                if (pixelPos.y < 0) continue;

                int currentIndex = PixelPosToIndex(pixelPos);
                BrushTerrain(currentIndex, brushCurrent, 255);
            }

            Map.UpdateTerrainMap();

        }

        public void BrushTerrain(int currentIndex, int brush, byte strength)
        {
            Map.TerrainData.terrain_layer1[currentIndex] = 0;
            Map.TerrainData.terrain_layer2[currentIndex] = 0;
            Map.TerrainData.terrain_layer3[currentIndex] = 0;
            Map.TerrainData.terrain_layer4[currentIndex] = 0;
            Map.TerrainData.terrain_layer5[currentIndex] = 0;
            Map.TerrainData.terrain_layer6[currentIndex] = 0;
            Map.TerrainData.terrain_layer7[currentIndex] = 0;
            Map.TerrainData.terrain_layer8[currentIndex] = 0;

            if (brush == -1)
            {
                
            }
            if (brush == 0)
            {
                Map.TerrainData.terrain_layer1[currentIndex] = strength;
            }
            if (brush == 1)
            {
                Map.TerrainData.terrain_layer2[currentIndex] = strength;
            }
            if (brush == 2)
            {
                Map.TerrainData.terrain_layer3[currentIndex] = strength;
            }
            if (brush == 3)
            {
                Map.TerrainData.terrain_layer4[currentIndex] = strength;
            }
            if (brush == 4)
            {
                Map.TerrainData.terrain_layer5[currentIndex] = strength;
            }
            if (brush == 5)
            {
                Map.TerrainData.terrain_layer6[currentIndex] = strength;
            }
            if (brush == 6)
            {
                Map.TerrainData.terrain_layer7[currentIndex] = strength;
            }
            if (brush == 7)
            {
                Map.TerrainData.terrain_layer8[currentIndex] = strength;
            }
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