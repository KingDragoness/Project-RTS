using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Sirenix.OdinInspector;

namespace ProtoRTS
{
	public class Map : MonoBehaviour
	{

        public List<SO_TerrainPreset> allVanillaTerrainPresets = new List<SO_TerrainPreset>();
		[Space]
        [SerializeField] private SyntiosTerrainData _terrainData;

        [FoldoutGroup("References")] public MeshRenderer DEBUG_MeshTerrain;
        [FoldoutGroup("References")] [SerializeField] private Shader terrainShader;
        [FoldoutGroup("References")] [SerializeField] private Material _sourceTerrainMat;


        [DisableInEditorMode] [SerializeField] private Material generatedTerrainMaterial;


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

        public static SyntiosTerrainData TerrainData
        {
            get { return instance._terrainData; }
        }


        public static Map instance;

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            InitializeMap();
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
            DEBUG_MeshTerrain.material = generatedTerrainMaterial;
        }


        private void GenerateMaterial()
        {
            generatedTerrainMaterial = new Material(_sourceTerrainMat);
            generatedTerrainMaterial.name = "GeneratedTerrainMat";
            var textureSplatTest = _sourceTerrainMat.GetTexture("_SplatMap");

            generatedTerrainMaterial.SetTexture("_SplatMap", textureSplatTest);
            generatedTerrainMaterial.SetTexture("_GroundTexture", MyPreset.ground);
            generatedTerrainMaterial.SetTexture("_TextureA", MyPreset.layer1);
            generatedTerrainMaterial.SetTexture("_TextureB", MyPreset.layer2);
            generatedTerrainMaterial.SetTexture("_TextureC", MyPreset.layer3);
            generatedTerrainMaterial.SetTexture("_TextureD", MyPreset.layer4);
            generatedTerrainMaterial.SetTexture("_TextureE", MyPreset.layer5);
            generatedTerrainMaterial.SetTexture("_TextureF", MyPreset.layer6);
            generatedTerrainMaterial.SetTexture("_TextureG", MyPreset.layer7);
            generatedTerrainMaterial.SetTexture("_TextureH", MyPreset.layer8);
           

        }




        private void Update()
        {

     

        }




    }
}