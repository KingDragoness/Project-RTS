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
        [FoldoutGroup("Selection")] public int minBoxSizeDrag = 12;
        [FoldoutGroup("Selection")] public float DoubleClickTime = 0.3f;


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

        public GameUnit GetSingleGameUnit()
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, 500f))
            {
                if (hit.collider.gameObject.CompareTag("Unit"))
                {
                    var parentGameunit = hit.collider.gameObject.GetComponentInParent<GameUnit>();

                    if (parentGameunit != null)
                    {
                        return parentGameunit;
                    }
                }
            }

            return null;
        }


        private bool _invalidDrag = false;
        private GameUnit singleSelectUnit;
        [ShowInInspector] [DisableInEditorMode] [SerializeField] private float _timeSinceMouse0 = 0.2f;


        private void Update()
        {

            bool doubleClickDetected = false;

            _timeSinceMouse0 += Time.deltaTime;
            {
                singleSelectUnit = GetSingleGameUnit();

                // When Clicked
                if (Input.GetMouseButtonDown(0))
                {

                    if (MainUI.GetEventSystemRaycastResults().Count > 0)
                    {
                        _invalidDrag = true;
                    }
                    else
                    {
                        Selection.DeselectAllUnits();
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

                    if (selectionBounds.size.magnitude > minBoxSizeDrag)
                    {
                        go_StartDragBox.transform.position = startPosition;
                        go_EndDragBox.transform.position = endPosition;
                        go_StartDragBox.gameObject.SetActive(true);
                        go_EndDragBox.gameObject.SetActive(true);
                    }
                    else
                    {

                    }

                }
                else
                {
                    go_StartDragBox.gameObject.SetActive(false);
                    go_EndDragBox.gameObject.SetActive(false);

                }

                if (_timeSinceMouse0 <= DoubleClickTime) doubleClickDetected = true;

                // When Releasing
                if (Input.GetMouseButtonUp(0))
                {
                    _timeSinceMouse0 = 0f;

                    if (doubleClickDetected == false)
                    {
                        if (_invalidDrag == false)
                        {
                            SelectUnits();
                        }
                        if (singleSelectUnit != null)
                        {
                            //Selection.DeselectAllUnits();
                            SelectOneUnit(singleSelectUnit);
                        }
                    }
                    else if (doubleClickDetected == true)
                    {
                        if (singleSelectUnit != null)
                        {
                            SelectAllUnitInScreenSpace(singleSelectUnit);
                        }
                    }
                    
                    _invalidDrag = false;

                    startPosition = Vector2.zero;
                    endPosition = Vector2.zero;
                    DrawVisual();
                }
            }

            doubleClickDetected = false;

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

        void SelectOneUnit(GameUnit unit)
        {
            Selection.SelectUnit(unit);
            unit.SelectedUnit(circleOutline_Green.transform);

            if (Selection.GetPortraitedUnit != null)
            {
                var unit1 = Selection.GetPortraitedUnit;
                SyntiosEvents.UI_NewSelection?.Invoke(unit1);
            }
        }

        void SelectAllUnitInScreenSpace(GameUnit singleUnit)
        {
            Debug.Log("Selecting all units in the view!");
            Selection.DeselectAllUnits();

            foreach (var unit in SyntiosEngine.Instance.ListedGameUnits)
            {
                if (unit.stat_faction != SyntiosEngine.CurrentFaction) continue;
                if (unit._class.ID != singleUnit._class.ID) continue;
                Vector3 vpPos = Camera.main.WorldToViewportPoint(unit.transform.position);

                if (vpPos.x >= 0f && vpPos.x <= 1f && vpPos.y >= 0.2f && vpPos.y <= 1f && vpPos.z > 0f)
                {
                    Selection.SelectUnit(unit);
                    unit.SelectedUnit(circleOutline_Green.transform);
                }

            }

            if (Selection.GetPortraitedUnit != null)
            {
                var unit1 = Selection.GetPortraitedUnit;

                SyntiosEvents.UI_NewSelection?.Invoke(unit1);

            }
        }

        void SelectUnits()
        {
            var everyUnit = FindObjectsOfType<GameUnit>();
            Rect rect = new Rect();
            Vector3 pos1 = startPosition;
            Vector3 pos2 = endPosition;

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

            }

            rect.min = camPos1;
            rect.max = camPos2;

            foreach (var unit in everyUnit)
            {
                if (unit.stat_faction != SyntiosEngine.CurrentFaction) continue;

                var upRad = unit.transform.position + Vector3.forward * unit.Class.Radius;
                var downRad = unit.transform.position + -Vector3.forward * unit.Class.Radius;
                var leftRad = unit.transform.position + -Vector3.right * unit.Class.Radius;
                var rightRad = unit.transform.position + Vector3.right * unit.Class.Radius;

                if (rect.Contains(myCam.WorldToScreenPoint(unit.transform.position)))
                {
                    Selection.SelectUnit(unit);
                    unit.SelectedUnit(circleOutline_Green.transform);
                }
                else if (rect.Contains(myCam.WorldToScreenPoint(upRad)))
                {
                    Selection.SelectUnit(unit);
                    unit.SelectedUnit(circleOutline_Green.transform);
                }
                else if (rect.Contains(myCam.WorldToScreenPoint(downRad)))
                {
                    Selection.SelectUnit(unit);
                    unit.SelectedUnit(circleOutline_Green.transform);
                }
                else if (rect.Contains(myCam.WorldToScreenPoint(leftRad)))
                {
                    Selection.SelectUnit(unit);
                    unit.SelectedUnit(circleOutline_Green.transform);
                }
                else if (rect.Contains(myCam.WorldToScreenPoint(rightRad)))
                {
                    Selection.SelectUnit(unit);
                    unit.SelectedUnit(circleOutline_Green.transform);
                }
            }


            if (Selection.GetPortraitedUnit != null)
            {
                var unit1 = Selection.GetPortraitedUnit;

                SyntiosEvents.UI_NewSelection?.Invoke(unit1);

            }

        }
    }
}