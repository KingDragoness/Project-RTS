using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;


namespace ProtoRTS
{

    [System.Serializable]
    public class CirclePixels
    {
        public string ID = "";
        [Range(2, 16)] public int LineOfSight = 2;
        public List<Vector2Int> coordToDraw = new List<Vector2Int>();
        public int[] coordToDraw_256px = new int[1];

        internal Map _map; //this is because cross memory!


        public int[] ConvertCoordToDrawToIndexes()
        {
            int[] ctd_intList = new int[coordToDraw.Count];

            for (int x = 0; x < ctd_intList.Length; x++)
            {
                int myIndex = coordToDraw[x].x + (coordToDraw[x].y * 256);

                ctd_intList[x] = myIndex;
            }

            coordToDraw_256px = ctd_intList;
            return ctd_intList;
        }

        //11/12/2024
        //Rather than creating new int array, directly write in the int[] indexes in the GetIndexes function
        //

        //if circle is 7x7 and loc origin is at 4,4
        //retrieve all indexes between 4,4 and 7,7
        public int[] GetLocalIndexes(Vector2Int localOrigin, Vector2Int sizeDraw)
        {
            int[] indexes = new int[sizeDraw.x * sizeDraw.y]; 
            int PixelLOS = LineOfSight * 2;

            int start_x = localOrigin.x;
            int start_y = localOrigin.y;
            int end_x = PixelLOS;
            int end_y = PixelLOS;

            if (localOrigin.x + sizeDraw.x < PixelLOS)
            {
                end_x = localOrigin.x + sizeDraw.x;
            }
            if (localOrigin.y + sizeDraw.y < PixelLOS)
            {
                end_y = localOrigin.y + sizeDraw.y;
            }

            int _id = 0;
            foreach (var ctd in coordToDraw_256px)
            {
                int y = FOWScript.X_table_indexes[ctd]; //(ctd / 256);
                int x = FOWScript.Y_table_indexes[ctd];//(ctd % 256);
                int myIndex = x + (y * 256);

                if (x > start_x && y > start_y && x < end_x && y < end_y)
                {
                    indexes[_id] = myIndex;
                    _id++;
                }
            }

            return indexes;
        }

        public int[] GetIndexes(Vector2Int originDraw, Vector2Int localOrigin, Vector2Int sizeDraw, int textureDimension = 256)
        {
            //every y, store indexes what X to draw. [2 * 256: + 0,1,2,3,4,5] [0 * 256: + 2,3,4]
            //IDEA only

            int[] indexes = new int[sizeDraw.x * sizeDraw.y];
            int startIndex = originDraw.x + (originDraw.y * 256);

            var localIndexes = GetLocalIndexes(localOrigin, sizeDraw);


            for (int c = 0; c < localIndexes.Length; c++)
            {
                int myIndex = startIndex + localIndexes[c];
                if (localIndexes[c] == 0) continue;
                if (myIndex < 0) continue;
                //int x = myIndex % 256;
                //int y = myIndex / 256;

                //DO NOT HEIGHTMAP CHECK HERE! 147k calls if there is ~500 units! More than 65336!
                //if (Map.TerrainData.cliffLevel.Length > myIndex)
                //{
                //    if (FOWScript.GetHeightmap[myIndex] - 128 > (unitYpos * 4)) continue; //-128 for sbyte 
                //}

                indexes[c] = myIndex;
            }

            {
                //        int indexCTD = 0;

                //for (int x = localOrigin.x; x < localOrigin.x + sizeDraw.x; x++)
                //{
                //    for (int y = localOrigin.y; y < localOrigin.y + sizeDraw.y; y++)
                //    {
                //        coordToDraw2[indexCTD].x = x;
                //        coordToDraw2[indexCTD].y = y;
                //        indexCTD++;
                //    }
                //}

                //foreach (var crd in coordToDraw2)
                //{
                //    //Debug.Log($"{crd}");
                //}

                //int startIndex = originDraw.x + (originDraw.y * 256);

                //for (int i = 0; i < indexes.Length; i++)
                //{
                //    if (coordToDraw.Contains(coordToDraw2[i]) == false) continue;

                //    int loc_x = originDraw.x + coordToDraw2[i].x;
                //    int loc_y = originDraw.y + coordToDraw2[i].y;
                //    int myIndex = loc_x + (loc_y * 256);

                //    indexes[i] = myIndex;
                //}
            }

            return indexes;
        }
    }

    public static class SomeFunctionForMap
    {
        public static Vector2Int ClampMapPixelPos(this Vector2Int pos)
        {
            return new Vector2Int(Mathf.Clamp(pos.x, 0, 255), Mathf.Clamp(pos.y, 0, 255));
        }
    }

    public class FOWScript : MonoBehaviour
    {

        [System.Serializable]
        public class FOWMap
        {
            public Unit.Player faction;
            public bool[,] activePoints;
            public bool[,] exploredPoints;
            public Texture2D rawDataTexture;
            public Texture2D nextTargetTexture;

            private Color32[] allColors;
            private Color32[] allColors_1;


            public FOWMap(Unit.Player faction, bool[,] activePoints, bool[,] exploredPoints)
            {
                this.faction = faction;
                this.activePoints = activePoints;
                this.exploredPoints = exploredPoints;
                rawDataTexture = new Texture2D(256, 256, TextureFormat.R8, false);
                rawDataTexture.name = $"RawFOW-{faction.ToString()}";
                nextTargetTexture = new Texture2D(256, 256, TextureFormat.R8, false);
                nextTargetTexture.name = $"ForTerrain_FOW-{faction.ToString()}";
                allColors = new Color32[65536];
                allColors_1 = new Color32[65536];
            }

            /// <summary>
            /// Don't generate texture FOR non-players! (Only main player)
            /// Because texture only used for graphics
            /// </summary>
            [Button("Generate Texture")]
            public void GenerateTexture()
            {
                int speedDeltaChangeTexture = 32;

                for (int x = 0; x < 255; x++)
                {
                    for (int y = 0; y < 255; y++)
                    {
                        int index = x + (y * 256);

                        if (activePoints[x, y])
                        {
                            allColors[index] = new Color32(255, 255, 255, 255);
                            allColors_1[index].r = (byte)Mathf.RoundToInt(Mathf.MoveTowards(allColors_1[index].r, 255, speedDeltaChangeTexture));
                        }
                        else if (exploredPoints[x, y])
                        {
                            allColors[index] = new Color32(38, 38, 38, 38);
                            allColors_1[index].r = (byte)Mathf.RoundToInt(Mathf.MoveTowards(allColors_1[index].r, 38, speedDeltaChangeTexture));
                        }
                        else
                        {
                            allColors[index] = new Color32(0, 0, 0, 0);
                            allColors_1[index].r = (byte)Mathf.RoundToInt(Mathf.MoveTowards(allColors_1[index].r, 0, speedDeltaChangeTexture));

                        }

                        activePoints[x, y] = false; //always reset
                    }
                }

                rawDataTexture.SetPixels32(allColors, 0);
                nextTargetTexture.SetPixels32(allColors_1, 0);
                rawDataTexture.Apply();
                nextTargetTexture.Apply();
            }

            [Button("Explore this point")]
            public void ExplorePoint(int x, int y)
            {
                activePoints[x, y] = true;
                exploredPoints[x, y] = true;
            }

            [Button("Explore this point")]
            public void ExplorePoint(int _index, System.Int16 unitPosY)
            {
                //check unit height
                if (Map.TerrainData.cliffLevel.Length > _index)
                {
                    if (FOWScript.GetHeightmap[_index] - 128 > (unitPosY * 4)) return; //-128 for sbyte 
                }

                int x = FOWScript.X_table_indexes[_index]; //_index % 256;
                int y = FOWScript.Y_table_indexes[_index]; //_index / 256;
                activePoints[x, y] = true;
                exploredPoints[x, y] = true;

                //Debug.Log($"Explore: ({x}, {y})");
            }

            public bool IsPointActive(int _index)
            {
                int x = FOWScript.X_table_indexes[_index]; //_index % 256;
                int y = FOWScript.Y_table_indexes[_index]; //_index / 256;

                return activePoints[x, y];
            }

            [Button("DEBUG explore some area")]
            public void DEBUG_ExplorePoint()
            {
                for (int x = 32; x < 90; x++)
                {
                    for (int y = 150; y < 250; y++)
                    {
                        activePoints[x, y] = true;
                    }
                }

                GenerateTexture();
            }
        }

        //FOW HEIGHTMAP
        //heightmap, 4 rgb = 1 height (256 / 4 = 64 total height)
        //y = 3 per cliff level (total cliff = 16 x 3)
        //cliff level [1] = 4 * 3 = 12
        //cliff level [2] = 4 * 6 = 24
        //y = 48 is the maximum

        public List<CirclePixels> FixedPatternCircles = new List<CirclePixels>();
        public Map map;
        [Space]
        [Header("References")]
        public List<FOWMap> allFOWMaps = new List<FOWMap>();
        public int UpdateTexturePerSecond = 30;

        [FoldoutGroup("DEBUG Perf")] public int Simulate_units = 100;
        [FoldoutGroup("DEBUG Perf")] public bool DEBUG_EnableSimulateUnitCall = false;

        private float _timerCooldown = 0.1f;
        private static FOWScript Instance;

        public static sbyte[] GetHeightmap
        {
            get
            {
                return Map.TerrainData.cliffLevel;
            }
        }

        [FoldoutGroup("FIXED Reference")] [SerializeField] public int[] x_table_indexes;
        [FoldoutGroup("FIXED Reference")] [SerializeField] public int[] y_table_indexes;

        public static int[] X_table_indexes { get => Instance.x_table_indexes; }
        public static int[] Y_table_indexes { get => Instance.y_table_indexes; }


        private void Awake()
        {
            Instance = this;
            _timerCooldown = 1f / UpdateTexturePerSecond;

        }

        private void Start()
        {
            allFOWMaps.Add(new FOWMap(Unit.Player.Player1, new bool[256, 256], new bool[256, 256]));
            allFOWMaps.Add(new FOWMap(Unit.Player.Player2, new bool[256, 256], new bool[256, 256]));
            allFOWMaps.Add(new FOWMap(Unit.Player.Player3, new bool[256, 256], new bool[256, 256]));
            allFOWMaps.Add(new FOWMap(Unit.Player.Player4, new bool[256, 256], new bool[256, 256]));
            allFOWMaps.Add(new FOWMap(Unit.Player.Player5, new bool[256, 256], new bool[256, 256]));
            allFOWMaps.Add(new FOWMap(Unit.Player.Player6, new bool[256, 256], new bool[256, 256]));
            allFOWMaps.Add(new FOWMap(Unit.Player.Player7, new bool[256, 256], new bool[256, 256]));
            allFOWMaps.Add(new FOWMap(Unit.Player.Player8, new bool[256, 256], new bool[256, 256]));

            foreach(var circle in FixedPatternCircles)
            {
                circle._map = map;
            }

            SetTerrainTexture(allFOWMaps[0]);
        }

        private void OnTick()
        {

            allFOWMaps[0].GenerateTexture();

            foreach (var unit in SyntiosEngine.Instance.ListedGameUnits)
            {
                DrawSquare(unit, allFOWMaps[0]);
            }

            if (DEBUG_EnableSimulateUnitCall)
            {
                for (int x = 0; x < Simulate_units; x++)
                {
                    DrawSquare(SyntiosEngine.Instance.ListedGameUnits[0], allFOWMaps[0]);
                }
            }
        }


        private void Update()
        {

            if (_timerCooldown > 0)
            {
                _timerCooldown -= Time.deltaTime;
            }
            else
            {
                OnTick();
                _timerCooldown = 1f / UpdateTexturePerSecond;
            }

            //foreach (var fowMap in allFOWMaps)
            //{
            //    fowMap.GenerateTexture();
            //}

        }


        private void DrawSquare(GameUnit unit, FOWMap fowMap)
        {
            CirclePixels myCirclePattern = GetCirclePixel(unit.Class.LineOfSight);
            if (myCirclePattern == null) return;

            int LineOfSight = unit.Class.LineOfSight * 2;
            int half1 = LineOfSight / 2;

            Vector2Int centerPos = WorldPosToMapPixel(unit.transform.position);
            Vector2Int originPos = new Vector2Int(centerPos.x - half1, centerPos.y - half1);
            Vector2Int leftLower = originPos.ClampMapPixelPos();
            Vector2Int rightUpper = new Vector2Int(originPos.x + LineOfSight, originPos.y + LineOfSight).ClampMapPixelPos();

            {
                //int x_draw = unit.lineOfSight;
                //int y_draw = unit.lineOfSight;

                //if (originPos.x + half1 > 255)
                //{
                //    x_draw = Mathf.Clamp(255 - originPos.x, 0, 255);
                //}
                //if (originPos.y + half1 > 255)
                //{
                //    y_draw = Mathf.Clamp(255 - originPos.y, 0, 255);
                //}

                //for(int x = 0; )

                //int[] indexesToDraw = new int[totalPixelsCovered];
            }
            int index1 = 0;
            int[] points_rect = new int[4];
            points_rect[0] = leftLower.x; //min x
            points_rect[1] = leftLower.y; //min y
            points_rect[2] = rightUpper.y;  //max y
            points_rect[3] = rightUpper.x; //max x

            int xDrawLength = points_rect[3] - points_rect[0];
            int yDrawLength = points_rect[2] - points_rect[1];
            int startDrawCircle_x = 0;
            int startDrawCircle_y = 0;

            if (originPos.x < 0) startDrawCircle_x = LineOfSight - xDrawLength; //based from circle texture coord
            if (originPos.y < 0) startDrawCircle_y = LineOfSight - yDrawLength; //based from circle texture coord

            //Debug.Log($"Rect: ({points_rect[0]} < {points_rect[3]} | {points_rect[1]} < {points_rect[2]}) = Draw: ({xDrawLength}, {yDrawLength}) | Origin pattern: ({startDrawCircle_x}, {startDrawCircle_y})");
            int _index = 0;

            var indexesToDraw = myCirclePattern.GetIndexes(originPos, new Vector2Int(startDrawCircle_x, startDrawCircle_y), new Vector2Int(xDrawLength, yDrawLength));

            foreach (var pxToDraw in indexesToDraw)
            {
                if (fowMap.IsPointActive(pxToDraw) == true) continue;
                fowMap.ExplorePoint(pxToDraw, unitPosY: (short)unit.transform.position.y);
            }

            {
                //for (int x = points_rect[0]; x < points_rect[3]; x++)
                //{
                //    for (int y = points_rect[1]; y < points_rect[2]; y++)
                //    {
                //        int index = x + (y * 256);
                //        fowMap.ExplorePoint(index);
                //        _index++;
                //    }
                //}
            }



            {
                //for (int x = originPos.x; x < (originPos.x + unit.lineOfSight); x++)
                //{
                //    if (x < 0) continue;
                //    if (x > 255) continue;

                //    for (int y = originPos.y; y < (originPos.y + unit.lineOfSight); y++)
                //    {
                //        if (y < 0) continue;
                //        if (y > 255) continue;

                //        int index = x + (y * 256);

                //        //indexesToDraw[index1] = index;
                //        fowMap.ExplorePoint(index);
                //        index1++;
                //    }
                //}
            }
        }

        [FoldoutGroup("DEBUG")]
        [Button("Set Terrain Texture")]
        public void DEBUG_TestTerrainSet()
        {
            SetTerrainTexture(allFOWMaps[0]);
        }

        [FoldoutGroup("DEBUG")]
        [Button("Convert CirclePos to Index")]
        public void DEBUG_CirclePosToIndex256()
        {
            foreach (var circle in FixedPatternCircles)
            {
                circle.ConvertCoordToDrawToIndexes();
            }
        }

        [FoldoutGroup("DEBUG")]
        [Button("Create indexer table")]
        public void DEBUG_CreateTableIndex()
        {
            x_table_indexes = new int[65536];
            y_table_indexes = new int[65536];

            for (int x = 0; x < 255; x++)
            {
                for (int y = 0; y < 255; y++)
                {
                    int index = x + (y * 256);

                    x_table_indexes[index] = x;
                    y_table_indexes[index] = y;

                }
            }

            Debug.Log($"{x_table_indexes.GetLength(0)} | {y_table_indexes[64004]}");
        }


        public void SetTerrainTexture(FOWMap fowMapTarget)
        {
            Map.instance.Material.SetTexture("_FOWMap", fowMapTarget.nextTargetTexture);
        }

        public Vector2Int WorldPosToMapPixel(Vector3 worldPos)
        {
            Vector2Int pos_1 = new Vector2Int();
            pos_1.x = Mathf.FloorToInt(worldPos.x / 2);
            pos_1.y = Mathf.FloorToInt(worldPos.z / 2);

            return pos_1;
        }

        public CirclePixels GetCirclePixel(int viewRange)
        {
            return FixedPatternCircles.Find(x => x.LineOfSight == viewRange);
        }

        public int PixelPosToIndex(int x, int y)
        {
            return x + (y * 256);
        }

        //unused, only for testing
        public int[] GetAllIndexesInsideBox(Vector2Int pos, int sizeX, int sizeY)
        {
            int[] indexesToDraw = new int[sizeX * sizeY];
            int _index = 0;

            for (int y = 0; y < sizeY; y++)
            {
                for (int x = 0; x < sizeX; x++)
                {
                    int index = x + (y * 256);

                    indexesToDraw[_index] = index;
                    _index++;
                }
            }

            return indexesToDraw;
        }

        public int PixelPosToIndex(Vector2Int pos, bool clampable)
        {
            if (pos.x < 0 && clampable == false) return -1;
            if (pos.y < 0 && clampable == false) return -1;
            if (pos.x > 255 && clampable == false) return -1;
            if (pos.y > 255 && clampable == false) return -1;

            if (clampable && (pos.x < 0 && pos.x > 255) && (pos.y < 0 && pos.y > 255))
            {
                int index = (pos.y * 255) + (pos.x);
                return index;
            }
            else
            {
                return -1;
            }
        }

    }
}
