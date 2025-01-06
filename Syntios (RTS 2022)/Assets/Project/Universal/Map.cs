using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Collections;
using Sirenix.OdinInspector;
using Pathfinding;
using ReadOnlyAttribute = Sirenix.OdinInspector.ReadOnlyAttribute;

namespace ProtoRTS
{
    public class Map : MonoBehaviour
    {

        public List<SO_TerrainPreset> allVanillaTerrainPresets = new List<SO_TerrainPreset>();
        [Space]
        [SerializeField] private SyntiosTerrainData _terrainData;

        [FoldoutGroup("DEBUG")] public bool DEBUG_dontInitializeData;
        [FoldoutGroup("DEBUG")] public bool DEBUG_ShowNoNeighborCheck = false;
        [FoldoutGroup("DEBUG")] public GameObject DEBUGObject_NoNeighborCheck;
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


        public static Map instance;

        private void Awake()
        {
            instance = this;
            terrainParent = new GameObject().transform;
            terrainParent.gameObject.name = "TerrainMesh";
        }

        private void Start()
        {
            InitializeMap();

            var gridGraph = AstarPath.active.data.gridGraph;
            int width = (_terrainData.size_x * 2) / 2;
            int depth = (_terrainData.size_y * 2) / 2;
            width -= 2;
            depth -= 2;
            var center = WorldPosCenter;
            center.x += 1 * 2;
            center.z += 0 * 2;

            gridGraph.SetDimensions(width, depth, 2);
            gridGraph.center = center;

            AstarPath.active.Scan(gridGraph);
        }



        public SO_TerrainPreset MyPreset
        {
            get
            {
                return allVanillaTerrainPresets.Find(x => x.PresetID == _terrainData.ID);
            }
        }


        private void InitializeMap()
        {
            GenerateMaterial();
            if (DEBUG_dontInitializeData == false) _terrainData.InitializeData();
            DEBUG_MeshTerrain.material = generatedTerrainMaterial;
        }


        private void GenerateMaterial()
        {
            generatedTerrainMaterial = new Material(_sourceTerrainMat);
            generatedTerrainMaterial.name = "GeneratedTerrainMat";
            var textureSplatTest = _sourceTerrainMat.GetTexture("_SplatMap");
            var textureSplatTest2 = _sourceTerrainMat.GetTexture("_SplatMap2");

            Shader.SetGlobalTexture("_SplatMap", textureSplatTest);
            Shader.SetGlobalTexture("_SplatMap2", textureSplatTest2);

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

        }


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


        public class CliffObjectDat
        {
            public Vector3 pos;
            public SO_TerrainPreset.Tileset tileset;
            public GameObject cliffGO;

            public CliffObjectDat(Vector3 pos, SO_TerrainPreset.Tileset tileset, GameObject cliffGO)
            {
                this.pos = pos;
                this.tileset = tileset;
                this.cliffGO = cliffGO;
            }
        }

        public bool DEBUG_SeeCliff = false;
        [ShowInInspector] List<CliffObjectDat> vd3 = new List<CliffObjectDat>();
        [ShowInInspector] private List<GameObject> cliffObjects = new List<GameObject>();
        private int DEBUG_lastSecondChecked = 0;

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

            startX = Mathf.Clamp(startX, 0, _terrainData.size_x);
            startY = Mathf.Clamp(startY, 0, _terrainData.size_y);

            int startingIndex = _terrainData.GetIndex(startX, startY);
            int finalIndex = _terrainData.cliffLevel.Length;

            int boxEndX = Mathf.Clamp(startX + width, 0, _terrainData.size_x);
            int boxEndY = Mathf.Clamp(startY + height, 0, _terrainData.size_y);


            if (width < 255 && height < 255) finalIndex = _terrainData.GetIndex(boxEndX, boxEndY);
            finalIndex = Mathf.Clamp(finalIndex, 0, _terrainData.TotalLength);

            foreach (var noNeighbor in debug_listAllNoNeighborObjs)
            {
                Destroy(noNeighbor.gameObject);
            }
            debug_listAllNoNeighborObjs.Clear();

            {
                //foreach (var cliffObj in vd3)
                //{
                //    if (cliffObj.Value != null) Destroy(cliffObj.Value.gameObject);
                //}

                //vd3.Clear();
                //cliffObjects.Clear();

            }

            //Debug.Log($"{startX}, {startY} | ({boxEndX}, {boxEndY})");

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

                //check type
                bool north = _terrainData.IsNeighborValid(SyntiosTerrainData.DirectionNeighbor.North, myPos).hasNeighbor;
                bool south = _terrainData.IsNeighborValid(SyntiosTerrainData.DirectionNeighbor.South, myPos).hasNeighbor;
                bool west = _terrainData.IsNeighborValid(SyntiosTerrainData.DirectionNeighbor.West, myPos).hasNeighbor;
                bool east = _terrainData.IsNeighborValid(SyntiosTerrainData.DirectionNeighbor.East, myPos).hasNeighbor;
                bool northwest = _terrainData.IsNeighborValid(SyntiosTerrainData.DirectionNeighbor.NorthWest, myPos).hasNeighbor;
                bool northeast = _terrainData.IsNeighborValid(SyntiosTerrainData.DirectionNeighbor.NorthEast, myPos).hasNeighbor;
                bool southwest = _terrainData.IsNeighborValid(SyntiosTerrainData.DirectionNeighbor.SouthWest, myPos).hasNeighbor;
                bool southeast = _terrainData.IsNeighborValid(SyntiosTerrainData.DirectionNeighbor.SouthEast, myPos).hasNeighbor;


                int indexDir = 0;

                //atlas coord
                foreach (var coord in myPosArray)
                {
                    GameObject instantiated = null;
                    var tilesetList= GetTileSet(myPos, indexDir, coord);

                    //big problem because this is shared 
                    //causing valid neighbours to be set null
                    //it must check the surrounding to be valid
                    //IT WAS FIGHTING FOR THE SAME FUCKING CELL
                    if (_terrainData.cliffLevel[i] < 1)
                    {
                        //tilesetTarget = SO_TerrainPreset.Tileset.Null;
                    }

                    for(int d1 = 0; d1 < tilesetList.Length; d1++)
                    {
                        var tileSet1 = tilesetList[d1];

                        Vector3 worldPos = new Vector3();
                        worldPos.x = (coord.x * 2);
                        worldPos.y = d1 * 4;
                        worldPos.z = (coord.y * 2);
                        var template = MyPreset.GetManmadeCliff(tileSet1);
                        int DEBUG_result = 0;



                        if (DEBUG_lastSecondChecked != Time.time.ToInt())
                        {
                            //Debug.Log($"[{i}] mypos: {x}, {y} | {coord} [{template} : {tileSet1}]");
                        }

                        //remove pre-existing first.
                        var cliffSimilar = vd3.Find(x => x.pos == worldPos);

                        //replacing existing
                        if (template != null)
                        {


                            if (cliffSimilar != null)
                            {
                                if (cliffSimilar.tileset == tileSet1)
                                {
                                    //ignore
                                    DEBUG_result = 1;
                                }
                                else
                                {
                                    Destroy(cliffSimilar.cliffGO);


                                    instantiated = CreateCliffObject(template, worldPos, $"Tile_{worldPos.ToInt()}_{tileSet1}({(Direction_TileCheck)indexDir})[{d1}]");
                                    cliffSimilar.cliffGO = instantiated;
                                    DEBUG_result = 2;

                                }

                            }
                            else
                            {

                                instantiated = CreateCliffObject(template, worldPos, $"Tile_{worldPos.ToInt()}_{tileSet1}({(Direction_TileCheck)indexDir})[{d1}]");
                                CliffObjectDat cod = new CliffObjectDat(worldPos, tileSet1, instantiated);

                                vd3.Add(cod);
                                DEBUG_result = 3;
                            }

                            if (DEBUG_lastSecondChecked != Time.time.ToInt())
                            {
                                //Debug.Log($"mypos: {x} {y} | {coord}");
                            }

                        }
                        //remove
                        else if (template == null)
                        {

                            if (cliffSimilar != null)
                            {
                                if (cliffSimilar.cliffGO != null) Destroy(cliffSimilar.cliffGO);
                            }
                        }


                        if (DEBUG_lastSecondChecked != Time.time.ToInt())
                        {
                            //Debug.Log($"{i} : ({x}, {y}) ({coord}) [result: {DEBUG_result}] [tileset: {tilesetTarget}]");
                        }
                    }
                   

                    indexDir++;
                }

                if (DEBUG_lastSecondChecked != Time.time.ToInt())
                {
                    //Debug.Log($"{i} : ({x}, {y})");
                }
            }

            if (Time.time % 2 == 0) DEBUG_lastSecondChecked = Time.time.ToInt();

            vd3.RemoveAll(f => f.cliffGO == null);
            cliffObjects.RemoveAll(x => x == null);
        }

        private List<GameObject> debug_listAllNoNeighborObjs = new List<GameObject>();

        public GameObject CreateCliffObject(GameObject template, Vector3 worldPos, string goName)
        {


            var cliffnewObj = Instantiate(template);
            Vector3 cliffPos = worldPos;

            cliffnewObj.transform.position = cliffPos;
            cliffnewObj.gameObject.name = goName;


            return cliffnewObj;

        }





        #endregion

        public Vector2Int IndexDirOffset(int indexDir)
        {
            Vector2Int offsetPos1 = new Vector2Int(0, 0);
            Vector2Int offsetPos2 = new Vector2Int(1, 0);
            Vector2Int offsetPos3 = new Vector2Int(0, 1);
            Vector2Int offsetPos4 = new Vector2Int(1, 1);

            if (indexDir == 0) return offsetPos1;
            if (indexDir == 1) return offsetPos2;
            if (indexDir == 2) return offsetPos3;
            if (indexDir == 3) return offsetPos4;
            return offsetPos1;
        }

        public enum Direction_TileCheck
        {
            Southwest,
            Southeast,
            Northwest,
            Northeast
        }

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

            if (dir == Direction_TileCheck.Southwest)
            {
                int maxHeight = Mathf.Max(new int[] { south.cliffLevel, southwest.cliffLevel, west.cliffLevel, myPos_cliff.cliffLevel});
       

                for(int x = 0; x <= maxHeight; x++)
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
                        tilesetList.Add(SO_TerrainPreset.Tileset.SharpCornerNorthWest);
                    }
                    if (south_cliffLV > x && southwest_cliffLV > x && west_cliffLV > x && myCliffLV <= x)
                    {
                        tilesetList.Add(SO_TerrainPreset.Tileset.SharpCornerNorthEast);
                    }
                    if (south_cliffLV > x && southwest_cliffLV <= x && west_cliffLV > x && myCliffLV > x)
                    {
                        tilesetList.Add(SO_TerrainPreset.Tileset.SharpCornerSouthWest);
                    }
                    if (south_cliffLV <= x && southwest_cliffLV > x && west_cliffLV > x && myCliffLV > x)
                    {
                        tilesetList.Add(SO_TerrainPreset.Tileset.SharpCornerSouthEast);
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
                        tilesetList.Add(SO_TerrainPreset.Tileset.SharpCornerNorthWest);
                    }
                    if (south_cliffLV > x && southeast_cliffLV > x && east_cliffLV <= x && myCliffLV > x)
                    {
                        tilesetList.Add(SO_TerrainPreset.Tileset.SharpCornerNorthEast);
                    }
                    if (south_cliffLV <= x && southeast_cliffLV > x && east_cliffLV > x && myCliffLV > x)
                    {
                        tilesetList.Add(SO_TerrainPreset.Tileset.SharpCornerSouthWest);
                    }
                    if (south_cliffLV > x && southeast_cliffLV <= x && east_cliffLV > x && myCliffLV > x)
                    {
                        tilesetList.Add(SO_TerrainPreset.Tileset.SharpCornerSouthEast);
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
                        tilesetList.Add(SO_TerrainPreset.Tileset.SharpCornerNorthWest);
                    }
                    if (north_cliffLV <= x && northwest_cliffLV > x && west_cliffLV > x && myCliffLV > x)
                    {
                        tilesetList.Add(SO_TerrainPreset.Tileset.SharpCornerNorthEast);
                    }
                    if (north_cliffLV > x && northwest_cliffLV > x && west_cliffLV <= x && myCliffLV > x)
                    {
                        tilesetList.Add(SO_TerrainPreset.Tileset.SharpCornerSouthWest);
                    }
                    if (north_cliffLV > x && northwest_cliffLV > x && west_cliffLV > x && myCliffLV <= x)
                    {
                        tilesetList.Add(SO_TerrainPreset.Tileset.SharpCornerSouthEast);
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
                        tilesetList.Add(SO_TerrainPreset.Tileset.SharpCornerNorthWest);
                    }
                    if (north_cliffLV > x && northeast_cliffLV <= x && east_cliffLV > x && myCliffLV > x)
                    {
                        tilesetList.Add(SO_TerrainPreset.Tileset.SharpCornerNorthEast);
                    }
                    if (north_cliffLV > x && northeast_cliffLV > x && east_cliffLV > x && myCliffLV <= x)
                    {
                        tilesetList.Add(SO_TerrainPreset.Tileset.SharpCornerSouthWest);
                    }
                    if (north_cliffLV > x && northeast_cliffLV > x && east_cliffLV <= x && myCliffLV > x)
                    {
                        tilesetList.Add(SO_TerrainPreset.Tileset.SharpCornerSouthEast);
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

        private void Update()
        {



        }




    }
}