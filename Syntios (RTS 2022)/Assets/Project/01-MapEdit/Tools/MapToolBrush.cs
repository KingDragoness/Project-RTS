using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ProtoRTS.MapEditor
{
	public class MapToolBrush : MonoBehaviour
	{

        public enum Shape
        {
            Square,
            Circle,
            Diagonal
        }

        public float targetBrushSize = 2;
        public Shape targetShape;
        public bool isCliff = false;

        [Header("References")]
        public Material mat_projectorSquare;
        public Material mat_projectorCircle;
        public Transform brushProjector;
        public Transform DEBUG_originBrush;
        public Projector projector;

        private Vector3 brushPosition = new Vector3();

        public Vector3 BrushPosition { get => brushPosition; set => brushPosition = value; }


        private void OnGUI()
        {
            Vector3 posOrigin = BrushPosition;
            Vector2Int pixelPos = MapToolScript.WorldPosToCliffmapPos(BrushPosition);

            if (Application.isEditor)
            {
                GUI.Label(new Rect(Input.mousePosition.x + 25, Screen.height - Input.mousePosition.y, 100, 20), $"{pixelPos}");

                int index = 0;

                foreach(var test in Map.TerrainData.cliffLevel)
                {
                    int x = index % Map.TerrainData.size_x;
                    int y = index / Map.TerrainData.size_y;
                    index++;

                    Vector3 v3 = new Vector3(x * 2, 0, y * 2);

                    Vector3 uiPos = Camera.main.WorldToScreenPoint(v3);
                    uiPos.y = Screen.height - uiPos.y;

                    if (x > 16) continue;
                    if (y > 16) continue;
                    GUI.Label(new Rect(uiPos.x, uiPos.y, 100, 20), $"({x}, {y}) {test}");
                }
            }
        }

        public void EnableBrush()
        {
            if (gameObject.activeSelf == false) gameObject.SetActive(true);
        }

        public void DisableBrush()
        {
            if (gameObject.activeSelf == true) gameObject.SetActive(false);
        }

        private void Update()
        {
            projector.orthographicSize = targetBrushSize / 2f;
            Update_BrushProjector();
        }


        private void Update_BrushProjector()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 1000))
            {
                Vector3 pos = hit.point;
                pos.y = 50f;

                if (isCliff)
                {
                    pos.x = Mathf.Round(pos.x / 2f) * 2f;
                    pos.z = Mathf.Round(pos.z / 2f) * 2f;
                }

                brushPosition = hit.point;
                brushProjector.transform.position = pos;
            }

            if (targetShape == Shape.Circle)
            {
                projector.material = mat_projectorCircle;
            }
            else if (targetShape == Shape.Square)
            {
                projector.material = mat_projectorSquare;
            }
        }

    }
}