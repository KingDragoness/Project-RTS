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
        public float MinimapRefreshRate = 2f;


        public LineRenderer line_Viewport;
        public RawImage mapTexture_Unit;
        public Image testImg_LD;
        public Image testImg_RD;
        public Image testImg_LU;
        public Image testImg_RU;

        private bool allowDrag = false;
        private Canvas myCanvas;
        private Texture minimap_Terrain;
        private Texture2D texture_minimap_Units;

        private float cooldown_MinimapRefresh = 2f;


        private void Start()
        {
            myCanvas = GetComponentInParent<Canvas>();
            RefreshMapData();
            texture_minimap_Units = new Texture2D(ui_Map.sizeDelta.x.ToInt(), ui_Map.sizeDelta.y.ToInt());

        }

        private void RefreshMapData()
        {
            if (Map.MapSize.x > Map.MapSize.y)
            {
                float rescale = Map.MapSize.y / Map.MapSize.x;
                Vector2 v2 = new Vector2(defaultMinimapLength, defaultMinimapLength);
                v2.y *= rescale;

                ui_Map.sizeDelta = v2;
            }
            else
            if (Map.MapSize.y > Map.MapSize.x)
            {
                float rescale = Map.MapSize.x / Map.MapSize.y;
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

            if (cooldown_MinimapRefresh > 0f)
            {
                cooldown_MinimapRefresh -= Time.deltaTime;
            }
            else
            {
                Update_Minimap();
                cooldown_MinimapRefresh = MinimapRefreshRate;
            }
        }

        private void Update_Viewport()
        {
            var rightUp_ray = Camera.main.ViewportPointToRay(new Vector3(1f, 1f, 0));
            var leftUp_ray = Camera.main.ViewportPointToRay(new Vector3(0f, 1f, 0));
            var rightDown_ray = Camera.main.ViewportPointToRay(new Vector3(1f, 0f, 0));
            var leftDown_ray = Camera.main.ViewportPointToRay(new Vector3(0f, 0f, 0));

            float scaleFactor_x = 1f;
            float scaleFactor_y = 1f;

            scaleFactor_x = 1f / (Map.MapSize.x / ui_Map.sizeDelta.x * 2f); //2 unity unit = 1 Syntios map unit
            scaleFactor_y = 1f / (Map.MapSize.y / ui_Map.sizeDelta.y * 2f); //2 unity unit = 1 Syntios map unit

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

        public Vector2 GetMousePosInMinimap()
        {
            var parent = ui_OriginMinimap.transform.parent;
            ui_OriginMinimap.SetParent(myCanvas.transform);
            Vector2 originMap = ui_OriginMinimap.anchoredPosition;
            Vector2 mousePosition = Input.mousePosition;

            var result = _getMousePositionInMinimap(mousePosition - originMap);
            ui_OriginMinimap.SetParent(parent);

            return result;
        }

        private void Update_MoveCam()
        {
            var parent = ui_OriginMinimap.transform.parent;
            ui_OriginMinimap.SetParent(myCanvas.transform);
            Vector2 originMap = ui_OriginMinimap.anchoredPosition;     
            Vector2 mousePosition = Input.mousePosition;


            var mouseposOnMinimap = _getMousePositionInMinimap(mousePosition - originMap);

            var newPosition = ConvertMinimapPosToCameraPos(mouseposOnMinimap);

            debug_sPos = originMap + " | " + mouseposOnMinimap + " | result: " + newPosition;
            RTSCamera.SetPositionByMinimap(newPosition);
            ui_OriginMinimap.SetParent(parent);

        }

        private void Update_Minimap()
        {
            //if (minimap_Units != null) Destroy(minimap_Units);

            //minimap_Units = new Texture2D(ui_Map.sizeDelta.x.ToInt(), ui_Map.sizeDelta.y.ToInt());
            texture_minimap_Units.filterMode = FilterMode.Point;
            var fillColorArray = texture_minimap_Units.GetPixels();

            Color transparentColor = new Color(0f, 0f, 0f, 0f);

            for (var i = 0; i < fillColorArray.Length; ++i)
            {
                fillColorArray[i] = transparentColor;
            }
            texture_minimap_Units.SetPixels(fillColorArray);

            foreach (var gameUnit in SyntiosEngine.Instance.ListedGameUnits)
            {
                Vector2Int posCenter = ConvertWorldPosToMinimapPos(gameUnit.transform.position);
                Color c = Unit.GetColor(gameUnit.stat_faction);
                int radius = RadiusUnitInMiniMap(gameUnit.Class.Radius);
                if (radius <= 2) radius = 2;
                int r2 = radius * radius;

                for(int x = 0; x < r2; x++)
                {
                    int pixelX = Mathf.FloorToInt(x % radius);
                    int pixelY = Mathf.FloorToInt(x / radius);
                    int mid = radius / 2;

                    Vector2Int currPixel = posCenter;
                    currPixel.x -= mid;
                    currPixel.y -= mid;
                    currPixel.x += pixelX;
                    currPixel.y += pixelY;

                    texture_minimap_Units.SetPixel(currPixel.x, currPixel.y, c);

                }
            }

            texture_minimap_Units.Apply();
            mapTexture_Unit.texture = texture_minimap_Units;
        }


        public void InitiateDrag()
        {
            allowDrag = true;
        }

        public Vector2 _getMousePositionInMinimap(Vector2 pos1)
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
            float mapLongestPixelAxis = Map.MapSize.x;

            if (Map.MapSize.y > Map.MapSize.x) mapLongestPixelAxis = Map.MapSize.y;
            float lerp_x = minimapPos.x / ui_Map.sizeDelta.x;
            float lerp_y = minimapPos.y / ui_Map.sizeDelta.y;

            Vector3 result = new Vector3();
            result.x = Mathf.Lerp(0, Map.MapSize.x, lerp_x) * 2f;
            result.y = 0f;
            result.z = Mathf.Lerp(0, Map.MapSize.y, lerp_y) * 2f;

            result.x += offset.x;
            result.z += offset.y;
            result = ClampPosition(result);

            return result;
        }

        public Vector3 ConvertMinimapPosToWorldPos(Vector2 minimapPos)
        {
            float mapLongestPixelAxis = Map.MapSize.x;

            if (Map.MapSize.y > Map.MapSize.x) mapLongestPixelAxis = Map.MapSize.y;
            float lerp_x = minimapPos.x / ui_Map.sizeDelta.x;
            float lerp_y = minimapPos.y / ui_Map.sizeDelta.y;

            Vector3 result = new Vector3();
            result.x = Mathf.Lerp(0, Map.MapSize.x, lerp_x) * 2f;
            result.y = 0f;
            result.z = Mathf.Lerp(0, Map.MapSize.y, lerp_y) * 2f;


            return result;
        }


        public Vector2Int ConvertWorldPosToMinimapPos(Vector3 worldPosition)
        {
            Vector2Int result = new Vector2Int();
            result.x = Mathf.Lerp(0, ui_Map.sizeDelta.x, (worldPosition.x / Map.MapSize.x /2f)).ToInt();
            result.y = Mathf.Lerp(0, ui_Map.sizeDelta.y, (worldPosition.z / Map.MapSize.y /2f)).ToInt();

            return result;
        }

        public int RadiusUnitInMiniMap(float realRadius)
        {
            return (( (realRadius + 1f) / (Map.MapSize.x + Map.MapSize.y)) * defaultMinimapLength).ToInt();
        }    

        public Vector3 ClampPosition (Vector3 pos)
        {
            if (pos.x > Map.MapSize.x * 2f)
            {
                pos.x = Map.MapSize.x * 2f;
            }

            if (pos.x < 0)
            {
                pos.x = 0;
            }

            if (pos.z > Map.MapSize.y * 2f)
            {
                pos.z = Map.MapSize.y * 2f;
            }

            if (pos.z < -10)
            {
                pos.z = -10;
            }

            return pos;
        }
    }
}