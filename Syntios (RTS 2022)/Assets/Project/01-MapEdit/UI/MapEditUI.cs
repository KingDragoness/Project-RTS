using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

namespace ProtoRTS.MapEditor
{
	public class MapEditUI : MonoBehaviour
	{

		public Text label_BrushType;
		public Toggle tg_Texture;
		public Toggle tg_Cliffs;
		[FoldoutGroup("Brush: Texture")] public Text label_TextureOperation;
		[FoldoutGroup("Brush: Texture")] public GameObject uiPanel_Texture;
		[FoldoutGroup("Brush: Texture")] public GameObject uiPanel_Texture_brushSettings;
		[FoldoutGroup("Brush: Texture")] public Toggle tg_Texture_Add;
		[FoldoutGroup("Brush: Texture")] public Toggle tg_Texture_Minus;
		[FoldoutGroup("Brush: Texture")] public Toggle tg_Texture_Uniform;
		[FoldoutGroup("Brush: Texture")] public Text label_Texture_Layer;
		[FoldoutGroup("Brush: Texture")] public Toggle prefab_tg_TextureLayer;
		[FoldoutGroup("Brush: Texture")] public RectTransform transform_LayerParent;
		[FoldoutGroup("Brush: Texture")] [ShowInInspector] private List<Toggle> allTextureToggles = new List<Toggle>();
		[FoldoutGroup("Brush: Texture/Settings")] public Toggle tg_Texture_Shape_Circle;
		[FoldoutGroup("Brush: Texture/Settings")] public Toggle tg_Texture_Shape_Rect;
		[FoldoutGroup("Brush: Texture/Settings")] public Toggle tg_Texture_Shape_Diagonal;
		[FoldoutGroup("Brush: Texture/Settings")] public Slider slider_Texture_BrushSize;
		[FoldoutGroup("Brush: Texture/Settings")] public Slider slider_Texture_BrushStrength;
		[FoldoutGroup("Brush: Texture/Settings")] public Toggle tg_Texture_PerlinNoise;
		[FoldoutGroup("Brush: Texture/Perlin Noise")] public Slider slider_Texture_PerlinMult;
		[FoldoutGroup("Brush: Texture/Perlin Noise")] public GameObject panel_PerlinNoise;

		[FoldoutGroup("Brush: Cliffs")] public Text label_CliffsOperation;
		[FoldoutGroup("Brush: Cliffs")] public GameObject uiPanel_Cliffs;
		[FoldoutGroup("Brush: Cliffs")] public GameObject uiPanel_Cliffs_brushSettings;
		[FoldoutGroup("Brush: Cliffs")] public Toggle tg_Cliffs_Raise;
		[FoldoutGroup("Brush: Cliffs")] public Toggle tg_Cliffs_Lower;
		[FoldoutGroup("Brush: Cliffs")] public Toggle tg_Cliffs_SameLevel;
		[FoldoutGroup("Brush: Cliffs/Settings")] public Toggle tg_Cliffs_Shape_Circle;
		[FoldoutGroup("Brush: Cliffs/Settings")] public Toggle tg_Cliffs_Shape_Rect;
		[FoldoutGroup("Brush: Cliffs/Settings")] public Toggle tg_Cliffs_Shape_Diagonal;
		[FoldoutGroup("Brush: Cliffs/Settings")] public Slider slider_Cliffs_BrushSize;
		[FoldoutGroup("Brush: Cliffs/Settings")] public Toggle tg_Cliffs_Manmade;

		private void Start()
		{
			uiPanel_Texture.gameObject.SetActive(false);
			uiPanel_Cliffs.gameObject.SetActive(false);
			uiPanel_Texture_brushSettings.gameObject.SetActive(false);
			uiPanel_Cliffs_brushSettings.gameObject.SetActive(false);
			Texture_CreateButtonLayers();
		}

		private void Texture_CreateButtonLayers()
        {
			for(int x = -1; x < 8; x++)
            {
				var newToggle = Instantiate(prefab_tg_TextureLayer, transform_LayerParent);
				newToggle.gameObject.name = $"Toggle_Layer{x}";
				newToggle.gameObject.SetActive(true);
				newToggle.transform.localPosition = Vector3.zero;
				var rawImage = newToggle.transform.GetComponentInChildren<RawImage>();

				rawImage.texture = Map.instance.MyPreset.GetLayer(x);
				allTextureToggles.Add(newToggle);
			}

			slider_Texture_BrushSize.wholeNumbers = true;
			slider_Texture_BrushSize.minValue = 2;
			slider_Texture_BrushSize.maxValue = 32;

			slider_Texture_BrushStrength.wholeNumbers = true;
			slider_Texture_BrushStrength.minValue = 25;
			slider_Texture_BrushStrength.maxValue = 255;

		}

		private void Update()
		{
			var currBrush = MapEdit.instance.CurrentBrush;

			if (currBrush != null)
            {
				label_BrushType.text = $"<b>Brush:</b> {currBrush.GetBrushName()}";

			}
            else
            {
				label_BrushType.text = $"<b>Brush:</b>";
			}

			if (tg_Texture.isOn)
            {
				SwitchBrush(1);
				Running_TextureBrush();

			}
			else if (tg_Cliffs.isOn)
			{
				SwitchBrush(2);
				Running_CliffsBrush();
			}
            else
            {
				SwitchBrush(0);
            }

		}



		public void SwitchBrush(int type)
        {
			MapEdit.instance.SetCurrentTool(type);

			if (type == 1)
            {
				uiPanel_Texture.gameObject.SetActive(true);
				uiPanel_Cliffs.gameObject.SetActive(false);
				uiPanel_Cliffs_brushSettings.gameObject.SetActive(false);

			}
			else if (type == 2)
			{
				uiPanel_Texture.gameObject.SetActive(false);
				uiPanel_Cliffs.gameObject.SetActive(true);
				uiPanel_Texture_brushSettings.gameObject.SetActive(false);
			}
			else
            {
				uiPanel_Texture.gameObject.SetActive(false);
				uiPanel_Texture_brushSettings.gameObject.SetActive(false);
				uiPanel_Cliffs.gameObject.SetActive(false);
				uiPanel_Cliffs_brushSettings.gameObject.SetActive(false);
			}
		}

		private void Running_TextureBrush()
        {
			if (MapEdit.instance.CurrentBrush != MapEdit.instance.BrushTexture) return; //mismatched
			var preset = Map.instance.MyPreset;
			label_TextureOperation.text = $"<b>Operation:</b> {MapEdit.instance.BrushTexture.currentOperation.ToString()}";

			if (tg_Texture_Add.isOn)
			{
				MapEdit.instance.BrushTexture.currentOperation = MapTool_BrushTexture.Operation.Add;
			}
			else if (tg_Texture_Minus.isOn)
			{
				MapEdit.instance.BrushTexture.currentOperation = MapTool_BrushTexture.Operation.Subtract;
			}
			else if (tg_Texture_Uniform.isOn)
			{
				MapEdit.instance.BrushTexture.currentOperation = MapTool_BrushTexture.Operation.Uniform;
			}
			else
            {
				MapEdit.instance.BrushTexture.currentOperation = MapTool_BrushTexture.Operation.None;
			}


			if (MapEdit.instance.BrushTexture.currentOperation == MapTool_BrushTexture.Operation.None)
			{
				uiPanel_Texture_brushSettings.gameObject.SetActive(false);
			}
			else
			{
				label_Texture_Layer.text = $"<b>Layer:</b>";
				uiPanel_Texture_brushSettings.gameObject.SetActive(true);

				//has texture
				{
					int index = GetIndexToggle(allTextureToggles) - 1;

					if (index != -2)
					{
						label_Texture_Layer.text = $"<b>Layer:</b> {preset.GetLayerName(index)}";
						MapEdit.instance.BrushTexture.brushCurrent = index;
					}

				}

				//shape
				{
					if (tg_Texture_Shape_Circle.isOn)
					{
						MapEdit.instance.BrushTexture.isMaskByDistance = true;

					}
					else if (tg_Texture_Shape_Rect.isOn)
					{
						MapEdit.instance.BrushTexture.isMaskByDistance = false;

					}

					if (tg_Texture_PerlinNoise.isOn)
					{
						MapEdit.instance.BrushTexture.isPerlinNoise = true;

					}
					else
					{
						MapEdit.instance.BrushTexture.isPerlinNoise = false;

					}
				}

				if (MapEdit.instance.BrushTexture.isPerlinNoise)
                {
					panel_PerlinNoise.gameObject.EnableGameobject(true);
					MapEdit.instance.BrushTexture.perlinMultiplier = slider_Texture_PerlinMult.value;
				}
                else
                {
					panel_PerlinNoise.gameObject.EnableGameobject(false);
				}

				MapEdit.instance.BrushTexture.brushSize = slider_Texture_BrushSize.value.ToInt();
				MapEdit.instance.BrushTexture.brushStrength = slider_Texture_BrushStrength.value.ToInt();

			}

		}

		private void Running_CliffsBrush()
		{
			if (MapEdit.instance.CurrentBrush != MapEdit.instance.BrushCliffs) return; //mismatched
			var preset = Map.instance.MyPreset;

			label_CliffsOperation.text = $"<b>Operation:</b> {MapEdit.instance.BrushCliffs.currentOperation.ToString()}";

			if (tg_Cliffs_Raise.isOn)
			{
				MapEdit.instance.BrushCliffs.currentOperation = MapTool_Cliffs.Operation.Raise;
			}
			else if (tg_Cliffs_Lower.isOn)
			{
				MapEdit.instance.BrushCliffs.currentOperation = MapTool_Cliffs.Operation.Lower;
			}
			else if (tg_Cliffs_SameLevel.isOn)
			{
				MapEdit.instance.BrushCliffs.currentOperation = MapTool_Cliffs.Operation.SameLevel;
			}
			else
			{
				MapEdit.instance.BrushCliffs.currentOperation = MapTool_Cliffs.Operation.None;
			}


			if (MapEdit.instance.BrushCliffs.currentOperation == MapTool_Cliffs.Operation.None)
			{
				uiPanel_Cliffs_brushSettings.gameObject.SetActive(false);
			}
			else
			{
				label_Texture_Layer.text = $"<b>Layer:</b>";
				uiPanel_Cliffs_brushSettings.gameObject.SetActive(true);

				//shape
				{
					if (tg_Cliffs_Shape_Circle.isOn)
					{
						MapEdit.instance.BrushCliffs.isMaskByDistance = true;

					}
					else if (tg_Cliffs_Shape_Rect.isOn)
					{
						MapEdit.instance.BrushCliffs.isMaskByDistance = false;

					}
				}

				if (tg_Cliffs_Manmade.isOn)
				{
					MapEdit.instance.BrushCliffs.isManmade = true;
				}
                else
                {
					MapEdit.instance.BrushCliffs.isManmade = false;
				}
				MapEdit.instance.BrushCliffs.brushSize = slider_Cliffs_BrushSize.value.ToInt();

			}
		}

		public int GetIndexToggle(List<Toggle> allToggles)
        {
			int index = -1;

			foreach(var toggle in allTextureToggles)
            {
				index++;

				if (toggle.isOn)
				{
					return index;
				}

			}

			return index;
        }
		
		private void OnEnable()
		{
			
		}
	}
}