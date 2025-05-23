using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

namespace ProtoRTS.Game
{
	public class UnitBar_HPSPMP_Element : MonoBehaviour
	{

		public GameUnit attachedGameUnit;
        public RectTransform mainElement;
        public Slider slider_HP;
        public Slider slider_SP;
        public Slider slider_Energy;
        public Image image_HP;

        [Button("Refresh")]
        public void InitializeElement(GameUnit gameunit)
        {
            if (Application.isPlaying == false) return;
            if (gameunit == null) return;
            attachedGameUnit = gameunit;

            mainElement.sizeDelta = new Vector2(width_element(gameunit), mainElement.sizeDelta.y);

            if (attachedGameUnit.Class.HasEnergy) slider_Energy.gameObject.SetActive(true); else slider_Energy.gameObject.SetActive(false);
            if (attachedGameUnit.Class.HasShield) slider_SP.gameObject.SetActive(true); else slider_SP.gameObject.SetActive(false);
            ImmediateCalculatePosition();

        }

        public void ImmediateCalculatePosition()
        {
            var cam = RTSCamera.Instance.MainCamera;

            Vector3 psuedoRealPos = attachedGameUnit.gameObject.transform.position;
            psuedoRealPos.y += 3f;
            if (attachedGameUnit.Class.IsFlyUnit) psuedoRealPos.y += 3f;
            psuedoRealPos.z += attachedGameUnit.Class.Radius;

            Vector3 screenPos = cam.WorldToScreenPoint(psuedoRealPos);
            screenPos.z = 0;
            mainElement.anchoredPosition = screenPos;
        }

        public int width_element(GameUnit gameunit)
        {
            int box_width = 2; //maxed at 20 boxes or 850 HP
            int l = Mathf.FloorToInt(gameunit.Class.Radius * 3.25f);
            if (gameunit.Class.AllUnitTags.Contains(Unit.Tag.Structure)) l += 1;
            box_width += Mathf.Clamp(l, 1, 17);
            box_width *= 15;

            return box_width + 1;
        }


        private void Update()
        {
            if (attachedGameUnit == null)
            {
                gameObject.SetActive(false);
                return;

            }

            float hpPercent = (float)attachedGameUnit.stat_HP / (float)attachedGameUnit._class.MaxHP;
            Color gradientColor = UI.UnitSelection.gradientWireframeHP.Evaluate(hpPercent);
            gradientColor.r -= 0.1f;
            gradientColor.g -= 0.1f;
            gradientColor.b -= 0.1f;

            slider_HP.value = attachedGameUnit.stat_HP;
            slider_HP.maxValue = attachedGameUnit._class.MaxHP;
            slider_SP.value = attachedGameUnit.stat_Shield;
            slider_SP.maxValue = attachedGameUnit._class.MaxShield;
            slider_Energy.value = attachedGameUnit.stat_Energy;
            slider_Energy.maxValue = attachedGameUnit._class.MaxMana();
            image_HP.color = gradientColor;

            var cam = RTSCamera.Instance.MainCamera;

            Vector3 psuedoRealPos = attachedGameUnit.gameObject.transform.position;
            psuedoRealPos.y += 3f;
            if (attachedGameUnit.Class.IsFlyUnit) psuedoRealPos.y += 3f;
            psuedoRealPos.z += attachedGameUnit.Class.Radius;

            Vector3 screenPos = cam.WorldToScreenPoint(psuedoRealPos);
            screenPos.z = 0;
            mainElement.anchoredPosition = screenPos;

        }

    }
}