using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ProtoRTS.MapEditor
{
    public class MapTool_BrushTexture : MapToolScript
    {
        
        public enum Operation
        {
            None,
            Add,
            Subtract,
            Uniform
        }

        public enum BrushOps
        {
            Additive,
            Set
        }

        /// <summary>
        /// Minimum 256 x 256
        /// 
        /// </summary>
        public Texture2D[] brushes;
        public Operation currentOperation;

        [Range(0f,1f)] public float hardness = 0.9f;
        [Range(0, 255)] public int clampOpacity = 200;
        [Range(2, 32)] public int brushSize = 4;
        public int refreshRate = 5;
        [Range(-1,7)] public int brushCurrent = -1;
        [Range(5, 128)] public int brushStrength = 45;

        public bool isPerlinNoise = false;
        public bool isMaskByDistance = false;
        public int circle_cutoff = 30;
        [FoldoutGroup("Noise")] public float scale = 1.0F;
        [FoldoutGroup("Noise")] public float perlinMultiplier = 0.1f;


        [ShowInInspector] [ReadOnly] private Texture2D _noiseTex;
        private int splatmap_pixel = 1024;
        private int brush_noiseMapPixel = 64;
        private Color[] _pix;

        private float _refreshTime = 1f;
        private bool _allowMouseToEdit = false;


        private void Start()
        {
            _noiseTex = new Texture2D(brush_noiseMapPixel, brush_noiseMapPixel);
            _pix = new Color[brush_noiseMapPixel * brush_noiseMapPixel];
            GenerateNoise(0,0);
        }

        private void GenerateNoise(float xAxis, float yAxis)
        {
            for (float y = 0.0F; y < brush_noiseMapPixel; y++)
            {
                for (float x = 0.0F; x < brush_noiseMapPixel; x++)
                {
                    float xCoord = xAxis + x / brush_noiseMapPixel * scale;
                    float yCoord = yAxis + y / brush_noiseMapPixel * scale;
                    float sample = Mathf.PerlinNoise(xCoord, yCoord);
                    _pix[(int)y * brush_noiseMapPixel + (int)x] = new Color(sample, sample, sample);
                }
            }

            // Copy the pixel data to the texture and load it into the GPU.
            _noiseTex.SetPixels(_pix);
            _noiseTex.Apply();
        }


        private void Update()
        {
            _refreshTime -= Time.deltaTime;

            if (currentOperation != Operation.None)
            {
                Brush.EnableBrush();
                Brush.isCliff = false;
            }
            else
            {
                Brush.DisableBrush();
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (MainUI.GetEventSystemRaycastResults().Count == 0)
                {
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
                    Update_BrushingTexture();
                }

                _refreshTime = 1f / refreshRate;
            }

            if (Input.GetMouseButtonUp(0))
            {
                Map.UpdateTerrainMap();
            }

            if (isMaskByDistance)
            {
                Brush.targetShape = MapToolBrush.Shape.Circle;
                Brush.targetBrushSize = brushSize * 0.75f;
            }
            else
            {
                Brush.targetShape = MapToolBrush.Shape.Square;
                Brush.targetBrushSize = brushSize;
            }

        }



        private void Update_BrushingTexture()
        {
            //if brush size 1 = 2 splatmap pixel

            int pixelWidthBrush = brushSize * 2;
            int totalLength = (brushSize * 2) * (brushSize * 2);
            float halfSize = brushSize / 2f;
            Vector3 posOrigin = Brush.BrushPosition - new Vector3(halfSize, 0, halfSize);
            Vector2Int pixelPosCenter = WorldPosToPixelPos(Brush.BrushPosition);
            Vector2Int pixelPosOrigin = new Vector2Int();

            pixelPosOrigin = WorldPosToPixelPos(posOrigin);
            GenerateNoise(0, 0);

            int countDebug = 0;

            //had a more fundamental problem with pixel!
            for (int i = 0; i < totalLength; i++)
            {
                int x = i % pixelWidthBrush;
                int y = Mathf.FloorToInt(i / pixelWidthBrush);
                Vector2Int pixelPos = new Vector2Int((pixelPosOrigin.x + halfSize).ToInt(), (pixelPosOrigin.y + halfSize).ToInt());
                pixelPos.x += x/2;
                pixelPos.y += y/2;

                if (pixelPos.x >= splatmap_pixel) continue;
                if (pixelPos.x < 0) continue;
                if (pixelPos.y >= splatmap_pixel) continue;
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

                if (isPerlinNoise)
                {
                    Color colPixel = _pix[IndexOfBrush(x, y)];

                    float rawValue = strength_i * colPixel.r * (perlinMultiplier * 10f);
                    rawValue = Mathf.Clamp(rawValue, 0, 255);
                    strength_i = (byte)rawValue;
                }

                BrushTerrain(currentIndex, brushCurrent, strength_i, BrushOps.Additive);


            }

            //Map.UpdateTerrainMap();
            Map.PartialUpdateTerrainMap(pixelPosOrigin.x, pixelPosOrigin.y, pixelWidthBrush, pixelWidthBrush);

        }

        public int IndexOfBrush(int x, int y)
        {
            return x + (y * brush_noiseMapPixel);
        }

        public void BrushTerrain(int currentIndex, int brush, byte strength, BrushOps brushOps)
        {

            float delta = 1 + (brushStrength * 4f / refreshRate);
            float current = Map.TerrainData.GetTerrainLayer_str(brush, currentIndex);
            float target = Map.TerrainData.GetTerrainLayer_str(brush, currentIndex) + strength;

            if (brushOps == BrushOps.Set)
            {
                if (target > strength)
                {
                    target = strength;
                }
            }

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
            if (f >= clampOpacity)
            {
                for(int x = 0; x < 8; x++)
                {
                    if (x == brush)
                    {
                        continue;
                    }
                    float f2 = Map.TerrainData.GetTerrainLayer_str(x, currentIndex);

                    f2 -= (brushStrength * 4f / refreshRate);
                    f2 = Mathf.Clamp(f2, 0, 255);
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
                    f2 = Mathf.Clamp(f2, 0, 255);
                    Map.TerrainData.SetTerrainLayer(x, currentIndex, f2);

                }
            }

            Map.TerrainData.SetTerrainLayer(brush, currentIndex, f);

         

        }

        private void FixedUpdate()
        {

        }



        public override string GetBrushName()
        {
            return "Texture";
        }
    }


}