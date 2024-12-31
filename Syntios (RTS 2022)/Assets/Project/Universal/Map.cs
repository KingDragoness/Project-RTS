using System.Collections;
using System.Collections.Generic;
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

        public bool DEBUG_dontInitializeData;
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
            instance._partialUpdateMap(x,y,width,length);
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
                    int index = _terrainData.GetSplatmapIndex(x,y) + _terrainData.GetSplatmapIndex(x1, y1);

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


        public bool DEBUG_SeeCliff = false;
        [ShowInInspector] Dictionary<Vector2Int, GameObject> vd3 = new Dictionary<Vector2Int, GameObject>();
        [ShowInInspector] private List<GameObject> cliffObjects = new List<GameObject>();


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

            for (int i = startingIndex; i < finalIndex; i++)
            {
                int x = i % _terrainData.size_x;
                int y = i / _terrainData.size_y;

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
                bool north = _terrainData.IsNeighborValid(SyntiosTerrainData.DirectionNeighbor.North, myPos);
                bool south = _terrainData.IsNeighborValid(SyntiosTerrainData.DirectionNeighbor.South, myPos);
                bool west = _terrainData.IsNeighborValid(SyntiosTerrainData.DirectionNeighbor.West, myPos);
                bool east = _terrainData.IsNeighborValid(SyntiosTerrainData.DirectionNeighbor.East, myPos);
                bool northwest = _terrainData.IsNeighborValid(SyntiosTerrainData.DirectionNeighbor.NorthWest, myPos);
                bool northeast = _terrainData.IsNeighborValid(SyntiosTerrainData.DirectionNeighbor.NorthEast, myPos);
                bool southwest = _terrainData.IsNeighborValid(SyntiosTerrainData.DirectionNeighbor.SouthWest, myPos);
                bool southeast = _terrainData.IsNeighborValid(SyntiosTerrainData.DirectionNeighbor.SouthEast, myPos);


                int indexDir = 0;

                //atlas coord
                foreach (var coord in myPosArray)
                {
                    GameObject instantiated = null;
                    var tilesetTarget = GetTileSet(myPos, indexDir);

                    if (_terrainData.cliffLevel[i] < 1)
                    {
                        tilesetTarget = SO_TerrainPreset.Tileset.Null;
                    }

                    Vector3 worldPos = new Vector3();
                    worldPos.x = (coord.x * 2);
                    worldPos.y = _terrainData.cliffLevel[i] * 2;
                    worldPos.z = (coord.y * 2);

                    var template = MyPreset.GetManmadeCliff(tilesetTarget);            

                    if (template != null)
                    {
                        if (vd3.ContainsKey(coord))
                        {
                            Destroy(vd3[coord].gameObject);
                            instantiated = Instantiate(template);
                            instantiated.transform.position = worldPos;
                            instantiated.gameObject.name = $"Tile_{coord}";
                            vd3[coord] = instantiated;
                        }
                        else
                        {
                            instantiated = Instantiate(template);
                            instantiated.transform.position = worldPos;
                            instantiated.gameObject.name = $"Tile_{coord}";
                            vd3.Add(coord, instantiated);
                            cliffObjects.Add(instantiated);
                        }

                    }
                    else
                    {

                    }


                    indexDir++;
                }

            }

        }


        //This will not work because you need to update newly ones too
        private void PartialUpdateCliffMaps(int startX = 0, int startY = 0, int width = 256, int height = 256)
        {
            Vector2Int offsetPos1 = new Vector2Int(0, 0);
            Vector2Int offsetPos2 = new Vector2Int(1, 0);
            Vector2Int offsetPos3 = new Vector2Int(0, 1);
            Vector2Int offsetPos4 = new Vector2Int(1, 1);

            foreach (var tile in vd3)
            {
                Vector2Int myPos = tile.Key;

                Vector2Int[] myPosArray = new Vector2Int[4]
                {
                    myPos + offsetPos1,
                    myPos + offsetPos2,
                    myPos + offsetPos3,
                    myPos + offsetPos4
                };

                //check type
                bool north = _terrainData.IsNeighborValid(SyntiosTerrainData.DirectionNeighbor.North, myPos);
                bool south = _terrainData.IsNeighborValid(SyntiosTerrainData.DirectionNeighbor.South, myPos);
                bool west = _terrainData.IsNeighborValid(SyntiosTerrainData.DirectionNeighbor.West, myPos);
                bool east = _terrainData.IsNeighborValid(SyntiosTerrainData.DirectionNeighbor.East, myPos);
                bool northwest = _terrainData.IsNeighborValid(SyntiosTerrainData.DirectionNeighbor.NorthWest, myPos);
                bool northeast = _terrainData.IsNeighborValid(SyntiosTerrainData.DirectionNeighbor.NorthEast, myPos);
                bool southwest = _terrainData.IsNeighborValid(SyntiosTerrainData.DirectionNeighbor.SouthWest, myPos);
                bool southeast = _terrainData.IsNeighborValid(SyntiosTerrainData.DirectionNeighbor.SouthEast, myPos);

                int indexDir = 0;

                //atlas coord
                foreach (var coord in myPosArray)
                {
                    var currentTile = GetTileSet(myPos, indexDir);
                    var template = MyPreset.GetManmadeCliff(currentTile);

                    Vector3 worldPos = new Vector3();
                    worldPos.x = (coord.x * 2);
                    //worldPos.y = _terrainData.cliffLevel[] * 2;
                    worldPos.z = (coord.y * 2);

                    if (template != null)
                    {
                        //attempt replace tile
                        vd3[coord] = Instantiate(template);
                        vd3[coord].transform.position = worldPos;
                    }

                    indexDir++;                 
                }
            }
        }



        #endregion

        public SO_TerrainPreset.Tileset GetTileSet(Vector2Int myPos, int indexDir)
        {
            bool north = _terrainData.IsNeighborValid(SyntiosTerrainData.DirectionNeighbor.North, myPos);
            bool south = _terrainData.IsNeighborValid(SyntiosTerrainData.DirectionNeighbor.South, myPos);
            bool west = _terrainData.IsNeighborValid(SyntiosTerrainData.DirectionNeighbor.West, myPos);
            bool east = _terrainData.IsNeighborValid(SyntiosTerrainData.DirectionNeighbor.East, myPos);
            bool northwest = _terrainData.IsNeighborValid(SyntiosTerrainData.DirectionNeighbor.NorthWest, myPos);
            bool northeast = _terrainData.IsNeighborValid(SyntiosTerrainData.DirectionNeighbor.NorthEast, myPos);
            bool southwest = _terrainData.IsNeighborValid(SyntiosTerrainData.DirectionNeighbor.SouthWest, myPos);
            bool southeast = _terrainData.IsNeighborValid(SyntiosTerrainData.DirectionNeighbor.SouthEast, myPos);

            SO_TerrainPreset.Tileset tilesetTarget = SO_TerrainPreset.Tileset.Null;

            if (indexDir == 2)
            {
                if (!west && !northwest && !north)
                {
                    tilesetTarget = SO_TerrainPreset.Tileset.CornerNorthWest;
                }

                if (west && !northwest && !north)
                {
                    tilesetTarget = SO_TerrainPreset.Tileset.North;
                }

                if (!west && !northwest && north)
                {
                    tilesetTarget = SO_TerrainPreset.Tileset.West;
                }

                if (west && !northwest && north)
                {
                    tilesetTarget = SO_TerrainPreset.Tileset.SharpCornerNorthWest;
                }

                if (!west && northwest && !north)
                {
                    tilesetTarget = SO_TerrainPreset.Tileset.DiagonalWest;
                }

                if (west && northwest && north)
                {
                    tilesetTarget = SO_TerrainPreset.Tileset.Flat;
                }
            }
            else if (indexDir == 3)
            {
                if (!east && !northeast && !north)
                {
                    tilesetTarget = SO_TerrainPreset.Tileset.CornerNorthEast;
                }

                if (east && !northeast && !north)
                {
                    tilesetTarget = SO_TerrainPreset.Tileset.North;
                }

                if (!east && !northeast && north)
                {
                    tilesetTarget = SO_TerrainPreset.Tileset.East;
                }

                if (east && !northeast && north)
                {
                    tilesetTarget = SO_TerrainPreset.Tileset.SharpCornerNorthEast;
                }

                if (!east && northeast && !north)
                {
                    tilesetTarget = SO_TerrainPreset.Tileset.DiagonalEast;
                }

                if (east && northeast && north)
                {
                    tilesetTarget = SO_TerrainPreset.Tileset.Flat;
                }
            }
            else if (indexDir == 1)
            {
                if (!east && !southeast && !south)
                {
                    tilesetTarget = SO_TerrainPreset.Tileset.CornerSouthEast;
                }

                if (east && !southeast && !south)
                {
                    tilesetTarget = SO_TerrainPreset.Tileset.South;
                }

                if (!east && !southeast && south)
                {
                    tilesetTarget = SO_TerrainPreset.Tileset.East;
                }

                if (east && !southeast && south)
                {
                    tilesetTarget = SO_TerrainPreset.Tileset.SharpCornerSouthEast;
                }

                if (!east && southeast && !south)
                {
                    tilesetTarget = SO_TerrainPreset.Tileset.DiagonalEast;
                }

                if (east && southeast && south)
                {
                    tilesetTarget = SO_TerrainPreset.Tileset.Flat;
                }
            }
            else if (indexDir == 0)
            {
                if (!west && !southwest && !south)
                {
                    tilesetTarget = SO_TerrainPreset.Tileset.CornerSouthWest;
                }

                if (west && !southwest && !south)
                {
                    tilesetTarget = SO_TerrainPreset.Tileset.South;
                }

                if (!west && !southwest && south)
                {
                    tilesetTarget = SO_TerrainPreset.Tileset.West;
                }

                if (west && !southwest && south)
                {
                    tilesetTarget = SO_TerrainPreset.Tileset.SharpCornerSouthWest;
                }

                if (!west && southwest && !south)
                {
                    tilesetTarget = SO_TerrainPreset.Tileset.DiagonalWest;
                }

                if (west && southwest && south)
                {
                    tilesetTarget = SO_TerrainPreset.Tileset.Flat;
                }
            }

            return tilesetTarget;
        }

        private void Update()
        {

     

        }




    }
}