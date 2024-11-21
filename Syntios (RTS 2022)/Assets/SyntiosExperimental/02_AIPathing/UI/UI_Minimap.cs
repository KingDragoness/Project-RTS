using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

namespace ProtoRTS
{
	public class UI_Minimap : MonoBehaviour
	{

        public float defaultMinimapLength = 255f;
        public Vector2 offset = new Vector2(15, 15);
        public RectTransform ui_Map;
        public RectTransform ui_OriginMinimap;


        public LineRenderer line_Viewport;
        public Image testImg_LD;
        public Image testImg_RD;
        public Image testImg_LU;
        public Image testImg_RU;

        private bool allowDrag = false;

        private void Start()
        {
            RefreshMapData();
        }

        private void RefreshMapData()
        {
            if (RTS.Controller.MapSize.x > RTS.Controller.MapSize.y)
            {
                float rescale = RTSController.MapSize1.y / RTSController.MapSize1.x;
                Vector2 v2 = new Vector2(defaultMinimapLength, defaultMinimapLength);
                v2.y *= rescale;

                ui_Map.sizeDelta = v2;
            }
            else
            if (RTS.Controller.MapSize.y > RTS.Controller.MapSize.x)
            {
                float rescale = RTSController.MapSize1.x / RTSController.MapSize1.y;
                Vector2 v2 = new Vector2(defaultMinimapLength, defaultMinimapLength);
                v2.x *= rescale;

                ui_Map.sizeDelta = v2;
            }
            else
            {
                Vector2 v2 = new Vector2(defaultMinimapLength, defaultMinimapLength);

                ui_Map.sizeDelta = v2;
            }
        }

        private void OnDrawGizmosSelected()
        {
            var rightUp_ray = Camera.main.ViewportPointToRay(new Vector3(1f, 1f, 0));
            var leftUp_ray = Camera.main.ViewportPointToRay(new Vector3(0f, 1f, 0));
            var rightDown_ray = Camera.main.ViewportPointToRay(new Vector3(1f, 0f, 0));
            var leftDown_ray = Camera.main.ViewportPointToRay(new Vector3(0f, 0f, 0));

            {
                RaycastHit hit;
                if (Physics.Raycast(rightUp_ray, out hit))
                {
                    Debug.DrawLine(rightUp_ray.origin, hit.point, Color.green);
                }
            }
            {
                RaycastHit hit;
                if (Physics.Raycast(leftUp_ray, out hit))
                {
                    Debug.DrawLine(leftUp_ray.origin, hit.point, Color.green);
                }
            }
            {
                RaycastHit hit;
                if (Physics.Raycast(rightDown_ray, out hit))
                {
                    Debug.DrawLine(rightDown_ray.origin, hit.point, Color.green);
                }
            }
            {
                RaycastHit hit;
                if (Physics.Raycast(leftDown_ray, out hit))
                {
                    Debug.DrawLine(leftDown_ray.origin, hit.point, Color.green);
                }
            }
        }

        private void Update()
        {
            Update_Viewport();

            if (Input.GetMouseButton(0) == false)
            {
                allowDrag = false;
            }

            if (allowDrag) Update_MoveCam();
        }

        private void Update_Viewport()
        {
            var rightUp_ray = RTS.MainCamera.ViewportPointToRay(new Vector3(1f, 1f, 0));
            var leftUp_ray = RTS.MainCamera.ViewportPointToRay(new Vector3(0f, 1f, 0));
            var rightDown_ray = RTS.MainCamera.ViewportPointToRay(new Vector3(1f, 0f, 0));
            var leftDown_ray = RTS.MainCamera.ViewportPointToRay(new Vector3(0f, 0f, 0));

            float scaleFactor_x = 1f;
            float scaleFactor_y = 1f;

            scaleFactor_x = 1f / (RTSController.MapSize1.x / ui_Map.sizeDelta.x * 2f); //2 unity unit = 1 Syntios map unit
            scaleFactor_y = 1f / (RTSController.MapSize1.y / ui_Map.sizeDelta.y * 2f); //2 unity unit = 1 Syntios map unit

            Vector3[] pos_line = new Vector3[4];

            {
                RaycastHit hit;
                if (Physics.Raycast(rightUp_ray, out hit))
                {
                    Vector3 pos1 = hit.point;
                    pos1.x = hit.point.x * scaleFactor_x;
                    pos1.y = hit.point.z * scaleFactor_y;
                    pos1.z = 0f;
                    testImg_RU.rectTransform.anchoredPosition = pos1;
                    pos_line[2] = pos1;

                }
            }
            {
                RaycastHit hit;
                if (Physics.Raycast(leftUp_ray, out hit))
                {
                    Vector3 pos1 = hit.point;
                    pos1.x = hit.point.x * scaleFactor_x;
                    pos1.y = hit.point.z * scaleFactor_y;
                    pos1.z = 0f;
                    testImg_LU.rectTransform.anchoredPosition = pos1;
                    pos_line[1] = pos1;

                }
            }
            {
                RaycastHit hit;
                if (Physics.Raycast(rightDown_ray, out hit))
                {
                    Vector3 pos1 = hit.point;
                    pos1.x = hit.point.x * scaleFactor_x;
                    pos1.y = hit.point.z * scaleFactor_y;
                    pos1.z = 0f;
                    testImg_RD.rectTransform.anchoredPosition = pos1;
                    pos_line[3] = pos1;

                }
            }
            {
                RaycastHit hit;
                if (Physics.Raycast(leftDown_ray, out hit))
                {
                    Vector3 pos1 = hit.point;
                    pos1.x = hit.point.x * scaleFactor_x;
                    pos1.y = hit.point.z * scaleFactor_y;
                    pos1.z = 0f;
                    testImg_LD.rectTransform.anchoredPosition = pos1;
                    pos_line[0] = pos1;

                }
            }

            for (int i = 0; i < pos_line.Length; i++)
            {
                if (0 > pos_line[i].x)
                {
                    pos_line[i] = new Vector3(0, pos_line[i].y);
                }
                if (0 > pos_line[i].y)
                {
                    pos_line[i] = new Vector3(pos_line[i].x, 0);
                }
                if (ui_Map.sizeDelta.x < pos_line[i].x)
                {
                    pos_line[i] = new Vector3(ui_Map.sizeDelta.x, pos_line[i].y);
                }
                if (ui_Map.sizeDelta.y < pos_line[i].y)
                {
                    pos_line[i] = new Vector3(pos_line[i].x, ui_Map.sizeDelta.y);
                }
            }

            line_Viewport.SetPositions(pos_line);
        }

        private string debug_sPos = "";

        private void Update_MoveCam()
        {
            var parent = ui_OriginMinimap.transform.parent;
            ui_OriginMinimap.SetParent(UI.CanvasMainUI.transform);
            Vector2 originMap = ui_OriginMinimap.anchoredPosition;     
            Vector2 mousePosition = Input.mousePosition;


            var mouseposOnMinimap = GetMousePositionInMinimap(mousePosition - originMap);

            var newPosition = ConvertMinimapPosToCameraPos(mouseposOnMinimap);

            debug_sPos = originMap + " | " + mouseposOnMinimap + " | result: " + newPosition;
            RTS.RTSCamera.transform.position = newPosition;
            ui_OriginMinimap.SetParent(parent);

        }


        public void InitiateDrag()
        {
            allowDrag = true;
        }

        public Vector2 GetMousePositionInMinimap(Vector2 pos1)
        {
            if (pos1.x > ui_Map.sizeDelta.x)
            {
                return new Vector2(ui_Map.sizeDelta.x, pos1.y);
            }
            if (pos1.y > ui_Map.sizeDelta.y)
            {
                return new Vector2(pos1.x, ui_Map.sizeDelta.y);
            }

            return pos1;

        }

        public Vector3 ConvertMinimapPosToCameraPos(Vector2 minimapPos)
        {
            float mapLongestPixelAxis = RTS.Controller.MapSize.x;

            if (RTS.Controller.MapSize.y > RTS.Controller.MapSize.x) mapLongestPixelAxis = RTS.Controller.MapSize.y;
            float lerp_x = minimapPos.x / ui_Map.sizeDelta.x;
            float lerp_y = minimapPos.y / ui_Map.sizeDelta.y;

            Vector3 result = new Vector3();
            result.x = Mathf.Lerp(0, RTS.Controller.MapSize.x, lerp_x) * 2f;
            result.y = 0f;
            result.z = Mathf.Lerp(0, RTS.Controller.MapSize.y, lerp_y) * 2f;

            result.x += offset.x;
            result.z += offset.y;

            return result;
        }
    }
}