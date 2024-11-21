using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ProtoRTS
{
	public class BoxSelectionUnit : MonoBehaviour
	{
        [FoldoutGroup("Selection")] public LayerMask layer_Terrain;
        [FoldoutGroup("Selection")] public GameObject go_StartDragBox;
        [FoldoutGroup("Selection")] public GameObject go_EndDragBox;

        public GameObject circleOutline_Green;
        Camera myCam;

        [SerializeField]
        RectTransform boxVisual;

        Bounds selectionBounds;

        private Vector3 startPosition;
        private Vector3 endPosition;

        private void Start()
        {
            myCam = Camera.main;
            startPosition = Vector3.zero;
            endPosition = Vector3.zero;
            DrawVisual();
        }

        public Vector3 GetRaycastWorldClick()
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, 500f, layer_Terrain))
            {
                return hit.point;
            }

            return Vector3.zero;
        }


        private bool _invalidDrag = false;


        private void Update()
        {


            {

                // When Clicked
                if (Input.GetMouseButtonDown(0))
                {
                    if (MainUI.GetEventSystemRaycastResults().Count > 0)
                    {
                        _invalidDrag = true;
                    }
                    else
                    {
                        RTSController.Instance.DeselectAllUnit();
                        startPosition = GetRaycastWorldClick();

                        // For selection the Units
                        selectionBounds = new Bounds();
                        _invalidDrag = false;
                    }
                }


                // When Dragging
                if (Input.GetMouseButton(0) && _invalidDrag == false)
                {
                    endPosition = GetRaycastWorldClick();
                    DrawVisual();
                    DrawSelection();
                    go_StartDragBox.transform.position = startPosition;
                    go_EndDragBox.transform.position = endPosition;
                    go_StartDragBox.gameObject.SetActive(true);
                    go_EndDragBox.gameObject.SetActive(true);
                }
                else
                {
                    go_StartDragBox.gameObject.SetActive(false);
                    go_EndDragBox.gameObject.SetActive(false);

                }

                // When Releasing
                if (Input.GetMouseButtonUp(0))
                {
                    SelectUnits();
                    _invalidDrag = false;

                    startPosition = Vector2.zero;
                    endPosition = Vector2.zero;
                    DrawVisual();
                }
            }

        }

        void DrawVisual()
        {
            // Calculate the starting and ending positions of the selection box.
            Vector2 boxStart = myCam.WorldToScreenPoint(startPosition);
            Vector2 boxEnd = myCam.WorldToScreenPoint(endPosition);

            // Calculate the center of the selection box.
            Vector2 boxCenter = (boxStart + boxEnd) / 2;

            // Set the position of the visual selection box based on its center.
            boxVisual.position = boxCenter;

            // Calculate the size of the selection box in both width and height.
            Vector2 boxSize = new Vector2(Mathf.Abs(boxStart.x - boxEnd.x), Mathf.Abs(boxStart.y - boxEnd.y));

            // Set the size of the visual selection box based on its calculated size.
            boxVisual.sizeDelta = boxSize;
        }

        void DrawSelection()
        {
            Vector3 pos1 = startPosition; pos1.y = -1000f;
            Vector3 pos2 = endPosition; pos2.y = 1000f;

            if (startPosition.x > endPosition.x)
            {
                pos1.x = endPosition.x;
                pos2.x = startPosition.x;
            }

            if (startPosition.z > endPosition.z)
            {
                pos1.z = endPosition.z;
                pos2.z = startPosition.z;
            }


            selectionBounds.min = pos1;
            selectionBounds.max = pos2;

        }


        void SelectUnits()
        {
            var everyUnit = FindObjectsOfType<GameUnit>();
            Rect rect = new Rect();
            Vector3 pos1 = startPosition;
            Vector3 pos2 = endPosition;

            {
                //if (startPosition.x > endPosition.x)
                //{
                //    pos1.x = endPosition.x;
                //    pos2.x = startPosition.x;
                //}
                //if (startPosition.z > endPosition.z)
                //{
                //    pos1.z = endPosition.z;
                //    pos2.z = startPosition.z;
                //}

                //Debug.Log($"{pos1}, {pos2}");
            }

            //rect.min = myCam.WorldToScreenPoint(pos1);
            //rect.max = myCam.WorldToScreenPoint(pos2);

            Vector3 camPos1 = myCam.WorldToScreenPoint(pos1);
            Vector3 camPos2 = myCam.WorldToScreenPoint(pos2);

            {
                //RECT!!!
                if (camPos1.x > camPos2.x)
                {
                    float min = camPos2.x;
                    float max = camPos1.x;

                    camPos1.x = min;
                    camPos2.x = max;
                }
                if (camPos1.y > camPos2.y)
                {
                    float min = camPos2.y;
                    float max = camPos1.y;

                    camPos1.y = min;
                    camPos2.y = max;
                }

                //Debug.Log($"{camPos1}, {camPos2}");
            }

            rect.min = camPos1;
            rect.max = camPos2;

            foreach (var unit in everyUnit)
            {
                if (rect.Contains(myCam.WorldToScreenPoint(unit.transform.position)))
                {
                    RTSController.Instance.DragSelect(unit);
                    unit.SelectedUnit(circleOutline_Green.transform);
                }
            }

            UI.CommandPanel.RefreshUI();

        }
    }
}