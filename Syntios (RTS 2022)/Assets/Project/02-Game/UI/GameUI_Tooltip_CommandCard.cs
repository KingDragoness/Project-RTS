using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using System;

namespace ProtoRTS.Game
{
	public class GameUI_Tooltip_CommandCard : MonoBehaviour
	{

        public Text label_Title;
        public Text label_Description;

        public GameObject resourcesGroup;
        public GameObject resource_Minerals;
        public Text label_Minerals;
        public GameObject resource_Gas;
        public Text label_Gas;
        public GameObject resource_Supply;
        public Text label_Supply;
        public GameObject resource_Time;
        public Text label_Time;
        [Space]
        public GameObject attachedButton_or_UI;


        public static GameUI_Tooltip_CommandCard Instance 
        { 
            get 
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<GameUI_Tooltip_CommandCard>(true);
                }
                return _instance; 
            } 
        }
        private static GameUI_Tooltip_CommandCard _instance;

        private int requirement_minerals = 0;
        private int requirement_gas = 0;
        private int requirement_supply = 0;


        private void Awake()
        {
            _instance = this;
            Tick.OnTick += event_OnTick;
            if (Time.timeSinceLevelLoad < 1f) gameObject.SetActive(false);
        }


        private void OnDestroy()
        {
            Tick.OnTick -= event_OnTick;
        }

        private void event_OnTick(int tick)
        {
            if (attachedButton_or_UI == null) 
            {
                CloseTooltip();
                return;
            }
            if (attachedButton_or_UI.gameObject.activeInHierarchy == false)
            {
                CloseTooltip();
            }
        }

        public void CloseTooltip()
        {
            gameObject.SetActive(false);
        }

        public void OpenTooltip(GameObject attachedUI)
        {
            label_Description.gameObject.SetActive(false);
            label_Title.gameObject.SetActive(false);
            resource_Minerals.gameObject.SetActive(false);
            resource_Gas.gameObject.SetActive(false);
            resource_Supply.gameObject.SetActive(false);
            resource_Time.gameObject.SetActive(false);
            resourcesGroup.gameObject.SetActive(false);

            label_Title.text = "";
            label_Description.text = "";

            gameObject.SetActive(true);
            attachedButton_or_UI = attachedUI;
        }

        public void Tooltip_Text(string title, string description)
        {
            label_Description.gameObject.SetActive(true);
            label_Title.gameObject.SetActive(true);
            label_Title.text = title;
            label_Description.text = description;
            if (label_Description.text == "") label_Description.gameObject.SetActive(false);
        }

        public void Show_Minerals(int value) 
        {
            resourcesGroup.gameObject.SetActive(true);
            resource_Minerals.gameObject.SetActive(true);
            label_Minerals.text = value.ToString();
            requirement_minerals = value;
        }

        public void Show_Gas(int value)
        {
            resourcesGroup.gameObject.SetActive(true);
            resource_Gas.gameObject.SetActive(true);
            label_Gas.text = value.ToString();
            requirement_gas = value;
        }

        public void Show_Supply(int value)
        {
            resourcesGroup.gameObject.SetActive(true);
            resource_Supply.gameObject.SetActive(true);
            label_Supply.text = value.ToString();
            requirement_supply = value;
        }

        public void Show_Time(int value)
        {
            resource_Time.gameObject.SetActive(true);
            label_Time.text = value.ToString();
        }
    }
}