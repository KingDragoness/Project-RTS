using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using System.Drawing;
using static UnityEngine.UI.CanvasScaler;

namespace ProtoRTS.Game
{
	public class OrderVisualPreviewer : MonoBehaviour
	{

        public OrderVisualPoint prefab_point;
        public int limitWires = 200;
        public Wire prefab_Wire;

        [ReadOnly]
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

                OrderVisualPoint visualPoint = GetVisualPoint();

                visualPoint.wire.origin = unit.transform;
                visualPoint.wire.posTarget = order.TargetPosition();
                visualPoint.attachedOrder = order;
                visualPoint.attachedUnit = unit;
                visualPoint.orderPosTarget = visualPoint.wire.posTarget.ToInt();
            }
        }

        public bool AlreadyExistPoint(Vector3Int v3)
        {
            return allVisualPoints.Find(x => x.orderPosTarget == v3);
        }

        private void Update()
        {
            int count_unit = 0;
            foreach (var unit in Selection.AllSelectedUnits)
            {
                if (count_unit > limitWires) break; // no need to visualize so many!

                Orders.UnitOrder order = unit.OrderHandler.GetCurrentOrder();
                if (order == null) continue;
                if (AlreadyExistPoint(order.TargetPosition().ToInt())) continue;

                OrderVisualPoint visualPoint = GetVisualPoint();

                visualPoint.wire.origin = unit.transform;
                visualPoint.wire.posTarget = order.TargetPosition();
                visualPoint.attachedOrder = order;
                visualPoint.attachedUnit = unit;
                visualPoint.orderPosTarget = visualPoint.wire.posTarget.ToInt();
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
                count_vp++;
            }
        }
    }
}