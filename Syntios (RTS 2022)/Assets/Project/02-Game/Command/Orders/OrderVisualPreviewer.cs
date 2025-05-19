using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using System.Drawing;
using static UnityEngine.UI.CanvasScaler;
using System.Linq;

namespace ProtoRTS.Game
{
	public class OrderVisualPreviewer : MonoBehaviour
	{

        public OrderVisualPoint prefab_point;
        public int limitWires = 200;
        public Wire prefab_Wire;

        [ReadOnly]
        [ShowInInspector] private Dictionary<Vector3Int, OrderVisualPoint> dictionary_VisualPoints = new Dictionary<Vector3Int, OrderVisualPoint>();
        [ShowInInspector] private List<OrderVisualPoint> allVisualPoints = new List<OrderVisualPoint>();
        [ShowInInspector] private List<Transform> allEndCircles = new List<Transform>();


        private void Awake()
        {
            SyntiosEvents.UI_OrderMove += event_OrderMove;
            SyntiosEvents.UI_ReselectUpdate += event_Reselect;
            SyntiosEvents.UI_NewSelection += event_newSelection;
            SyntiosEvents.UI_DeselectAll += event_deselectAll;
        }

        private void Start()
        {
            prefab_point.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            SyntiosEvents.UI_OrderMove -= event_OrderMove;
            SyntiosEvents.UI_ReselectUpdate -= event_Reselect;
            SyntiosEvents.UI_NewSelection -= event_newSelection;
            SyntiosEvents.UI_DeselectAll -= event_deselectAll;
        }


        private void event_deselectAll()
        {
            Refresh();
        }

        private void event_newSelection(GameUnit unit)
        {
            Refresh();
        }

        private void event_Reselect()
        {
            Refresh();
        }

        private void event_OrderMove(GameUnit gameUnit)
        {
            Refresh();
        }

        public OrderVisualPoint GetVisualPoint()
        {
            var availablePoint = allVisualPoints.Find(x => x.gameObject.activeSelf == false);

            if (availablePoint != null)
            {
                availablePoint.gameObject.SetActive(true);
                return availablePoint;
            }


            availablePoint = Instantiate(prefab_point, transform);
            availablePoint.gameObject.SetActive(true);
            allVisualPoints.Add(availablePoint);

            return availablePoint;
        }

        void Refresh()
        {
            foreach(var item in allVisualPoints) 
            { item.gameObject.SetActive(false); }

            int c = allVisualPoints.Count(x => x.gameObject.activeInHierarchy == true);

            foreach (var unit in Selection.AllSelectedUnits)
            {
                c = allVisualPoints.Count(x => x.gameObject.activeInHierarchy == true);
                if (c > limitWires) break;

                //LATER: need to be changed to defaultCommandCard.HasOrder
                if (unit.CheckFlag(Unit.Tag.Factory) && unit.behaviorTable.HasOrder(OrderClass.order_setRallyPoint))
                {
                    var visual_p = SpawnOrderVisual_rallyPoint(unit);
                    visual_p.pointType = OrderVisualPoint.PointType.RallyPoint;
                    continue;
                }

                var order = unit.behaviorTable.GetCurrentOrder();
                if (order == null) continue;

                SpawnOrderVisual(unit);
            }

        }

        public OrderVisualPoint SpawnOrderVisual_rallyPoint(GameUnit unit)
        {

            OrderVisualPoint visualPoint = GetVisualPoint();

            visualPoint.wire.origin = unit.transform;
            visualPoint.wire.posTarget = unit.trainRallyPoint;
            visualPoint.attachedUnit = unit;
            visualPoint.orderPosTarget = visualPoint.wire.posTarget.ToInt();

            if (dictionary_VisualPoints.ContainsValue(visualPoint))
            {
                var similarKey = dictionary_VisualPoints.FirstOrDefault(x => x.Value == visualPoint).Key;
                dictionary_VisualPoints.Remove(similarKey);
                dictionary_VisualPoints.TryAdd(visualPoint.orderPosTarget, visualPoint);
            }
            else
            {
                dictionary_VisualPoints.TryAdd(visualPoint.orderPosTarget, visualPoint);
            }

            visualPoint.wire.InstantUpdate();
            visualPoint.circle.transform.position = visualPoint.wire.posTarget;

            return visualPoint;
        }

        public OrderVisualPoint SpawnOrderVisual(GameUnit unit)
        {
            var order = unit.behaviorTable.GetCurrentBehavior();
            if (order == null) return null;

            OrderVisualPoint visualPoint = GetVisualPoint();

            visualPoint.wire.origin = unit.transform;
            visualPoint.wire.posTarget = order.TargetPosition();
            visualPoint.attachedOrder = order;
            visualPoint.attachedUnit = unit;
            visualPoint.orderPosTarget = visualPoint.wire.posTarget.ToInt();
            visualPoint.pointType = OrderVisualPoint.PointType.OrderQueue;

            if (dictionary_VisualPoints.ContainsValue(visualPoint))
            {
                var similarKey = dictionary_VisualPoints.FirstOrDefault(x => x.Value == visualPoint).Key;
                dictionary_VisualPoints.Remove(similarKey);
                dictionary_VisualPoints.TryAdd(visualPoint.orderPosTarget, visualPoint);
            }
            else
            {
                dictionary_VisualPoints.TryAdd(visualPoint.orderPosTarget, visualPoint);
            }

            visualPoint.wire.InstantUpdate();
            visualPoint.circle.transform.position = visualPoint.wire.posTarget;

            return visualPoint;
        }

        public bool AlreadyExistPoint(Vector3Int pos)
        {
            if (dictionary_VisualPoints.ContainsKey(pos))
                return true;

            return false;
            //return allVisualPoints.Find(x => x.orderPosTarget.x == pos_x &&
            // x.orderPosTarget.y == pos_y &&
            // x.orderPosTarget.z == pos_z);
        }

        private void Update()
        {
            int count_unit = allVisualPoints.Count(x => x.gameObject.activeInHierarchy == true); ;
            foreach (var unit in Selection.AllSelectedUnits)
            {
                if (count_unit > limitWires) break; // no need to visualize so many!

                var order = unit.behaviorTable.GetCurrentOrder();
                if (order == null) continue;
                var targetPos = order.TargetPosition();
                if (AlreadyExistPoint(targetPos.ToInt())) continue;

                SpawnOrderVisual(unit);

                count_unit++;
            }

            foreach (var item in allVisualPoints)
            {
                if (!item.gameObject.activeInHierarchy) continue;

                //if (count_vp > limitWires) break; // no need to visualize so many!

                if (item.pointType == OrderVisualPoint.PointType.OrderQueue)
                {
                    if (item.attachedUnit == null)
                    {
                        item.gameObject.SetActive(false); continue;
                    }

                    var bt = item.attachedUnit.behaviorTable;

                    if (item.attachedOrder == null)
                    {
                        item.gameObject.SetActive(false); continue;
                    }

                    if (item.attachedOrder.isCompleted)
                    {
                        item.gameObject.SetActive(false); continue;
                    }

                    if (!item.attachedUnit.behaviorTable.IsOrderQueued(item.attachedOrder))
                    {
                        item.gameObject.SetActive(false); continue;
                    }

                    //update target
                    {
                        item.wire.posTarget = item.attachedOrder.TargetPosition();
                        item.circle.transform.position = item.wire.posTarget;

                    }
                    //count_vp++;
                }
                else
                {
                    if (item.attachedUnit == null)
                    {
                        item.gameObject.SetActive(false); continue;
                    }

                    item.wire.posTarget = item.attachedUnit.trainRallyPoint;
                    item.circle.transform.position = item.wire.posTarget;

                    //count_vp++;

                }

            }
        }
    }
}