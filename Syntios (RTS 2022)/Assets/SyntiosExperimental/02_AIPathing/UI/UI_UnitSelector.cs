using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using Sirenix.OdinInspector;
using ToolBox.Pools;

namespace ProtoRTS
{
	public class UI_UnitSelector : MonoBehaviour
	{

        public GameObject panel;
        public Button_Unit prefab;
        public Button[] allGroupButtons;
        public Transform parentButton;
        public Image image_PortraitScreen;
        public Image image_NoiseTV;
        public VideoPlayer portraitVideoPlayer;
        public AudioSource portraitAudioSource;
        public int maxUnitInGroup = 24;

        [HideInEditorMode] private List<Button_Unit> pooledButtons = new List<Button_Unit>();
        private SO_GameUnit currentUnitPortrait;
        private bool _isTalking = false;

        private void Awake()
        {
        }

        private void Start()
        {
            prefab.gameObject.Populate(24);

        }

        private void Update()
        {
            if (_isTalking)
            {
                if (portraitAudioSource.isPlaying == false)
                {
                    if (currentUnitPortrait != null)
                    {
                        if (currentUnitPortrait.port_Idles.Length > 0)
                        {
                            portraitVideoPlayer.isLooping = false;

                            if (portraitVideoPlayer.isPlaying == false)
                            {
                                PlayPortrait(currentUnitPortrait.port_Idles[0]);
                                _isTalking = false;
                            }
                        }
                    }
                    else
                    {
                        _isTalking = false;
                    }
                }
            }
            else
            {
                portraitVideoPlayer.isLooping = true;
            }
        }


        public void RefreshUI()
        {
            foreach (var button in pooledButtons)
            {


            }

            pooledButtons.ReleasePoolObject();

            int selectedIndex = -1;
            int index = 0;

            int count = RTS.Selection.AllSelectedUnits.Count; if (count > maxUnitInGroup) count = maxUnitInGroup;
            int totalButtonCount = 1 + Mathf.FloorToInt(RTS.Selection.AllSelectedUnits.Count / maxUnitInGroup);

            if (totalButtonCount > allGroupButtons.Length)
            {
                totalButtonCount = allGroupButtons.Length;
            }

            for (int x = 0; x < count; x++)
            {
                var unit = RTS.Selection.AllSelectedUnits[x];
                var button = prefab.gameObject.Reuse<Button_Unit>(); //Instantiate(prefab, parentButton);
                if (button.transform.parent != parentButton) button.transform.SetParent(parentButton);
                button.gameObject.SetActive(true);
                button.icon_Unit.sprite = unit.Class.spriteWireframe;
                button.transform.localPosition = Vector3.zero;
                button.transform.localScale = Vector3.one;
                index++;
                pooledButtons.Add(button);
            }

            foreach(var button in allGroupButtons)
            {
                button.gameObject.SetActive(false);
            }

            if (totalButtonCount > 1)
            {
                for (int z = 0; z < totalButtonCount; z++)
                {
                    allGroupButtons[z].gameObject.SetActive(true);
                }
            }

        }

        public void PlayPortrait(VideoClip clip)
        {
            portraitVideoPlayer.clip = clip;
            portraitVideoPlayer.Play();
            image_PortraitScreen.gameObject.SetActive(true);
        }

        public void SwitchedIdlePortrait(GameUnit gameUnit)
        {
            VideoClip selectedPortrait = null;

            _isTalking = false;

            if (gameUnit == null)
            {
                portraitVideoPlayer.Stop();
                portraitVideoPlayer.clip = null;
                image_PortraitScreen.gameObject.SetActive(false);
            }
            else if (gameUnit.Class.port_Idles.Length > 0)
            {
                selectedPortrait = gameUnit.Class.port_Idles[0];

                if (portraitVideoPlayer.clip != selectedPortrait)
                {
                    image_NoiseTV.gameObject.SetActive(false);
                    image_NoiseTV.gameObject.SetActive(true);
                }

                PlayPortrait(selectedPortrait);
                currentUnitPortrait = gameUnit.Class;
            }
            else
            {
                portraitVideoPlayer.Stop();
                image_PortraitScreen.gameObject.SetActive(false);
            }
        }


        public void PlayTalkingPortrait(GameUnit gameUnit, bool isIdleQuote, bool noChecks = false)
        {
            VideoClip selectedPortrait = null;


            if (gameUnit == null)
            {


            }
            else if (gameUnit.Class.port_Talkings.Length > 0 && (portraitAudioSource.isPlaying == false | noChecks == true)) //forbids playing audio if not finished
            {
                selectedPortrait = gameUnit.Class.port_Talkings[0];

                PlayPortrait(selectedPortrait);
                portraitVideoPlayer.isLooping = true;
                _isTalking = true;

                PlayVoice(gameUnit, isIdleQuote);
            }          
            else if ((portraitAudioSource.isPlaying == false | noChecks == true))
            {
                PlayVoice(gameUnit, isIdleQuote);
            }
        }

        public void PlayVoice(GameUnit gameUnit, bool isIdle = true)
        {
            AudioClip voice = null;

            if (isIdle)
            {
                if (gameUnit.Class.voiceline_Ready.Length > 0)
                {
                    voice = gameUnit.Class.voiceline_Ready[Random.Range(0, gameUnit.Class.voiceline_Ready.Length)];
                }
            }
            else
            {
                if (gameUnit.Class.voiceline_Move.Length > 0)
                {
                    voice = gameUnit.Class.voiceline_Move[Random.Range(0, gameUnit.Class.voiceline_Move.Length)];
                }
            }

            if (voice != null)
            {
                portraitAudioSource.clip = voice;
                portraitAudioSource.Play();
            }
        }
    }
}