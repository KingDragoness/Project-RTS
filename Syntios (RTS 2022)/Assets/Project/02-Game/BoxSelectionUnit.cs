using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;

namespace ProtoRTS
{
	public class BoxSelectionUnit : MonoBehaviour
	{
        [FoldoutGroup("Selection")] public LayerMask layer_Terrain;
        [FoldoutGroup("Selection")] public GameObject go_StartDragBox;
        [FoldoutGroup("Selection")] public GameObject go_EndDragBox;
        [FoldoutGroup("Selection")] public int minBoxSizeDrag = 12;
        [FoldoutGroup("Selection")] public float DoubleClickTime = 0.3f;
        public float disableBoxSelectTime = 0f;


        //public GameObject circleOutline_Green;
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
        private GameUnit gameunitCached_singleUnit;


        private void Update()
        {

            bool doubleClickDetected = false;
            bool addUnit = false;

            _timeSinceMouse0 += Time.deltaTime;
            if (disableBoxSelectTime > 0) disableBoxSelectTime -= Time.deltaTime;

            if (disableBoxSelectTime <= 0f)
            {
                var tempHoveredUnit = GetSingleGameUnit();
                if (singleSelectUnit != null && singleSelectUnit != tempHoveredUnit) { singleSelectUnit.DehighlightUnit(); }
                singleSelectUnit = tempHoveredUnit;

                // When Clicked
                if (Input.GetMouseButtonDown(0))
                {

                    if (MainUI.GetEventSystemRaycastResults().Count > 0)
                    {
                        _invalidDrag = true;
                    }
                    else
                    {
                        //Selection.DeselectAllUnits();
                        startPosition = GetRaycastWorldClick();

                        // For selection the Units
                        selectionBounds = new Bounds();
                        _invalidDrag = false;
                    }
                }

                if (Input.GetKey(KeyCode.LeftShift))
                {
                    addUnit = true;
                }

                if (Input.GetKeyUp(KeyCode.Escape))
                {
                    Selection.DeselectAllUnits();
                    startPosition = GetRaycastWorldClick();

                    selectionBounds = new Bounds();
                    _invalidDrag = false;
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
                        _invalidDrag = true;

                    }

                }
                else
                {
                    go_StartDragBox.gameObject.SetActive(false);
                    go_EndDragBox.gameObject.SetActive(false);

                }

                if (_timeSinceMouse0 <= DoubleClickTime) doubleClickDetected = true;

                // When Releasing
                if (Input.GetMouseButtonUp(0) && disableBoxSelectTime <= 0f)
                {
                    _timeSinceMouse0 = 0f;

                    if (doubleClickDetected == false)
                    {
                        bool isSelectAnyUnit = CheckAnyUnitInBox();

                        //start box selection if box select
                        if (isSelectAnyUnit == true)
                        {
                            if (addUnit == false) Selection.DeselectAllUnits();
                            SelectUnits();
                        }
                        //
                        else if (singleSelectUnit != null && MainUI.GetEventSystemRaycastResults().Count <= 0)
                        {
                            SelectOneUnit(singleSelectUnit);
                        }
                    }
                    else if (doubleClickDetected == true)
                    {
                        if (singleSelectUnit != null && MainUI.GetEventSystemRaycastResults().Count <= 0)
                        {
                            Selection.DeselectAllUnits();
                            SelectAllUnitInScreenSpace(singleSelectUnit);
                        }
                    }
                    
                    startPosition = Vector2.zero;
                    endPosition = Vector2.zero;
                    DrawVisual();
                }

                if (singleSelectUnit != null && MainUI.GetEventSystemRaycastResults().Count <= 0)
                {
                    singleSelectUnit.HighlightUnit();
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
            //if (Selection.AllSelectedUnits.Count > 0 && unit.IsPlayerUnit() == false) return;
            if (!FOWScript.IsCoordRevealed(unit.transform.position, SyntiosEngine.CurrentFaction) && unit.IsPlayerUnit() == false) return;

            Selection.DeselectAllExcept(unit);
            Selection.SelectUnit(unit);
            unit.SelectedUnit();


            if (Selection.GetPortraitedUnit != null)
            {
                var unit1 = Selection.GetPortraitedUnit;
                SyntiosEvents.UI_NewSelection?.Invoke(unit1);
            }

            gameunitCached_singleUnit = unit;
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
                    unit.SelectedUnit();
                }

            }

            if (Selection.GetPortraitedUnit != null)
            {
                var unit1 = Selection.GetPortraitedUnit;

                SyntiosEvents.UI_NewSelection?.Invoke(unit1);

            }
        }

        private bool CheckAnyUnitInBox()
        {
            var everyUnit = SyntiosEngine.Instance.ListedGameUnits;
            Rect rect = new Rect();
            Vector3 pos1 = startPosition;
            Vector3 pos2 = endPosition;
            Vector3 centerPos = (startPosition + endPosition) / 2f;

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
            bool allowBoxSelectOtherFaction = false;

            if (rect.size.x < 999 && rect.size.y < 999)
            {
                allowBoxSelectOtherFaction = true;
            }

            var currentFaction = SyntiosEngine.CurrentFaction;

            List<GameUnit> playerUnits = new List<GameUnit>();

            playerUnits = everyUnit.ToList().FindAll(s => s.stat_faction == currentFaction);

            //1st PASS: Player but only game unit
            foreach (var unit in playerUnits)
            {
                if (unit.Class.AllUnitTags.Contains(Unit.Tag.Structure)) continue;

                var upRad = unit.transform.position + Vector3.forward * unit.Class.Radius;
                var downRad = unit.transform.position + -Vector3.forward * unit.Class.Radius;
                var leftRad = unit.transform.position + -Vector3.right * unit.Class.Radius;
                var rightRad = unit.transform.position + Vector3.right * unit.Class.Radius;

                if (rect.Contains(myCam.WorldToScreenPoint(unit.transform.position)))
                {
                    return true;

                }
                else if (rect.Contains(myCam.WorldToScreenPoint(upRad)))
                {
                    return true;

                }
                else if (rect.Contains(myCam.WorldToScreenPoint(downRad)))
                {
                    return true;

                }
                else if (rect.Contains(myCam.WorldToScreenPoint(leftRad)))
                {
                    return true;

                }
                else if (rect.Contains(myCam.WorldToScreenPoint(rightRad)))
                {
                    return true;

                }

            }

            //2nd PASS: structure
            {
                foreach (var unit in playerUnits)
                {
                    if (!unit.Class.AllUnitTags.Contains(Unit.Tag.Structure)) continue;

                    var upRad = unit.transform.position + Vector3.forward * unit.Class.Radius;
                    var downRad = unit.transform.position + -Vector3.forward * unit.Class.Radius;
                    var leftRad = unit.transform.position + -Vector3.right * unit.Class.Radius;
                    var rightRad = unit.transform.position + Vector3.right * unit.Class.Radius;

                    if (rect.Contains(myCam.WorldToScreenPoint(unit.transform.position)))
                    {
                        return true;

                    }
                    else if (rect.Contains(myCam.WorldToScreenPoint(upRad)))
                    {
                        return true;

                    }
                    else if (rect.Contains(myCam.WorldToScreenPoint(downRad)))
                    {
                        return true;

                    }
                    else if (rect.Contains(myCam.WorldToScreenPoint(leftRad)))
                    {
                        return true;

                    }
                    else if (rect.Contains(myCam.WorldToScreenPoint(rightRad)))
                    {
                        return true;

                    }

                }
            }


            //3rd PASS: Neutral/Enemy
            {
                List<GameUnit> byClosestUnit = new List<GameUnit>();
                byClosestUnit.AddRange(everyUnit.ToList());
                byClosestUnit = byClosestUnit.OrderBy((d) => (d.transform.position - centerPos).sqrMagnitude).ToList();

                foreach (var unit in byClosestUnit)
                {
       
                    if (!FOWScript.IsCoordRevealed(unit.transform.position, SyntiosEngine.CurrentFaction))
                    {
                        continue;
                    }
                    if (unit.Class.AllUnitTags.Contains(Unit.Tag.Structure)) continue;


                    if (unit.stat_faction == currentFaction)
                    {
                        continue;
                    }
                    else if (unit.stat_faction != currentFaction)
                    {
                        if (allowBoxSelectOtherFaction)
                        {

                        }
                        else if (allowBoxSelectOtherFaction == false)
                        {
                            continue;
                        }
                    }


                    var upRad = unit.transform.position + Vector3.forward * unit.Class.Radius;
                    var downRad = unit.transform.position + -Vector3.forward * unit.Class.Radius;
                    var leftRad = unit.transform.position + -Vector3.right * unit.Class.Radius;
                    var rightRad = unit.transform.position + Vector3.right * unit.Class.Radius;

                    if (rect.Contains(myCam.WorldToScreenPoint(unit.transform.position)))
                    {
                        return true;

                    }
                    else if (rect.Contains(myCam.WorldToScreenPoint(upRad)))
                    {
                        return true;

                    }
                    else if (rect.Contains(myCam.WorldToScreenPoint(downRad)))
                    {
                        return true;

                    }
                    else if (rect.Contains(myCam.WorldToScreenPoint(leftRad)))
                    {
                        return true;

                    }
                    else if (rect.Contains(myCam.WorldToScreenPoint(rightRad)))
                    {
                        return true;

                    }

                }
            }

            return false;
        }

        void SelectUnits()
        {
            var everyUnit = SyntiosEngine.Instance.ListedGameUnits;
            Rect rect = new Rect();
            Vector3 pos1 = startPosition;
            Vector3 pos2 = endPosition;
            Vector3 centerPos = (startPosition + endPosition) / 2f;

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
            bool allowBoxSelectOtherFaction = false;

            if (rect.size.x < 999 && rect.size.y < 999)
            {
                allowBoxSelectOtherFaction = true;
            }

            var currentFaction = SyntiosEngine.CurrentFaction;

            List<GameUnit> playerUnits = new List<GameUnit>();

            playerUnits = everyUnit.ToList().FindAll(s => s.stat_faction == currentFaction);
           
            //1st PASS: Player but only game unit
            foreach (var unit in playerUnits)
            {
                if (unit.Class.AllUnitTags.Contains(Unit.Tag.Structure)) continue;

                var upRad = unit.transform.position + Vector3.forward * unit.Class.Radius;
                var downRad = unit.transform.position + -Vector3.forward * unit.Class.Radius;
                var leftRad = unit.transform.position + -Vector3.right * unit.Class.Radius;
                var rightRad = unit.transform.position + Vector3.right * unit.Class.Radius;

                if (rect.Contains(myCam.WorldToScreenPoint(unit.transform.position)))
                {
                    Selection.SelectUnit(unit);
                    unit.SelectedUnit();
                }
                else if (rect.Contains(myCam.WorldToScreenPoint(upRad)))
                {
                    Selection.SelectUnit(unit);
                    unit.SelectedUnit();
                }
                else if (rect.Contains(myCam.WorldToScreenPoint(downRad)))
                {
                    Selection.SelectUnit(unit);
                    unit.SelectedUnit();
                }
                else if (rect.Contains(myCam.WorldToScreenPoint(leftRad)))
                {
                    Selection.SelectUnit(unit);
                    unit.SelectedUnit();
                }
                else if (rect.Contains(myCam.WorldToScreenPoint(rightRad)))
                {
                    Selection.SelectUnit(unit);
                    unit.SelectedUnit();
                }

            }

            //2nd PASS: structure
            if (Selection.AllSelectedUnits.Count == 0)
            {
                foreach (var unit in playerUnits)
                {
                    if (!unit.Class.AllUnitTags.Contains(Unit.Tag.Structure)) continue;

                    var upRad = unit.transform.position + Vector3.forward * unit.Class.Radius;
                    var downRad = unit.transform.position + -Vector3.forward * unit.Class.Radius;
                    var leftRad = unit.transform.position + -Vector3.right * unit.Class.Radius;
                    var rightRad = unit.transform.position + Vector3.right * unit.Class.Radius;

                    if (rect.Contains(myCam.WorldToScreenPoint(unit.transform.position)))
                    {
                        Selection.SelectUnit(unit);
                        unit.SelectedUnit();
                    }
                    else if (rect.Contains(myCam.WorldToScreenPoint(upRad)))
                    {
                        Selection.SelectUnit(unit);
                        unit.SelectedUnit();
                    }
                    else if (rect.Contains(myCam.WorldToScreenPoint(downRad)))
                    {
                        Selection.SelectUnit(unit);
                        unit.SelectedUnit();
                    }
                    else if (rect.Contains(myCam.WorldToScreenPoint(leftRad)))
                    {
                        Selection.SelectUnit(unit);
                        unit.SelectedUnit();
                    }
                    else if (rect.Contains(myCam.WorldToScreenPoint(rightRad)))
                    {
                        Selection.SelectUnit(unit);
                        unit.SelectedUnit();
                    }

                }
            }


            //3rd PASS: Neutral/Enemy
            if (Selection.AllSelectedUnits.Count == 0)
            {
                List<GameUnit> byClosestUnit = new List<GameUnit>();
                byClosestUnit.AddRange(everyUnit.ToList());
                byClosestUnit = byClosestUnit.OrderBy((d) => (d.transform.position - centerPos).sqrMagnitude).ToList();

                foreach (var unit in byClosestUnit)
                {
                    if (Selection.AllSelectedUnits.Count > 0)
                    {
                        break;
                    }
                    if (!FOWScript.IsCoordRevealed(unit.transform.position, SyntiosEngine.CurrentFaction))
                    {
                        continue;
                    }
                    if (unit.Class.AllUnitTags.Contains(Unit.Tag.Structure)) continue;


                    if (unit.stat_faction == currentFaction)
                    { 
                        continue;
                    }
                    else if (unit.stat_faction != currentFaction)
                    {
                        if (allowBoxSelectOtherFaction)
                        {

                        }
                        else if (allowBoxSelectOtherFaction == false)
                        {
                            continue;
                        }
                    }


                    var upRad = unit.transform.position + Vector3.forward * unit.Class.Radius;
                    var downRad = unit.transform.position + -Vector3.forward * unit.Class.Radius;
                    var leftRad = unit.transform.position + -Vector3.right * unit.Class.Radius;
                    var rightRad = unit.transform.position + Vector3.right * unit.Class.Radius;

                    if (rect.Contains(myCam.WorldToScreenPoint(unit.transform.position)))
                    {
                        Selection.SelectUnit(unit);
                        unit.SelectedUnit();
                    }
                    else if (rect.Contains(myCam.WorldToScreenPoint(upRad)))
                    {
                        Selection.SelectUnit(unit);
                        unit.SelectedUnit();
                    }
                    else if (rect.Contains(myCam.WorldToScreenPoint(downRad)))
                    {
                        Selection.SelectUnit(unit);
                        unit.SelectedUnit();
                    }
                    else if (rect.Contains(myCam.WorldToScreenPoint(leftRad)))
                    {
                        Selection.SelectUnit(unit);
                        unit.SelectedUnit();
                    }
                    else if (rect.Contains(myCam.WorldToScreenPoint(rightRad)))
                    {
                        Selection.SelectUnit(unit);
                        unit.SelectedUnit();
                    }

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