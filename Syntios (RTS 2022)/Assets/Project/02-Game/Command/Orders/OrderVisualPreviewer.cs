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

            foreach (var unit in Selection.AllSelectedUnits)
            {
                Orders.UnitOrder order = unit.OrderHandler.GetCurrentOrder();
                if (order == null) continue;

                SpawnOrderVisual(unit);
            }
        }

        public void SpawnOrderVisual(GameUnit unit)
        {
            Orders.UnitOrder order = unit.OrderHandler.GetCurrentOrder();
            if (order == null) return;

            OrderVisualPoint visualPoint = GetVisualPoint();

            visualPoint.wire.origin = unit.transform;
            visualPoint.wire.posTarget = order.TargetPosition();
            visualPoint.attachedOrder = order;
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
            int count_unit = 0;
            foreach (var unit in Selection.AllSelectedUnits)
            {
                if (count_unit > limitWires) break; // no need to visualize so many!

                Orders.UnitOrder order = unit.OrderHandler.GetCurrentOrder();
                if (order == null) continue;
                var targetPos = order.TargetPosition();
                if (AlreadyExistPoint(targetPos.ToInt())) continue;

                SpawnOrderVisual(unit);

                count_unit++;
            }

            int count_vp = 0;
            foreach (var item in allVisualPoints)
            {
                if (count_vp > limitWires) break; // no need to visualize so many!

                if (item.attachedUnit == null)
                {
                    item.gameObject.SetActive(false); continue;
                }

                if (item.attachedOrder.isCompleted)
                {
                    item.gameObject.SetActive(false); continue;
                }

                if (!item.attachedOrder.IsOrderExistsInUnit(item.attachedUnit))
                {
                    item.gameObject.SetActive(false); continue;
                }

                //update target
                {
                    item.wire.posTarget = item.attachedOrder.TargetPosition();
                    item.circle.transform.position = item.wire.posTarget;

                }
                count_vp++;
            }
        }
    }
}