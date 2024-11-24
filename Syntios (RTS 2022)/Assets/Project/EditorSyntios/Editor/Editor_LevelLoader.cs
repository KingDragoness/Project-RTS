using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using Sirenix.OdinInspector;
using Object = System.Object;
using UnityEditor.SceneManagement;

namespace ProtoRTS.Editor
{
    //https://www.youtube.com/watch?v=00sqxQFm-7A&list=PLg_ZciC4y1WilboQL_UgDv4IR-eoQ-KTs&index=3

    public class Editor_LevelLoader : EditorWindow
	{

        private Button loadgameButton;
        private Button loadMapEditorButton;

		[MenuItem("Syntios/Scene Loader")]
		public static void OpenEditorWindow()
        {
			Editor_LevelLoader window = GetWindow<Editor_LevelLoader>();
			window.maxSize = new Vector2(x: 300, y: 300);
        }

        private void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Project/EditorSyntios/LevelLoader.uxml");

            VisualElement tree = visualTree.Instantiate();
            root.Add(tree);

            loadgameButton = root.Q<Button>("button_Game");
            loadMapEditorButton = root.Q<Button>("button_MapEditor");

            loadgameButton.clicked += LoadgameButton_clicked;
            loadMapEditorButton.clicked += LoadMapEditorButton_clicked;
        }

        private void LoadgameButton_clicked()
        {
            if (Application.isPlaying) throw new Exception("Game is running. Forbidden opening scene.");
            //Debug.Log("Load game!");

            EditorSceneManager.OpenScene("Assets/Scenes/02-ProtoRTSSystem.unity");
            //EditorSceneManager.LoadScene("", UnityEngine.SceneManagement.LoadSceneMode.Additive);

        }


        private void LoadMapEditorButton_clicked()
        {
            if (Application.isPlaying) throw new Exception("Game is running. Forbidden opening scene.");
            //Debug.Log("Load map editor!");


            EditorSceneManager.OpenScene("Assets/Scenes/01-ProtoMapEditor.unity");
            //EditorSceneManager.LoadScene("", UnityEngine.SceneManagement.LoadSceneMode.Additive);

        }
    }
}