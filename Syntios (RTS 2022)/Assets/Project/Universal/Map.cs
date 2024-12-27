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
            int width = (_terrainData.size_x * 2) / 3;
            int depth = (_terrainData.size_y * 2) / 3;
            width -= 2;
            depth -= 2;
            var center = WorldPosCenter;
            center.x += 1 * 3;
            center.z += 0 * 3;

            gridGraph.SetDimensions(width, depth, 3);
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

        #endregion


        private void Update()
        {

     

        }




    }
}