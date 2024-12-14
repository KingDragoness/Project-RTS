using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using Sirenix.OdinInspector;
using ToolBox.Pools;

namespace ProtoRTS.Game
{
	public class UI_UnitSelector : MonoBehaviour
	{

        public GameObject panel;
        public Button_Unit prefab;
        public Button[] allGroupButtons;
        public Transform parentButton;
        public GridLayoutGroup gridLayoutGroup;
        [FoldoutGroup("Portrait")] public Image image_PortraitScreen;
        [FoldoutGroup("Portrait")] public Image image_NoiseTV;
        [FoldoutGroup("Portrait")] public VideoPlayer portraitVideoPlayer;
        [FoldoutGroup("Portrait")] public AudioSource portraitAudioSource;
        public int maxUnitInGroup = 24;

        [HideInEditorMode] private List<Button_Unit> pooledButtons = new List<Button_Unit>();
        private SO_GameUnit currentUnitPortrait;
        private bool _isTalking = false;
        private bool _disallowTotalRefresh = false;
        private int _groupIndex = 0;

        private void Awake()
        {
            SyntiosEvents.UI_NewSelection += event_UI_NewSelection;
            SyntiosEvents.UI_DeselectAll += event_UI_DeselectAll;
            SyntiosEvents.UI_OrderMove += event_UI_OrderMove;
        }

        private void Start()
        {
            prefab.gameObject.Populate(24);
            _disallowTotalRefresh = false;
        }

        #region Wrapper events

        private void event_UI_NewSelection(GameUnit gameunit)
        {
            SwitchedIdlePortrait(gameunit);
            PlayTalkingPortrait(gameunit, true, noChecks: true);
            UI.CommandPanel.RefreshUI();

        }

        private void event_UI_DeselectAll()
        {
            UI.CommandPanel.RefreshUI();
            SwitchedIdlePortrait(null);
            _disallowTotalRefresh = false;
        }

        private void event_UI_OrderMove(GameUnit gameunit)
        {
            PlayTalkingPortrait(gameunit, false);
        }
        #endregion

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
            //if (_disallowTotalRefresh)
            //{
            //    return;
            //}
            //else
            //{

            //}

            gridLayoutGroup.enabled = true;

            foreach (var button in pooledButtons)
            {


            }


            int index = 0;
            int totalUnitCount = Selection.AllSelectedUnits.Count;
            int totalButtonCount = 1 + Mathf.FloorToInt(Selection.AllSelectedUnits.Count / maxUnitInGroup);


            if (totalButtonCount > allGroupButtons.Length)
            {
                totalButtonCount = allGroupButtons.Length;
            }

            if (_groupIndex >= totalButtonCount)
            {
                _groupIndex = 0;
            }
            else
            {

            }

            int currentCeilingIndex = (_groupIndex + 1) * maxUnitInGroup;
            if (currentCeilingIndex > totalUnitCount) currentCeilingIndex = totalUnitCount;

            int currentIndex = (_groupIndex * maxUnitInGroup); 
            if (currentIndex > currentCeilingIndex) currentIndex = currentCeilingIndex;

            pooledButtons.ReleasePoolObject();

            for (int x = currentIndex; x < currentCeilingIndex; x++)
            {
                var unit = Selection.AllSelectedUnits[x];
                var button = prefab.gameObject.Reuse<Button_Unit>(); //Instantiate(prefab, parentButton);
                if (button.transform.parent != parentButton) button.transform.SetParent(parentButton);
                button.gameObject.SetActive(true);
                button.button.onClick.RemoveAllListeners();
                button.icon_Unit.sprite = unit.Class.spriteWireframe;
                button.transform.localPosition = Vector3.zero;
                button.transform.localScale = Vector3.one;
                button.attachedGameUnit = unit;

                {
                    button.button.onClick.AddListener(Check);
                }

                index++;
                pooledButtons.Add(button);
            }

            //GROUP BUTTONS
            {
                foreach (var button in allGroupButtons)
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

            _disallowTotalRefresh = true;
            StartCoroutine(DisableGLG(0.02f));
        }

        public void ChangeGroup(int index)
        {
            _groupIndex = index;
            RefreshUI();
        }

        private IEnumerator DisableGLG(float delayTime)
        {
            yield return new WaitForSeconds(delayTime);
            gridLayoutGroup.enabled = false;

        }

        public void Check()
        {

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