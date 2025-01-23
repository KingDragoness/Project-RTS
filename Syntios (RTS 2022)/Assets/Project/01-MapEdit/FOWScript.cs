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

        //X = 4 | {1,2,3,4,5}
        //X = 5 | {2,3,4}
        public bool[,] coordInstructions;

        internal Map _map; //this is because cross memory!
        private int[] _cachedIndexes;
        private int _maxCount;

        public int MaxCount { get => _maxCount; }
        public int[] CachedIndexes { get => _cachedIndexes; }

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

        public bool[,] DEBUG_CreateCoordInstructions() 
        {
            bool[,] new_coordInstructions = new bool[LineOfSight * LineOfSight, LineOfSight * LineOfSight];

            foreach(var coord in coordToDraw)
            {
                new_coordInstructions[coord.x, coord.y] = true;
            }

            return new_coordInstructions;
        }

        private int[] cached_xTable;
        private int[] cached_yTable;

        //if circle is 7x7 and loc origin is at 4,4
        //retrieve all indexes between 4,4 and 7,7
        public void GetLocalIndexes(Vector2Int localOffset, Vector2Int sizeDraw)
        {
            int PixelLOS = LineOfSight * 2;

            if (cached_xTable == null) cached_xTable = FOWScript.X_table_indexes;
            if (cached_yTable == null) cached_yTable = FOWScript.Y_table_indexes;

            if (_cachedIndexes == null)
            {
                _cachedIndexes = new int[32 * 32];
            }
            else
            {
                System.Array.Clear(_cachedIndexes, 0, _cachedIndexes.Length);
            }

            int start_x = localOffset.x;
            int start_y = localOffset.y;
            int end_x = PixelLOS;
            int end_y = PixelLOS;

            if (localOffset.x + sizeDraw.x < PixelLOS)
            {
                end_x = localOffset.x + sizeDraw.x;
            }
            if (localOffset.y + sizeDraw.y < PixelLOS)
            {
                end_y = localOffset.y + sizeDraw.y;
            }

            int _id = 0;
            foreach (var ctd in coordToDraw_256px)
            {
                int y = cached_xTable[ctd]; //(ctd / 256);
                int x = cached_yTable[ctd];//(ctd % 256);
                int myIndex = x + (y * 256);

                if (x > start_x && y > start_y && x < end_x && y < end_y)
                {
                    _cachedIndexes[_id] = myIndex;
                    _id++;
                }
            }

            _maxCount = _id + 1;
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

            //cached
            int cachedStartIndex = 0; //the one with max size 256
            int cachedStartIndex_cliffmap = 0; //the one with variable size
            int ch_x = 0;
            int ch_y = 0;
            int[] cached_LocalIndexes;

            int[] cached_xTable;
            int[] cached_yTable;

            private int[] X_Table
            {
                get
                {
                    if (cached_xTable == null) cached_xTable = FOWScript.X_table_indexes;
                    return cached_xTable;
                }
            }

            private int[] Y_Table
            {
                get
                {
                    if (cached_yTable == null) cached_yTable = FOWScript.Y_table_indexes;
                    return cached_yTable;
                }
            }

            public FOWMap(Unit.Player faction, bool[,] activePoints, bool[,] exploredPoints)
            {
                this.faction = faction;
                this.activePoints = activePoints;
                this.exploredPoints = exploredPoints;
                rawDataTexture = new Texture2D(256, 256, TextureFormat.R8, false);
                rawDataTexture.filterMode = FilterMode.Point;
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
                byte fowCol_Explored = 30;
                byte byte_Delta = 19;

                for (int x = 0; x < 255; x++)
                {
                    for (int y = 0; y < 255; y++)
                    {
                        byte_Delta = 19;
                        int index = x + (y * 256);

                        if (activePoints[x, y])
                        {
                            allColors[index].r = 255;
                            allColors[index].g = 255;
                            allColors[index].b = 255;
                            allColors[index].a = 5;


                            if (allColors_1[index].r < 255)
                            {
                                if ((byte_Delta + allColors_1[index].r) > 255)
                                    byte_Delta = System.Convert.ToByte(255 - allColors_1[index].r);

                                allColors_1[index].r += byte_Delta;
                            } 

                            //allColors_1[index].r = (byte)Mathf.RoundToInt(Mathf.MoveTowards(allColors_1[index].r, 255, speedDeltaChangeTexture));
                        }
                        else if (exploredPoints[x, y])
                        {
                            allColors[index].r = fowCol_Explored;
                            allColors[index].g = fowCol_Explored;
                            allColors[index].b = fowCol_Explored;
                            allColors[index].a = 100;


                            if (allColors_1[index].r < fowCol_Explored)
                            {
                                if ((allColors_1[index].r + byte_Delta) > fowCol_Explored)
                                    byte_Delta = System.Convert.ToByte(fowCol_Explored - allColors_1[index].r);

                                allColors_1[index].r += byte_Delta;
                            }
                            else if (allColors_1[index].r > fowCol_Explored)
                            {
                                if ((allColors_1[index].r - byte_Delta) < fowCol_Explored)
                                    byte_Delta = System.Convert.ToByte(allColors_1[index].r - fowCol_Explored);

                                allColors_1[index].r -= byte_Delta;
                            }
                            

                            //allColors_1[index].r = (byte)Mathf.RoundToInt(Mathf.MoveTowards(allColors_1[index].r, 38, speedDeltaChangeTexture));
                        }
                        else
                        {
                            allColors[index].r = 0;
                            allColors[index].g = 0;
                            allColors[index].b = 0;
                            allColors[index].a = 245;

                            if (allColors_1[index].r > 0)
                            {
                                if ((allColors_1[index].r - byte_Delta) < 0)
                                    byte_Delta = System.Convert.ToByte(allColors_1[index].r);

                                allColors_1[index].r -= byte_Delta;
                            }

                            //allColors_1[index].r = (byte)Mathf.RoundToInt(Mathf.MoveTowards(allColors_1[index].r, 0, speedDeltaChangeTexture));

                        }

                    }
                }

                rawDataTexture.SetPixels32(allColors, 0);
                nextTargetTexture.SetPixels32(allColors_1, 0);
                rawDataTexture.Apply();
                nextTargetTexture.Apply();
            }


            public void FlushActive()
            {
                System.Array.Clear(activePoints, 0, activePoints.Length);
            }


            public void WriteBuffer(Vector2Int globalOffset, Vector2Int localOrigin, Vector2Int sizeDraw, CirclePixels circlePixel, float posYUnit)
            {
                //every y, store indexes what X to draw. [2 * 256: + 0,1,2,3,4,5] [0 * 256: + 2,3,4]
                //IDEA only

                cachedStartIndex = globalOffset.x + (globalOffset.y * 256);
                circlePixel.GetLocalIndexes(localOrigin, sizeDraw);
                cached_LocalIndexes = circlePixel.CachedIndexes;

                int x_center = globalOffset.x + (sizeDraw.x / 2);
                int y_center = globalOffset.y + (sizeDraw.y / 2);

                int maxCount = circlePixel.MaxCount;
                int myIndex = 0;

                for (int c = 0; c < maxCount; c++)
                {
                    myIndex = cachedStartIndex + cached_LocalIndexes[c];
                    if (cached_LocalIndexes[c] == 0) continue;
                    if (myIndex < 0) continue;

                    ch_x = X_Table[myIndex];
                    ch_y = Y_Table[myIndex];

                    //if already written, skips
                    if (activePoints[ch_x, ch_y]) continue;

                    int index_cliffmap = ch_x + (ch_y * Map.TerrainData.size_x);


                    if (Map.TerrainData.cliffLevel.Length > index_cliffmap)
                    {
                        if (FOWScript.GetCliffLV[index_cliffmap] * 4f >= posYUnit + 0.5f)
                        {
                            continue;
                        }
                    }

                    //occlusion cliffmap
                    {
                        bool isBlocked = IsBlocked(ch_x, ch_y, x_center, y_center, posYUnit);

                        //if (c == 2 | c == 3) Debug.Log($"{isBlocked} ({ch_x}, {ch_y})");

                        if (isBlocked) continue;
                    }
               
                    activePoints[ch_x, ch_y] = true;
                    exploredPoints[ch_x, ch_y] = true;

                    //ExplorePoint(myIndex, unitPosY: 1);
                }
            }

            public bool IsBlocked(int x_current, int y_current, int x_center, int y_center, float posYUnit)
            {
                int x_sample1 = Mathf.Lerp(x_current, x_center, 0.2f).ToInt(); //x_current + (x_center - x_current) / 2;
                int y_sample1 = Mathf.Lerp(y_current, y_center, 0.2f).ToInt(); //y_current + (y_center - y_current) / 2;

                int x_sample2 = Mathf.Lerp(x_current, x_center, 0.8f).ToInt();
                int y_sample2 = Mathf.Lerp(y_current, y_center, 0.8f).ToInt();

                int x_sample3 = Mathf.Lerp(x_current, x_center, 0.5f).ToInt();
                int y_sample3 = Mathf.Lerp(y_current, y_center, 0.5f).ToInt();


                int index_cliffmap1 = x_sample1 + (y_sample1 * Map.TerrainData.size_x);
                int index_cliffmap2 = x_sample2 + (y_sample2 * Map.TerrainData.size_x);
                int index_cliffmap3 = x_sample3 + (y_sample3 * Map.TerrainData.size_x);

                if (Map.TerrainData.cliffLevel.Length > index_cliffmap1 && index_cliffmap1 > 0)
                {
                    if (FOWScript.GetCliffLV[index_cliffmap1] * 4f >= posYUnit + 0.5f)
                    {
                        return true;
                    }
                }

                if (Map.TerrainData.cliffLevel.Length > index_cliffmap2 && index_cliffmap2 > 0)
                {
                    if (FOWScript.GetCliffLV[index_cliffmap2] * 4f >= posYUnit + 0.5f)
                    {
                        return true;
                    }
                }

                if (Map.TerrainData.cliffLevel.Length > index_cliffmap3 && index_cliffmap3 > 0)
                {
                    if (FOWScript.GetCliffLV[index_cliffmap3] * 4f >= posYUnit + 0.5f)
                    {
                        return true;
                    }
                }

                return false;
            }

            [Button("Explore this point")]
            public void ExplorePoint(int x, int y)
            {
                activePoints[x, y] = true;
                exploredPoints[x, y] = true;
            }


            //TOO EXPENSIVE
            [Button("Explore this point")]
            public void ExplorePoint(int _index, System.Int16 unitPosY)
            {
                //check unit height
                if (Map.TerrainData.cliffLevel.Length > _index)
                {
                    if (FOWScript.GetCliffLV[_index] - 128 > (unitPosY * 4)) return; //-128 for sbyte 
                }

                ch_x = X_Table[_index];
                ch_y = Y_Table[_index];
                activePoints[ch_x, ch_y] = true;
                exploredPoints[ch_x, ch_y] = true;

                //Debug.Log($"Explore: ({x}, {y})");
            }

            public bool IsPointActive(int _index)
            {
                int x = X_Table[_index]; //_index % 256;
                int y = Y_Table[_index]; //_index / 256;

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
        public Texture2D texture2d_WhiteFOW;
        public int UpdateTexturePerSecond = 30;
        public int UpdateFOWPerSecond = 10;

        [FoldoutGroup("DEBUG Perf")] public int Simulate_units = 100;
        [FoldoutGroup("DEBUG Perf")] public bool DEBUG_EnableSimulateUnitCall = false;

        private float _timerUpdateTexture = 0.1f;
        private float _timerUpdateFOW = 0.1f;
        private static FOWScript Instance;

        public static byte[] GetCliffLV
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

        public static FOWMap GetFOW(Unit.Player faction)
        {
            var fowMap = Instance.allFOWMaps.Find(x => x.faction == faction);
            return fowMap;
        }


        private void Awake()
        {
            Instance = this;
            _timerUpdateTexture = 1f / UpdateTexturePerSecond;
            _timerUpdateFOW = 1f / UpdateFOWPerSecond;

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

        }

        private void UpdateTexture()
        {
            if (SyntiosEngine.Instance.CurrentGamemode == Gamemode.Game)
            {
                SetTerrainTexture(GetFOW(SyntiosEngine.CurrentFaction));

                foreach (var fowMap in allFOWMaps)
                {
                    fowMap.GenerateTexture();
                }
            }
            else if (SyntiosEngine.Instance.CurrentGamemode == Gamemode.MapEdit)
            {
                Shader.SetGlobalTexture("_FOWMap", texture2d_WhiteFOW);

            }
        }

        private void UpdateFOW()
        {

            foreach (var fowMap in allFOWMaps)
            {
                fowMap.FlushActive();
            }
            foreach (var unit in SyntiosEngine.Instance.ListedGameUnits)
            {
                DrawSquare(unit, GetFOW(unit.stat_faction));
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

            if (_timerUpdateFOW > 0)
            {
                _timerUpdateFOW -= Time.deltaTime;
            }
            else
            {
                UpdateFOW();
                _timerUpdateFOW = 1f / UpdateFOWPerSecond;
            }

            if (_timerUpdateTexture > 0)
            {
                _timerUpdateTexture -= Time.deltaTime;
            }
            else
            {
                UpdateTexture();
                _timerUpdateTexture = 1f / UpdateTexturePerSecond;
            }


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

            fowMap.WriteBuffer(originPos, new Vector2Int(startDrawCircle_x, startDrawCircle_y), new Vector2Int(xDrawLength, yDrawLength), myCirclePattern, unit.transform.position.y);


            //foreach (var pxToDraw in indexesToDraw)
            //{
            //    fowMap.ExplorePoint(pxToDraw, unitPosY: (short)unit.transform.position.y);
            //}

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
        [Button("Convert bool instructions")]
        public void DEBUG_CircleInstructions()
        {
            foreach (var circle in FixedPatternCircles)
            {
                circle.DEBUG_CreateCoordInstructions();
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
            //Map.instance.Material.SetTexture("_FOWMap", fowMapTarget.nextTargetTexture);
            Shader.SetGlobalTexture("_FOWMap", fowMapTarget.nextTargetTexture);
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
