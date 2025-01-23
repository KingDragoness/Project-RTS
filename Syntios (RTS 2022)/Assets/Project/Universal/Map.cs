using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Unity.Collections;
using Sirenix.OdinInspector;
using Pathfinding;
using Newtonsoft.Json;
using ReadOnlyAttribute = Sirenix.OdinInspector.ReadOnlyAttribute;

namespace ProtoRTS
{
    public class Map : MonoBehaviour
    {

        public class CliffObjectDat
        {
            public Vector2Int pos;
            public SO_TerrainPreset.Tileset[] tileset;
            public GameObject[] allCliffs = new GameObject[1];

            public CliffObjectDat(Vector2Int pos, SO_TerrainPreset.Tileset tileset)
            {
                this.pos = pos;
                this.tileset = new SO_TerrainPreset.Tileset[1] { tileset };
            }

            public bool ExactSameSet(SO_TerrainPreset.Tileset[] toCompare)
            {
                if (toCompare.Length != tileset.Length) return false;

                int i = 0;
                foreach(var tile in tileset)
                {
                    if (tile != toCompare[i]) return false;
                    i++;
                }

                return true;
            }

            public void WipeCliffs()
            {
                foreach(var cliff in allCliffs)
                {
                    if (cliff != null)
                        Destroy(cliff.gameObject);
                }
            }
        }

        public List<SO_TerrainPreset> allVanillaTerrainPresets = new List<SO_TerrainPreset>();
        [Space]
        [SerializeField] private SyntiosTerrainData _terrainData;

        [Header("Maps")]
        [FoldoutGroup("DEBUG")] public bool DEBUG_dontInitializeData;
        [FoldoutGroup("DEBUG")] public bool DEBUG_autoUpdateOnStart = true; //disabled on map editor
        [FoldoutGroup("DEBUG")] public bool DEBUG_ShowNoNeighborCheck = false;
        [FoldoutGroup("DEBUG")] public bool DEBUG_SeeCliff = false;
        [FoldoutGroup("DEBUG")] public GameObject DEBUGObject_NoNeighborCheck;
        [FoldoutGroup("DEBUG")] private int DEBUG_lastSecondChecked = 0;
        [FoldoutGroup("DEBUG")] [ShowInInspector] [ReadOnly] private CliffObjectDat[] allCliffObjectResultData;

        [FoldoutGroup("References")] public AstarPath aStarPath;
        [FoldoutGroup("References")] public MeshRenderer DEBUG_MeshTerrain;
        [FoldoutGroup("References")] [SerializeField] private Shader terrainShader;
        [FoldoutGroup("References")] [SerializeField] private Material _sourceTerrainMat;


        [DisableInEditorMode] [SerializeField] private Material generatedTerrainMaterial;
        private Transform terrainParent;
        [SerializeField] [ReadOnly] private Texture2D generatedSplatmap;
        [SerializeField] [ReadOnly] private Texture2D generatedSplatmap2;
        private Color32[] color_splat1 = new Color32[0];
        private Color32[] color_splat2 = new Color32[0];

        public static readonly string MapEditorPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "/My Games/Syntios/_protoTerrain";


        [ShowInInspector] List<CliffObjectDat> vd3 = new List<CliffObjectDat>();

        /// <summary>
        /// Retrieve the generated material terrain shader.
        /// </summary>
        public Material Material { get => generatedTerrainMaterial; }

        public static Vector2 MapSize
        {
            get
            {
                return new Vector2(TerrainData.size_x, TerrainData.size_y);
            }
        }

        public static Vector3 WorldPosCenter
        {
            get
            {
                return new Vector3(TerrainData.size_x, 0, TerrainData.size_y);
            }
        }


        public static SyntiosTerrainData TerrainData
        {
            get { return instance._terrainData; }
        }


        public SO_TerrainPreset MyPreset
        {
            get
            {
                return allVanillaTerrainPresets.Find(x => x.PresetID == _terrainData.ID);
            }
        }



        public static Map instance;

        private void Awake()
        {
            instance = this;
            terrainParent = new GameObject().transform;
            terrainParent.gameObject.name = "TerrainMesh";
        }

        private void Start()
        {
            BootMap_Start();
            if (DEBUG_dontInitializeData == false) _terrainData.InitializeData();
            GenerateMaterial();

            if (DEBUG_autoUpdateOnStart)
            {
                UpdateTerrainMap();
                UpdateCliffMap();
            }

            UpdateNavMesh();
            SyntiosEvents.Game_ReloadMap?.Invoke();
        }

        public static string lastLoadedValidmap = "";

        #region Map Loading
        public void LoadMapFromProtoDir(string mapName)
        {
            var loadedTerrainDat = UnpackTerrainDat(MapEditorPath + $"/{mapName}.map");

            if (loadedTerrainDat == null)
            {
                throw new System.Exception("Map loading failed!");
            }

            _terrainData = loadedTerrainDat;
            lastLoadedValidmap = mapName;
            //Reload cliff results
            {
                if (allCliffObjectResultData != null)
                {
                    for (int x = 0; x < allCliffObjectResultData.Length; x++)
                    {
                        allCliffObjectResultData[x].WipeCliffs();
                    }
                }

                allCliffObjectResultData = new CliffObjectDat[_terrainData.size_x * _terrainData.size_y];

                int index = 0;
                Vector2Int cliffmapPos = new Vector2Int();

                for (int x = 0; x < _terrainData.size_x; x++)
                {
                    for (int y = 0; y < _terrainData.size_y; y++)
                    {
                        cliffmapPos = new Vector2Int(x, y);
                        allCliffObjectResultData[index] = new CliffObjectDat(cliffmapPos, SO_TerrainPreset.Tileset.Null);
                        index++;
                    }
                }

            }

            GenerateMaterial();
            UpdateTerrainMap();
            UpdateCliffMap();
            StartCoroutine(RETARD_UpdateNavmesh());
            SyntiosEvents.Game_ReloadMap?.Invoke();
        }

        IEnumerator RETARD_UpdateNavmesh()
        {
            yield return null;
            UpdateNavMesh();
        }

        public SyntiosTerrainData UnpackTerrainDat(string path)
        {
            SyntiosTerrainData result = null;
            JsonSerializerSettings settings = JsonSettings();

            try
            {
                result = JsonConvert.DeserializeObject<SyntiosTerrainData>(File.ReadAllText(path), settings);
            }
            catch
            {
                Debug.LogError("Failed load!");
                DevConsole.Instance.SendConsoleMessage("Map file cannot be loaded!");
            }

            return result;
        }

        public static JsonSerializerSettings JsonSettings()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            return settings;

        }

        public void UpdateNavMesh()
        {

            var recastGraph = AstarPath.active.data.recastGraph;
            int width = (_terrainData.size_x * 2) / 1;
            int depth = (_terrainData.size_y * 2) / 1;

            var center = WorldPosCenter;
            center.x += 0;
            center.z += 0;

            recastGraph.cellSize = 1;
            recastGraph.forcedBoundsSize = new Vector3(width, 100, depth);
            recastGraph.forcedBoundsCenter = center;
            //gridGraph.SetDimensions(width, depth, 1);
            //gridGraph.center = center;

            AstarPath.active.Scan(recastGraph);
        }

        public float GetPositionY(Vector3 realpos)
        {
            var recastGraph = AstarPath.active.data.recastGraph;
            var nn = recastGraph.GetNearest(realpos, constraint: NNConstraint.Walkable);
            return nn.position.y;
        }

        private void BootMap_Start()
        {
            var textureSplatTest = _sourceTerrainMat.GetTexture("_SplatMap");
            var textureSplatTest2 = _sourceTerrainMat.GetTexture("_SplatMap2");

            Shader.SetGlobalTexture("_SplatMap", textureSplatTest);
            Shader.SetGlobalTexture("_SplatMap2", textureSplatTest2);

            if (allCliffObjectResultData != null)
            {
                for (int x = 0; x < allCliffObjectResultData.Length; x++)
                {
                    allCliffObjectResultData[x].WipeCliffs();
                }
            }

            allCliffObjectResultData = new CliffObjectDat[_terrainData.size_x * _terrainData.size_y];

            int index = 0;
            Vector2Int cliffmapPos = new Vector2Int();

            for (int x = 0; x < _terrainData.size_x; x++)
            {
                for (int y = 0; y < _terrainData.size_y; y++)
                {
                    cliffmapPos = new Vector2Int(x, y);
                    allCliffObjectResultData[index] = new CliffObjectDat(cliffmapPos, SO_TerrainPreset.Tileset.Null);
                    index++;
                }
            }


        }


        private void GenerateMaterial()
        {
            generatedTerrainMaterial = new Material(_sourceTerrainMat);
            generatedTerrainMaterial.name = "GeneratedTerrainMat";


            generatedTerrainMaterial.SetTexture("_GroundTexture", MyPreset.ground);
            generatedTerrainMaterial.SetTexture("_TextureA", MyPreset.layer1);
            generatedTerrainMaterial.SetTexture("_TextureB", MyPreset.layer2);
            generatedTerrainMaterial.SetTexture("_TextureC", MyPreset.layer3);
            generatedTerrainMaterial.SetTexture("_TextureD", MyPreset.layer4);
            generatedTerrainMaterial.SetTexture("_TextureE", MyPreset.layer5);
            generatedTerrainMaterial.SetTexture("_TextureF", MyPreset.layer6);
            generatedTerrainMaterial.SetTexture("_TextureG", MyPreset.layer7);
            generatedTerrainMaterial.SetTexture("_TextureH", MyPreset.layer8);
            generatedTerrainMaterial.SetVector("_MapSize", new Vector4(_terrainData.size_x, _terrainData.size_y));
            DEBUG_MeshTerrain.material = generatedTerrainMaterial;

            //set global
            {
                float f_SplatmapScale = _sourceTerrainMat.GetFloat("_SplatmapScale");
                float f_FOWmapScale = _sourceTerrainMat.GetFloat("_FOWmapScale");
                float f_FOWSampleRadiusBlur = _sourceTerrainMat.GetFloat("_FOWSampleRadiusBlur");
                float f_border = _sourceTerrainMat.GetFloat("_BorderMap");
                var cloudTexture = _sourceTerrainMat.GetTexture("_CloudFog");
                Shader.SetGlobalFloat("_SplatmapScale", f_SplatmapScale);
                Shader.SetGlobalFloat("_FOWmapScale", f_FOWmapScale);
                Shader.SetGlobalFloat("_FOWSampleRadiusBlur", f_FOWSampleRadiusBlur);
                Shader.SetGlobalFloat("_BorderMap", f_border);
                Shader.SetGlobalTexture("_CloudFog", cloudTexture);
                Shader.SetGlobalVector("_MapSize", new Vector4(_terrainData.size_x, _terrainData.size_y));

            }
        }

        #endregion


        #region Generate Terrain

        public static void UpdateTerrainMap()
        {
            instance._updateTerrainMap();
        }

        public static void PartialUpdateTerrainMap(int x, int y, int width, int length)
        {
            instance._partialUpdateMap(x, y, width, length);
        }



        [FoldoutGroup("DEBUG")]
        [Button("Update terrain map")]

        //Very expensive operation, avoid this at all cost
        private void _updateTerrainMap()
        {
            if (generatedSplatmap == null)
            {
                generatedSplatmap = new Texture2D(256 * 4, 256 * 4, TextureFormat.RGBA32, false);
                Shader.SetGlobalTexture("_SplatMap", generatedSplatmap);
            }
            if (generatedSplatmap2 == null)
            {
                generatedSplatmap2 = new Texture2D(256 * 4, 256 * 4, TextureFormat.RGBA32, false);
                Shader.SetGlobalTexture("_SplatMap2", generatedSplatmap2);
            }

            if (color_splat1.Length != _terrainData.SplatmapLength)
            {
                color_splat1 = new Color32[_terrainData.SplatmapLength];
                color_splat2 = new Color32[_terrainData.SplatmapLength];
            }

            byte r1;
            byte g1;
            byte b1;
            byte a1;
            byte r2;
            byte g2;
            byte b2;
            byte a2;

            for (int x = 0; x < _terrainData.SplatmapLength; x++)
            {
                r1 = _terrainData.terrain_layer1[x];
                g1 = _terrainData.terrain_layer2[x];
                b1 = _terrainData.terrain_layer3[x];
                a1 = _terrainData.terrain_layer4[x];
                r2 = _terrainData.terrain_layer5[x];
                g2 = _terrainData.terrain_layer6[x];
                b2 = _terrainData.terrain_layer7[x];
                a2 = _terrainData.terrain_layer8[x];

                color_splat1[x] = new Color32(r1, g1, b1, a1);
                color_splat2[x] = new Color32(r2, g2, b2, a2);

            }

            generatedSplatmap.SetPixels32(color_splat1);
            generatedSplatmap2.SetPixels32(color_splat2);
            generatedSplatmap.Apply();
            generatedSplatmap2.Apply();


        }

        private void _partialUpdateMap(int x, int y, int width, int length)
        {
            if (generatedSplatmap == null)
            {
                _updateTerrainMap();
                Debug.LogError("splatmap not yet generated! Generating splat...");
                return;
            }

            int lengthArr = width * length;

            color_splat1 = generatedSplatmap.GetPixels32();
            color_splat2 = generatedSplatmap2.GetPixels32();

            byte r1;
            byte g1;
            byte b1;
            byte a1;
            byte r2;
            byte g2;
            byte b2;
            byte a2;

            for (int x1 = 0; x1 < width * 2; x1++)
            {

                for (int y1 = 0; y1 < length * 2; y1++)
                {
                    int index = _terrainData.GetSplatmapIndex(x, y) + _terrainData.GetSplatmapIndex(x1, y1);

                    if (index < 0) continue;
                    if (index >= _terrainData.SplatmapLength) continue;


                    r1 = _terrainData.terrain_layer1[index];
                    g1 = _terrainData.terrain_layer2[index];
                    b1 = _terrainData.terrain_layer3[index];
                    a1 = _terrainData.terrain_layer4[index];
                    r2 = _terrainData.terrain_layer5[index];
                    g2 = _terrainData.terrain_layer6[index];
                    b2 = _terrainData.terrain_layer7[index];
                    a2 = _terrainData.terrain_layer8[index];

                    color_splat1[index] = new Color32(r1, g1, b1, a1);
                    color_splat2[index] = new Color32(r2, g2, b2, a2);
                }

            }

            generatedSplatmap.SetPixels32(color_splat1);
            generatedSplatmap2.SetPixels32(color_splat2);
            generatedSplatmap.Apply();
            generatedSplatmap2.Apply();
        }


        [FoldoutGroup("DEBUG")]
        [Button("UpdateCliffMap")]

        public void UpdateCliffMap(int startX = 0, int startY = 0, int width = 256, int height = 256)
        {
            Vector2Int[] offsetCoords = new Vector2Int[4]
            {
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
                new Vector2Int(0, 1),
                new Vector2Int(1, 1)
            };
            Vector2Int offsetPos1 = new Vector2Int(0, 0);
            Vector2Int offsetPos2 = new Vector2Int(1, 0);
            Vector2Int offsetPos3 = new Vector2Int(0, 1);
            Vector2Int offsetPos4 = new Vector2Int(1, 1);

            startX = Mathf.Clamp(startX, 0, _terrainData.size_x-1);
            startY = Mathf.Clamp(startY, 0, _terrainData.size_y-1);

            int startingIndex = _terrainData.GetIndex(startX, startY);
            int finalIndex = _terrainData.cliffLevel.Length;

            int boxEndX = Mathf.Clamp(startX + width, 0, _terrainData.size_x-1);
            int boxEndY = Mathf.Clamp(startY + height, 0, _terrainData.size_y-1);


            if (width < 255 && height < 255) finalIndex = _terrainData.GetIndex(boxEndX, boxEndY);
            finalIndex = Mathf.Clamp(finalIndex, 0, _terrainData.TotalLength);


            int indexToPrintDebug = Random.Range(startingIndex, finalIndex);
            string allIndexes = "";

            for (int i = startingIndex; i < finalIndex; i++)
            {
                int x = i % _terrainData.size_x;
                int y = i / _terrainData.size_x;

                if (x < startX) continue;
                if (y < startY) continue;
                if (x > boxEndX) continue;
                if (y > boxEndY) continue;

                Vector2Int myPos = new Vector2Int(x, y);

                Vector2Int[] myPosArray = new Vector2Int[4]
                {
                    myPos + offsetPos1,
                    myPos + offsetPos2,
                    myPos + offsetPos3,
                    myPos + offsetPos4
                };


                int indexDir = 0;

                //atlas coord
                foreach (var coord in myPosArray)
                {
                    if (_terrainData.IsIndexOutsideArray(coord.x, coord.y) == true) continue;

                    GameObject instantiated = null;
                    var tilesetList = GetTileSet(myPos, indexDir, coord);
                    int index_coord = _terrainData.GetIndex(coord.x, coord.y);


                    CliffObjectDat cliffObjTarget = allCliffObjectResultData[index_coord]; //always exist
                    bool isExactSameset = cliffObjTarget.ExactSameSet(tilesetList);

                    if (!isExactSameset)
                    {
                        foreach (var cliff in cliffObjTarget.allCliffs)
                        {
                            if (cliff != null)
                                Destroy(cliff);
                        }
                    }
                    else
                    {
                        indexDir++;
                        //SKIP
                        continue;
                    }

                    cliffObjTarget.tileset = new SO_TerrainPreset.Tileset[tilesetList.Length];
                    cliffObjTarget.allCliffs = new GameObject[tilesetList.Length];
                    cliffObjTarget.pos = coord;


                    int thisList = cliffObjTarget.allCliffs.Length;

                    for (int d1 = 0; d1 < tilesetList.Length; d1++)
                    {
                        var tileSet1 = tilesetList[d1];

                        Vector3 worldPos = new Vector3();
                        worldPos.x = (coord.x * 2);
                        worldPos.y = d1 * 4;
                        worldPos.z = (coord.y * 2);
                        GameObject template = null;

                        if (_terrainData.manmadeCliffs[i])
                        {
                            template = MyPreset.GetManmadeCliff(tileSet1);
                        }
                        else
                        {
                            template = MyPreset.GetOrganicCliff(tileSet1);
                        }
                        int DEBUG_result = 0;


                        if (template != null)
                        {

                            instantiated = CreateCliffObject(template, worldPos, $"Tile_{myPos}y{worldPos.y.ToInt()}_{tileSet1}({(Direction_TileCheck)indexDir})[{d1}]");
                            cliffObjTarget.allCliffs[d1] = instantiated;
                            cliffObjTarget.tileset[d1] = tileSet1;


                        }

                    }

                    if (DEBUG_lastSecondChecked != Time.time.ToInt())
                    {
                        //Debug.Log($"mypos: {x} {y} | {coord}");
                    }

                    indexDir++;
                }

            }


            //removing hanging cliff objects
            //foreach (var cliffObj in vd3)
            //{
            //    //if (cliffObj.isToBeDestroyed == true) continue;
            //    if (cliffObj.cliffGO == null) continue;
            //    Vector3 myPos = cliffObj.pos;
            //    myPos.y = 0;
            //    var baseCliff = vd3.Find(x => x.pos == myPos && x.isBase && x.isToBeDestroyed == false);

            //    if (baseCliff != null)
            //    {
            //       // Debug.Log($"{baseCliff.cliffGO.gameObject.name} {baseCliff.pos} ||| {cliffObj.cliffGO.gameObject.name} {cliffObj.pos}");
            //    }

            //    if (baseCliff == null)
            //    {
            //        Destroy(cliffObj.cliffGO);
            //       // Debug.Log($"DELETING: {cliffObj.pos}");
            //    }
            //}


            if (Time.time % 2 == 0) DEBUG_lastSecondChecked = Time.time.ToInt();

            //vd3.RemoveAll(f => f.cliffGO == null);
        }

        private List<GameObject> debug_listAllNoNeighborObjs = new List<GameObject>();

        public GameObject CreateCliffObject(GameObject template, Vector3 worldPos, string goName)
        {
            var cliffnewObj = Instantiate(template);
            Vector3 cliffPos = worldPos;

            cliffnewObj.transform.position = cliffPos;
            cliffnewObj.gameObject.name = goName;
            MeshRenderer[] meshRenders = cliffnewObj.GetComponentsInChildren<MeshRenderer>();

            foreach(var meshRndr in meshRenders)
            {

                for (int x = 0; x < meshRndr.sharedMaterials.Length; x++)
                {
                    var demArrays = meshRndr.sharedMaterials;

                    if (demArrays[x] == _sourceTerrainMat)
                    {
                        demArrays[x] = generatedTerrainMaterial;
                    }

                    meshRndr.sharedMaterials = demArrays;
                }
            }

            cliffnewObj.transform.SetParent(terrainParent);

            return cliffnewObj;
        }





        #endregion


        #region Functions
        public Vector2Int GetDirOffset(Direction_TileCheck dir, Direction_TileCheck coordDir)
        {
            Vector2Int offsetPos1 = new Vector2Int(0, 0);
            Vector2Int offsetPos2 = new Vector2Int(1, 0);
            Vector2Int offsetPos3 = new Vector2Int(0, 1);
            Vector2Int offsetPos4 = new Vector2Int(1, 1);

            if (coordDir == Direction_TileCheck.Northeast)
            {
                Vector2Int offset_byNeighbor = new Vector2Int(0, 0);
                offsetPos1 -= offset_byNeighbor;
                offsetPos2 -= offset_byNeighbor;
                offsetPos3 -= offset_byNeighbor;
                offsetPos4 -= offset_byNeighbor;
            }
            else if (coordDir == Direction_TileCheck.Northwest)
            {
                Vector2Int offset_byNeighbor = new Vector2Int(1, 0);
                offsetPos1 -= offset_byNeighbor;
                offsetPos2 -= offset_byNeighbor;
                offsetPos3 -= offset_byNeighbor;
                offsetPos4 -= offset_byNeighbor;
            }
            else if (coordDir == Direction_TileCheck.Southeast)
            {
                Vector2Int offset_byNeighbor = new Vector2Int(0, 1);
                offsetPos1 -= offset_byNeighbor;
                offsetPos2 -= offset_byNeighbor;
                offsetPos3 -= offset_byNeighbor;
                offsetPos4 -= offset_byNeighbor;
            }
            else if (coordDir == Direction_TileCheck.Southwest)
            {
                Vector2Int offset_byNeighbor = new Vector2Int(1, 1);
                offsetPos1 -= offset_byNeighbor;
                offsetPos2 -= offset_byNeighbor;
                offsetPos3 -= offset_byNeighbor;
                offsetPos4 -= offset_byNeighbor;
            }

            if (dir == Direction_TileCheck.Southwest) return offsetPos1;
            if (dir == Direction_TileCheck.Southeast) return offsetPos2;
            if (dir == Direction_TileCheck.Northwest) return offsetPos3;
            if (dir == Direction_TileCheck.Northeast) return offsetPos4;
            return offsetPos1;
        }

        public enum Direction_TileCheck
        {
            Southwest,
            Southeast,
            Northwest,
            Northeast
        }

        [FoldoutGroup("DEBUG")] public bool DEBUG_OutputTest = false;
        private int countDEBUG_tileset = 0;

        //there is a problem here
        //Array because it returns depends on the cliff levels (3 means it returns 3 objects)
        public SO_TerrainPreset.Tileset[] GetTileSet(Vector2Int myPos, int indexDir, Vector2Int coord, bool printDEBUG = false)
        {
            List<SO_TerrainPreset.Tileset> tilesetList = new List<SO_TerrainPreset.Tileset>();
            SO_TerrainPreset.Tileset tilesetTarget ;

            Direction_TileCheck dir = (Direction_TileCheck)indexDir;

            var myPos_cliff = _terrainData.GetNeighbor(SyntiosTerrainData.DirectionNeighbor.Self, myPos);
            var north = _terrainData.GetNeighbor(SyntiosTerrainData.DirectionNeighbor.North, myPos);
            var south = _terrainData.GetNeighbor(SyntiosTerrainData.DirectionNeighbor.South, myPos);
            var west = _terrainData.GetNeighbor(SyntiosTerrainData.DirectionNeighbor.West, myPos);
            var east = _terrainData.GetNeighbor(SyntiosTerrainData.DirectionNeighbor.East, myPos);
            var northwest = _terrainData.GetNeighbor(SyntiosTerrainData.DirectionNeighbor.NorthWest, myPos);
            var northeast = _terrainData.GetNeighbor(SyntiosTerrainData.DirectionNeighbor.NorthEast, myPos);
            var southwest = _terrainData.GetNeighbor(SyntiosTerrainData.DirectionNeighbor.SouthWest, myPos);
            var southeast = _terrainData.GetNeighbor(SyntiosTerrainData.DirectionNeighbor.SouthEast, myPos);


            int countTile = 0;

            if (DEBUG_OutputTest)
            {
                countDEBUG_tileset++;
            }

            if (dir == Direction_TileCheck.Southwest)
            {
                int maxHeight = Mathf.Max(new int[] { south.cliffLevel, southwest.cliffLevel, west.cliffLevel, myPos_cliff.cliffLevel});
                maxHeight = Mathf.Clamp(maxHeight, 0, 16);


                for (int x = 0; x <= maxHeight; x++)
                {
                    int south_cliffLV = south.cliffLevel;
                    int southwest_cliffLV = southwest.cliffLevel;
                    int west_cliffLV = west.cliffLevel;
                    int myCliffLV = myPos_cliff.cliffLevel;

               

                    //should 16 permutations
                    //CORNER
                    if (south_cliffLV <= x && southwest_cliffLV <= x && west_cliffLV <= x && myCliffLV <= x)
                    {
                        tilesetList.Add(SO_TerrainPreset.Tileset.Null);
                    }
                    if (south_cliffLV > x && southwest_cliffLV <= x && west_cliffLV <= x && myCliffLV <= x)
                    {
                        tilesetList.Add(SO_TerrainPreset.Tileset.CornerNorthWest);
                    }
                    if (south_cliffLV <= x && southwest_cliffLV > x && west_cliffLV <= x && myCliffLV <= x)
                    {
                        tilesetList.Add(SO_TerrainPreset.Tileset.CornerNorthEast);
                    }
                    if (south_cliffLV <= x && southwest_cliffLV <= x && west_cliffLV <= x && myCliffLV > x)
                    {
                        tilesetList.Add(SO_TerrainPreset.Tileset.CornerSouthWest);
                    }
                    if (south_cliffLV <= x && southwest_cliffLV <= x && west_cliffLV > x && myCliffLV <= x)
                    {
                        tilesetList.Add(SO_TerrainPreset.Tileset.CornerSouthEast);
                    }

                    if (south_cliffLV > x && southwest_cliffLV > x && west_cliffLV <= x && myCliffLV <= x)
                    {
                        tilesetList.Add(SO_TerrainPreset.Tileset.North);
                    }
                    if (south_cliffLV <= x && southwest_cliffLV <= x && west_cliffLV > x && myCliffLV > x)
                    {
                        tilesetList.Add(SO_TerrainPreset.Tileset.South);
                    }
                    if (south_cliffLV > x && southwest_cliffLV <= x && west_cliffLV <= x && myCliffLV > x)
                    {
                        tilesetList.Add(SO_TerrainPreset.Tileset.West);
                    }
                    if (south_cliffLV <= x && southwest_cliffLV > x && west_cliffLV > x && myCliffLV <= x)
                    {
                        tilesetList.Add(SO_TerrainPreset.Tileset.East);
                    }

                    //diagonals
                    if (south_cliffLV <= x && southwest_cliffLV > x && west_cliffLV <= x && myCliffLV > x)
                    {
                        tilesetList.Add(SO_TerrainPreset.Tileset.DiagonalEast);
                    }
                    if (south_cliffLV > x && southwest_cliffLV <= x && west_cliffLV > x && myCliffLV <= x)
                    {
                        tilesetList.Add(SO_TerrainPreset.Tileset.DiagonalWest);
                    }

                    //sharp corners
                    if (south_cliffLV > x && southwest_cliffLV > x && west_cliffLV <= x && myCliffLV > x)
                    {
                        var cornerPos = myPos + GetDirOffset(Direction_TileCheck.Northwest, dir);
                        var corner_Northeast = _terrainData.GetNeighbor(SyntiosTerrainData.DirectionNeighbor.NorthEast, cornerPos);
                        var corner_Southwest = _terrainData.GetNeighbor(SyntiosTerrainData.DirectionNeighbor.SouthWest, cornerPos);

                        if (corner_Northeast.cliffLevel > x && corner_Southwest.cliffLevel > x)
                        {
                            tilesetList.Add(SO_TerrainPreset.Tileset.SharpCornerNorthWest);
                        }
                        else
                        {
                            tilesetList.Add(SO_TerrainPreset.Tileset.Corner78NorthWest);
                        }
                    }
                    if (south_cliffLV > x && southwest_cliffLV > x && west_cliffLV > x && myCliffLV <= x)
                    {
                        var cornerPos = myPos + GetDirOffset(Direction_TileCheck.Northeast, dir);
                        var corner_Northwest = _terrainData.GetNeighbor(SyntiosTerrainData.DirectionNeighbor.NorthWest, cornerPos);
                        var corner_Southeast = _terrainData.GetNeighbor(SyntiosTerrainData.DirectionNeighbor.SouthEast, cornerPos);

                        if (corner_Northwest.cliffLevel > x && corner_Southeast.cliffLevel > x)
                        {
                            tilesetList.Add(SO_TerrainPreset.Tileset.SharpCornerNorthEast);
                        }
                        else
                        {
                            tilesetList.Add(SO_TerrainPreset.Tileset.Corner78NorthEast);
                        }
                    }
                    if (south_cliffLV > x && southwest_cliffLV <= x && west_cliffLV > x && myCliffLV > x)
                    {
                        var cornerPos = myPos + GetDirOffset(Direction_TileCheck.Southwest, dir);
                        var corner_Northwest = _terrainData.GetNeighbor(SyntiosTerrainData.DirectionNeighbor.NorthWest, cornerPos);
                        var corner_Southeast = _terrainData.GetNeighbor(SyntiosTerrainData.DirectionNeighbor.SouthEast, cornerPos);

                        if (corner_Northwest.cliffLevel > x && corner_Southeast.cliffLevel > x)
                        {
                            tilesetList.Add(SO_TerrainPreset.Tileset.SharpCornerSouthWest);
                        }
                        else
                        {
                            tilesetList.Add(SO_TerrainPreset.Tileset.Corner78SouthWest);
                        }
                    }
                    if (south_cliffLV <= x && southwest_cliffLV > x && west_cliffLV > x && myCliffLV > x)
                    {
                        var cornerPos = myPos + GetDirOffset(Direction_TileCheck.Southeast, dir);
                        var corner_Northeast = _terrainData.GetNeighbor(SyntiosTerrainData.DirectionNeighbor.NorthEast, cornerPos);
                        var corner_Southwest = _terrainData.GetNeighbor(SyntiosTerrainData.DirectionNeighbor.SouthWest, cornerPos);

                        if (corner_Northeast.cliffLevel > x && corner_Southwest.cliffLevel > x)
                        {
                            tilesetList.Add(SO_TerrainPreset.Tileset.SharpCornerSouthEast);
                        }
                        else
                        {
                            tilesetList.Add(SO_TerrainPreset.Tileset.Corner78SouthEast);
                        }
                    }

                    if (south_cliffLV > x && southwest_cliffLV > x && west_cliffLV > x && myCliffLV > x)
                    {
                        tilesetList.Add(SO_TerrainPreset.Tileset.Flat);
                    }



                    countTile++;

                }
                
            }
            else if (dir == Direction_TileCheck.Southeast)
            {
                int maxHeight = Mathf.Max(new int[] { south.cliffLevel, southeast.cliffLevel, east.cliffLevel, myPos_cliff.cliffLevel });
                maxHeight = Mathf.Clamp(maxHeight, 0, 16);

                for (int x = 0; x <= maxHeight; x++)
                {
                    int south_cliffLV = south.cliffLevel;
                    int southeast_cliffLV = southeast.cliffLevel;
                    int east_cliffLV = east.cliffLevel;
                    int myCliffLV = myPos_cliff.cliffLevel;

                    //CORNER
                    if (south_cliffLV <= x && southeast_cliffLV <= x && east_cliffLV <= x && myCliffLV <= x)
                    {
                        tilesetList.Add(SO_TerrainPreset.Tileset.Null);
                    }
                    if (south_cliffLV <= x && southeast_cliffLV > x && east_cliffLV <= x && myCliffLV <= x)
                    {
                        tilesetList.Add(SO_TerrainPreset.Tileset.CornerNorthWest);
                    }
                    if (south_cliffLV > x && southeast_cliffLV <= x && east_cliffLV <= x && myCliffLV <= x)
                    {
                        tilesetList.Add(SO_TerrainPreset.Tileset.CornerNorthEast);
                    }
                    if (south_cliffLV <= x && southeast_cliffLV <= x && east_cliffLV > x && myCliffLV <= x)
                    {
                        tilesetList.Add(SO_TerrainPreset.Tileset.CornerSouthWest);
                    }
                    if (south_cliffLV <= x && southeast_cliffLV <= x && east_cliffLV <= x && myCliffLV > x)
                    {
                        tilesetList.Add(SO_TerrainPreset.Tileset.CornerSouthEast);
                    }

                    if (south_cliffLV > x && southeast_cliffLV > x && east_cliffLV <= x && myCliffLV <= x)
                    {
                        tilesetList.Add(SO_TerrainPreset.Tileset.North);
                    }
                    if (south_cliffLV <= x && southeast_cliffLV <= x && east_cliffLV > x && myCliffLV > x)
                    {
                        tilesetList.Add(SO_TerrainPreset.Tileset.South);
                    }
                    if (south_cliffLV <= x && southeast_cliffLV > x && east_cliffLV > x && myCliffLV <= x)
                    {
                        tilesetList.Add(SO_TerrainPreset.Tileset.West);
                    }
                    if (south_cliffLV > x && southeast_cliffLV <= x && east_cliffLV <= x && myCliffLV > x)
                    {
                        tilesetList.Add(SO_TerrainPreset.Tileset.East);
                    }

                    //diagonals
                    if (south_cliffLV > x && southeast_cliffLV <= x && east_cliffLV > x && myCliffLV <= x)
                    {
                        tilesetList.Add(SO_TerrainPreset.Tileset.DiagonalEast);
                    }
                    if (south_cliffLV <= x && southeast_cliffLV > x && east_cliffLV <= x && myCliffLV > x)
                    {
                        tilesetList.Add(SO_TerrainPreset.Tileset.DiagonalWest);
                    }

                    //sharp corners
                    if (south_cliffLV > x && southeast_cliffLV > x && east_cliffLV > x && myCliffLV <= x)
                    {
                        var cornerPos = myPos + GetDirOffset(Direction_TileCheck.Northwest, dir);
                        var corner_Northeast = _terrainData.GetNeighbor(SyntiosTerrainData.DirectionNeighbor.NorthEast, cornerPos);
                        var corner_Southwest = _terrainData.GetNeighbor(SyntiosTerrainData.DirectionNeighbor.SouthWest, cornerPos);

                        if (corner_Northeast.cliffLevel > x && corner_Southwest.cliffLevel > x)
                        {
                            tilesetList.Add(SO_TerrainPreset.Tileset.SharpCornerNorthWest);
                        }
                        else
                        {
                            tilesetList.Add(SO_TerrainPreset.Tileset.Corner78NorthWest);
                        }
                    }
                    if (south_cliffLV > x && southeast_cliffLV > x && east_cliffLV <= x && myCliffLV > x)
                    {
                        var cornerPos = myPos + GetDirOffset(Direction_TileCheck.Northeast, dir);
                        var corner_Northwest = _terrainData.GetNeighbor(SyntiosTerrainData.DirectionNeighbor.NorthWest, cornerPos);
                        var corner_Southeast = _terrainData.GetNeighbor(SyntiosTerrainData.DirectionNeighbor.SouthEast, cornerPos);

                        if (corner_Northwest.cliffLevel > x && corner_Southeast.cliffLevel > x)
                        {
                            tilesetList.Add(SO_TerrainPreset.Tileset.SharpCornerNorthEast);
                        }
                        else
                        {
                            tilesetList.Add(SO_TerrainPreset.Tileset.Corner78NorthEast);
                        }
                    }
                    if (south_cliffLV <= x && southeast_cliffLV > x && east_cliffLV > x && myCliffLV > x)
                    {
                        var cornerPos = myPos + GetDirOffset(Direction_TileCheck.Southwest, dir);
                        var corner_Northwest = _terrainData.GetNeighbor(SyntiosTerrainData.DirectionNeighbor.NorthWest, cornerPos);
                        var corner_Southeast = _terrainData.GetNeighbor(SyntiosTerrainData.DirectionNeighbor.SouthEast, cornerPos);

                        if (corner_Northwest.cliffLevel > x && corner_Southeast.cliffLevel > x)
                        {
                            tilesetList.Add(SO_TerrainPreset.Tileset.SharpCornerSouthWest);
                        }
                        else
                        {
                            tilesetList.Add(SO_TerrainPreset.Tileset.Corner78SouthWest);
                        }
                    }
                    if (south_cliffLV > x && southeast_cliffLV <= x && east_cliffLV > x && myCliffLV > x)
                    {
                        var cornerPos = myPos + GetDirOffset(Direction_TileCheck.Southeast, dir);
                        var corner_Northeast = _terrainData.GetNeighbor(SyntiosTerrainData.DirectionNeighbor.NorthEast, cornerPos);
                        var corner_Southwest = _terrainData.GetNeighbor(SyntiosTerrainData.DirectionNeighbor.SouthWest, cornerPos);

                        if (corner_Northeast.cliffLevel > x && corner_Southwest.cliffLevel > x)
                        {
                            tilesetList.Add(SO_TerrainPreset.Tileset.SharpCornerSouthEast);
                        }
                        else
                        {
                            tilesetList.Add(SO_TerrainPreset.Tileset.Corner78SouthEast);
                        }
                    }

                    if (south_cliffLV > x && southeast_cliffLV > x && east_cliffLV > x && myCliffLV > x)
                    {
                        tilesetList.Add(SO_TerrainPreset.Tileset.Flat);
                    }


                    countTile++;

                }

            }
            else if (dir == Direction_TileCheck.Northwest)
            {
                int maxHeight = Mathf.Max(new int[] { north.cliffLevel, northwest.cliffLevel, west.cliffLevel, myPos_cliff.cliffLevel });
                maxHeight = Mathf.Clamp(maxHeight, 0, 16);


                for (int x = 0; x <= maxHeight; x++)
                {
                    int north_cliffLV = north.cliffLevel;
                    int northwest_cliffLV = northwest.cliffLevel;
                    int west_cliffLV = west.cliffLevel;
                    int myCliffLV = myPos_cliff.cliffLevel;

                    //CORNER
                    if (north_cliffLV <= x && northwest_cliffLV <= x && west_cliffLV <= x && myCliffLV <= x)
                    {
                        tilesetList.Add(SO_TerrainPreset.Tileset.Null);
                    }
                    if (north_cliffLV <= x && northwest_cliffLV <= x && west_cliffLV <= x && myCliffLV > x)
                    {
                        tilesetList.Add(SO_TerrainPreset.Tileset.CornerNorthWest);
                    }
                    if (north_cliffLV <= x && northwest_cliffLV <= x && west_cliffLV > x && myCliffLV <= x)
                    {
                        tilesetList.Add(SO_TerrainPreset.Tileset.CornerNorthEast);
                    }
                    if (north_cliffLV > x && northwest_cliffLV <= x && west_cliffLV <= x && myCliffLV <= x)
                    {
                        tilesetList.Add(SO_TerrainPreset.Tileset.CornerSouthWest);
                    }
                    if (north_cliffLV <= x && northwest_cliffLV > x && west_cliffLV <= x && myCliffLV <= x)
                    {
                        tilesetList.Add(SO_TerrainPreset.Tileset.CornerSouthEast);
                    }

                    if (north_cliffLV <= x && northwest_cliffLV <= x && west_cliffLV > x && myCliffLV > x)
                    {
                        tilesetList.Add(SO_TerrainPreset.Tileset.North);
                    }
                    if (north_cliffLV > x && northwest_cliffLV > x && west_cliffLV <= x && myCliffLV <= x)
                    {
                        tilesetList.Add(SO_TerrainPreset.Tileset.South);
                    }
                    if (north_cliffLV > x && northwest_cliffLV <= x && west_cliffLV <= x && myCliffLV > x)
                    {
                        tilesetList.Add(SO_TerrainPreset.Tileset.West);
                    }
                    if (north_cliffLV <= x && northwest_cliffLV > x && west_cliffLV > x && myCliffLV <= x)
                    {
                        tilesetList.Add(SO_TerrainPreset.Tileset.East);
                    }

                    //diagonals
                    if (north_cliffLV > x && northwest_cliffLV <= x && west_cliffLV > x && myCliffLV <= x)
                    {
                        tilesetList.Add(SO_TerrainPreset.Tileset.DiagonalEast);
                    }
                    if (north_cliffLV <= x && northwest_cliffLV > x && west_cliffLV <= x && myCliffLV > x)
                    {
                        tilesetList.Add(SO_TerrainPreset.Tileset.DiagonalWest);
                    }

                    //sharp corners
                    if (north_cliffLV > x && northwest_cliffLV <= x && west_cliffLV > x && myCliffLV > x)
                    {
                        var cornerPos = myPos + GetDirOffset(Direction_TileCheck.Northwest, dir);
                        var corner_Northeast = _terrainData.GetNeighbor(SyntiosTerrainData.DirectionNeighbor.NorthEast, cornerPos);
                        var corner_Southwest = _terrainData.GetNeighbor(SyntiosTerrainData.DirectionNeighbor.SouthWest, cornerPos);

                        if (corner_Northeast.cliffLevel > x && corner_Southwest.cliffLevel > x)
                        {
                            tilesetList.Add(SO_TerrainPreset.Tileset.SharpCornerNorthWest);
                        }
                        else
                        {
                            tilesetList.Add(SO_TerrainPreset.Tileset.Corner78NorthWest);
                        }
                    }
                    if (north_cliffLV <= x && northwest_cliffLV > x && west_cliffLV > x && myCliffLV > x)
                    {
                        var cornerPos = myPos + GetDirOffset(Direction_TileCheck.Northeast, dir);
                        var corner_Northwest = _terrainData.GetNeighbor(SyntiosTerrainData.DirectionNeighbor.NorthWest, cornerPos);
                        var corner_Southeast = _terrainData.GetNeighbor(SyntiosTerrainData.DirectionNeighbor.SouthEast, cornerPos);

                        if (corner_Northwest.cliffLevel > x && corner_Southeast.cliffLevel > x)
                        {
                            tilesetList.Add(SO_TerrainPreset.Tileset.SharpCornerNorthEast);
                        }
                        else
                        {
                            tilesetList.Add(SO_TerrainPreset.Tileset.Corner78NorthEast);
                        }
                    }
                    if (north_cliffLV > x && northwest_cliffLV > x && west_cliffLV <= x && myCliffLV > x)
                    {
                        var cornerPos = myPos + GetDirOffset(Direction_TileCheck.Southwest, dir);
                        var corner_Northwest = _terrainData.GetNeighbor(SyntiosTerrainData.DirectionNeighbor.NorthWest, cornerPos);
                        var corner_Southeast = _terrainData.GetNeighbor(SyntiosTerrainData.DirectionNeighbor.SouthEast, cornerPos);

                        if (corner_Northwest.cliffLevel > x && corner_Southeast.cliffLevel > x)
                        {
                            tilesetList.Add(SO_TerrainPreset.Tileset.SharpCornerSouthWest);
                        }
                        else
                        {
                            tilesetList.Add(SO_TerrainPreset.Tileset.Corner78SouthWest);
                        }
                    }
                    if (north_cliffLV > x && northwest_cliffLV > x && west_cliffLV > x && myCliffLV <= x)
                    {
                        var cornerPos = myPos + GetDirOffset(Direction_TileCheck.Southeast, dir);
                        var corner_Northeast = _terrainData.GetNeighbor(SyntiosTerrainData.DirectionNeighbor.NorthEast, cornerPos);
                        var corner_Southwest = _terrainData.GetNeighbor(SyntiosTerrainData.DirectionNeighbor.SouthWest, cornerPos);

                        if (corner_Northeast.cliffLevel > x && corner_Southwest.cliffLevel > x)
                        {
                            tilesetList.Add(SO_TerrainPreset.Tileset.SharpCornerSouthEast);
                        }
                        else
                        {
                            tilesetList.Add(SO_TerrainPreset.Tileset.Corner78SouthEast);
                        }
                    }

                    if (north_cliffLV > x && northwest_cliffLV > x && west_cliffLV > x && myCliffLV > x)
                    {
                        tilesetList.Add(SO_TerrainPreset.Tileset.Flat);
                    }


                    countTile++;

                }

            }
            else if (dir == Direction_TileCheck.Northeast)
            {
                int maxHeight = Mathf.Max(new int[] { north.cliffLevel, northeast.cliffLevel, east.cliffLevel, myPos_cliff.cliffLevel });
                maxHeight = Mathf.Clamp(maxHeight, 0, 16);


                for (int x = 0; x <= maxHeight; x++)
                {
                    int north_cliffLV = north.cliffLevel;
                    int northeast_cliffLV = northeast.cliffLevel;
                    int east_cliffLV = east.cliffLevel;
                    int myCliffLV = myPos_cliff.cliffLevel;

                    //CORNER
                    if (north_cliffLV <= x && northeast_cliffLV <= x && east_cliffLV <= x && myCliffLV <= x)
                    {
                        tilesetList.Add(SO_TerrainPreset.Tileset.Null);
                    }
                    if (north_cliffLV <= x && northeast_cliffLV <= x && east_cliffLV > x && myCliffLV <= x)
                    {
                        tilesetList.Add(SO_TerrainPreset.Tileset.CornerNorthWest);
                    }
                    if (north_cliffLV <= x && northeast_cliffLV <= x && east_cliffLV <= x && myCliffLV > x)
                    {
                        tilesetList.Add(SO_TerrainPreset.Tileset.CornerNorthEast);
                    }
                    if (north_cliffLV <= x && northeast_cliffLV > x && east_cliffLV <= x && myCliffLV <= x)
                    {
                        tilesetList.Add(SO_TerrainPreset.Tileset.CornerSouthWest);
                    }
                    if (north_cliffLV > x && northeast_cliffLV <= x && east_cliffLV <= x && myCliffLV <= x)
                    {
                        tilesetList.Add(SO_TerrainPreset.Tileset.CornerSouthEast);
                    }

                    if (north_cliffLV <= x && northeast_cliffLV <= x && east_cliffLV > x && myCliffLV > x)
                    {
                        tilesetList.Add(SO_TerrainPreset.Tileset.North);
                    }
                    if (north_cliffLV > x && northeast_cliffLV > x && east_cliffLV <= x && myCliffLV <= x)
                    {
                        tilesetList.Add(SO_TerrainPreset.Tileset.South);
                    }
                    if (north_cliffLV <= x && northeast_cliffLV > x && east_cliffLV > x && myCliffLV <= x)
                    {
                        tilesetList.Add(SO_TerrainPreset.Tileset.West);
                    }
                    if (north_cliffLV > x && northeast_cliffLV <= x && east_cliffLV <= x && myCliffLV > x)
                    {
                        tilesetList.Add(SO_TerrainPreset.Tileset.East);
                    }

                    //diagonals
                    if (north_cliffLV <= x && northeast_cliffLV > x && east_cliffLV <= x && myCliffLV > x)
                    {
                        tilesetList.Add(SO_TerrainPreset.Tileset.DiagonalEast);
                    }
                    if (north_cliffLV > x && northeast_cliffLV <= x && east_cliffLV > x && myCliffLV <= x)
                    {
                        tilesetList.Add(SO_TerrainPreset.Tileset.DiagonalWest);
                    }

                    //sharp corners
                    if (north_cliffLV <= x && northeast_cliffLV > x && east_cliffLV > x && myCliffLV > x)
                    {
                        var cornerPos = myPos + GetDirOffset(Direction_TileCheck.Northwest, dir);
                        var corner_Northeast = _terrainData.GetNeighbor(SyntiosTerrainData.DirectionNeighbor.NorthEast, cornerPos);
                        var corner_Southwest = _terrainData.GetNeighbor(SyntiosTerrainData.DirectionNeighbor.SouthWest, cornerPos);

                        if (corner_Northeast.cliffLevel > x && corner_Southwest.cliffLevel > x)
                        {
                            tilesetList.Add(SO_TerrainPreset.Tileset.SharpCornerNorthWest);
                        }
                        else
                        {
                            tilesetList.Add(SO_TerrainPreset.Tileset.Corner78NorthWest);
                        }

                    }
                    if (north_cliffLV > x && northeast_cliffLV <= x && east_cliffLV > x && myCliffLV > x)
                    {
                        var cornerPos = myPos + GetDirOffset(Direction_TileCheck.Northeast, dir);
                        var corner_Northwest = _terrainData.GetNeighbor(SyntiosTerrainData.DirectionNeighbor.NorthWest, cornerPos);
                        var corner_Southeast = _terrainData.GetNeighbor(SyntiosTerrainData.DirectionNeighbor.SouthEast, cornerPos);

                        if (corner_Northwest.cliffLevel > x && corner_Southeast.cliffLevel > x)
                        {
                            tilesetList.Add(SO_TerrainPreset.Tileset.SharpCornerNorthEast);
                        }
                        else
                        {
                            tilesetList.Add(SO_TerrainPreset.Tileset.Corner78NorthEast);
                        }

                    }
                    if (north_cliffLV > x && northeast_cliffLV > x && east_cliffLV > x && myCliffLV <= x)
                    {
                        var cornerPos = myPos + GetDirOffset(Direction_TileCheck.Southwest, dir);
                        var corner_Northwest = _terrainData.GetNeighbor(SyntiosTerrainData.DirectionNeighbor.NorthWest, cornerPos);
                        var corner_Southeast = _terrainData.GetNeighbor(SyntiosTerrainData.DirectionNeighbor.SouthEast, cornerPos);

                        if (corner_Northwest.cliffLevel > x && corner_Southeast.cliffLevel > x)
                        {
                            tilesetList.Add(SO_TerrainPreset.Tileset.SharpCornerSouthWest);
                        }
                        else
                        {
                            tilesetList.Add(SO_TerrainPreset.Tileset.Corner78SouthWest);
                        }

                    }
                    if (north_cliffLV > x && northeast_cliffLV > x && east_cliffLV <= x && myCliffLV > x)
                    {
                        var cornerPos = myPos + GetDirOffset(Direction_TileCheck.Southeast, dir);
                        var corner_Northeast = _terrainData.GetNeighbor(SyntiosTerrainData.DirectionNeighbor.NorthEast, cornerPos);
                        var corner_Southwest = _terrainData.GetNeighbor(SyntiosTerrainData.DirectionNeighbor.SouthWest, cornerPos);

                        if (corner_Northeast.cliffLevel > x && corner_Southwest.cliffLevel > x)
                        {
                            tilesetList.Add(SO_TerrainPreset.Tileset.SharpCornerSouthEast);
                        }
                        else
                        {
                            tilesetList.Add(SO_TerrainPreset.Tileset.Corner78SouthEast);
                        }

                    }

                    if (north_cliffLV > x && northeast_cliffLV > x && east_cliffLV > x && myCliffLV > x)
                    {
                        tilesetList.Add(SO_TerrainPreset.Tileset.Flat);
                    }


                    countTile++;

                }

            }



            return tilesetList.ToArray();
        }
        #endregion

        private void Update()
        {



        }




    }
}