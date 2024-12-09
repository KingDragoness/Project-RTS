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
        [FoldoutGroup("References")] public Texture2D texture_FOWDefault;
        [FoldoutGroup("References")] [SerializeField] private Shader terrainShader;
        [FoldoutGroup("References")] [SerializeField] private Material _sourceTerrainMat;

        [FoldoutGroup("FOW")] [Range(0f,1f)] public float UpdateFOWTimer = 0.25f;

        [DisableInEditorMode] [SerializeField] private Material generatedTerrainMaterial;
        private Texture2D texture_FogOfWar;
        private float _FOWTimer = 0.1f;

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
            GenerateBlankFOW();
            GenerateMaterial();
            DEBUG_MeshTerrain.material = generatedTerrainMaterial;
        }


        private void GenerateMaterial()
        {
            generatedTerrainMaterial = new Material(terrainShader);
            var textureSplatTest = _sourceTerrainMat.GetTexture("_SplatMap");
            var float_depth = _sourceTerrainMat.GetFloat("_Depth");

            generatedTerrainMaterial.SetTexture("_SplatMap", textureSplatTest);
            generatedTerrainMaterial.SetTexture("_GroundTexture", MyPreset.ground);
            generatedTerrainMaterial.SetTexture("_FOWMap", texture_FogOfWar);
            generatedTerrainMaterial.SetTexture("_TextureA", MyPreset.layer1);
            generatedTerrainMaterial.SetTexture("_TextureB", MyPreset.layer2);
            generatedTerrainMaterial.SetTexture("_TextureC", MyPreset.layer3);
            generatedTerrainMaterial.SetTexture("_TextureD", MyPreset.layer4);
            generatedTerrainMaterial.SetTexture("_TextureE", MyPreset.layer5);
            generatedTerrainMaterial.SetTexture("_TextureF", MyPreset.layer6);
            generatedTerrainMaterial.SetTexture("_TextureG", MyPreset.layer7);
            generatedTerrainMaterial.SetTexture("_TextureH", MyPreset.layer8);
            generatedTerrainMaterial.SetFloat("_TextureScale", 0.1f);
            generatedTerrainMaterial.SetFloat("_SplatmapScale", 0.195f);
            generatedTerrainMaterial.SetFloat("_FOWSampleRadiusBlur", 0.0008f);
            generatedTerrainMaterial.SetFloat("_Depth", float_depth);

        }


        private void GenerateBlankFOW()
        {
            texture_FogOfWar = new Texture2D(texture_FOWDefault.width, texture_FOWDefault.height);
            texture_FogOfWar.SetPixels(texture_FOWDefault.GetPixels());

            texture_FogOfWar.Apply();

        }

        [FoldoutGroup("DEBUG")] [Button("Test Reveal Map")]
        public void UpdateFOW(Unit.Player faction)
        {
            var myFactionUnits = SyntiosEngine.Instance.ListedGameUnits.FindAll(x => x.stat_faction == faction);

            foreach (var gameUnit in myFactionUnits)
            {

            }

            //texture_FogOfWar.SetPixels(texture_FOWDefault.GetPixels());

            //Color color_Revealed = new Color(1, 1, 1, 1);
            //Color color_Unexplored = new Color(0.5f, 0.5f, 0.5f, 1f);

            //List<Vector2Int> positionToModify = new List<Vector2Int>();

            //foreach (var gameUnit in myFactionUnits)
            //{
            //    if (gameUnit.stat_faction != faction) continue;

            //    Vector2Int posCenter = ConvertWorldPosToFOWPos(gameUnit.transform.position);

            //    int radius = (gameUnit.Class.LineOfSight) * 4;
            //    if (radius <= 2) radius = 2;
            //    int r2 = radius * radius;

            //    int mid = radius / 2;
            //    Color[] allColors = new Color[r2];

            //    for (int z1 = 0; z1 < allColors.Length; z1++)
            //    {
            //        int pixelX = Mathf.FloorToInt(z1 % radius);
            //        int pixelY = Mathf.FloorToInt(z1 / radius);
            //        Vector2Int currPixel = posCenter;
            //        currPixel.x -= mid;
            //        currPixel.y -= mid;
            //        currPixel.x += pixelX;
            //        currPixel.y += pixelY;

            //        if (Vector2.Distance(currPixel, posCenter) > radius / 2)
            //        {
            //            //allColors[z1] = texture_FogOfWar.GetPixel(currPixel.x, currPixel.y);
            //            continue;
            //        }

            //        positionToModify.Add(currPixel);
            //        //allColors[z1] = color_Revealed;
            //    }
            //}

            //foreach(var p in positionToModify)
            //{
            //    texture_FogOfWar.SetPixel(p.x, p.y, color_Revealed);
            //}

            //texture_FogOfWar.Apply();
        }

        private void Update()
        {

            if (_FOWTimer > 0f)
            {
                _FOWTimer -= Time.deltaTime;

            }
            else
            {
                _FOWTimer = UpdateFOWTimer;

                UpdateFOW(SyntiosEngine.Instance.CurrentFaction);
            }

        }


        //1024 = pixel of FOW
        public Vector2Int ConvertWorldPosToFOWPos(Vector3 worldPosition)
        {
            Vector2Int result = new Vector2Int();
            result.x = Mathf.Lerp(0, 1024f, (worldPosition.x / 256f / 2f)).ToInt();
            result.y = Mathf.Lerp(0, 1024f, (worldPosition.z / 256f / 2f)).ToInt();

            return result;
        }


        public int ConvertPixelPosToColorArrayIndex(Vector2Int pixelPos)
        {
            int width = texture_FogOfWar.width;
            int height = texture_FogOfWar.height;

            return (width * pixelPos.y) + pixelPos.x;
        }
   

    }
}