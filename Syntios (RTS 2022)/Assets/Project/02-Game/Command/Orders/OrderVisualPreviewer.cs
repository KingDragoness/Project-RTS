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
        public Wire prefab_Wire;
        public Transform prefab_endCircle;

        [ReadOnly]
        [ShowInInspector] private List<OrderVisualPoint> orderVisualPoints = new List<OrderVisualPoint>();
        //[ShowInInspector] private List<Wire> allWires = new List<Wire>();
        [ShowInInspector] private List<Transform> allEndCircles = new List<Transform>();

        private void Awake()
        {
            SyntiosEvents.UI_OrderMove += event_OrderMove;
            SyntiosEvents.UI_ReselectUpdate += event_Reselect;
            SyntiosEvents.UI_NewSelection += event_newSelection;
            SyntiosEvents.UI_DeselectAll += event_deselectAll;
            Tick.OnTick += event_onTick;
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
            Tick.OnTick -= event_onTick;
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

        private OrderVisualPoint GetVisualPoint()
        {
            OrderVisualPoint emptyPoint = null;  //allWires.Find(x  => x.gameObject.activeSelf == false);

            foreach (var visualPoint in orderVisualPoints) 
            {
                if (visualPoint == null) continue;
                if (visualPoint.gameObject.activeSelf == false) 
                {
                    emptyPoint = visualPoint;
                    break;
                } 
            }

            if (emptyPoint != null)
            {
                emptyPoint.gameObject.EnableGameobject(true);
                return emptyPoint;
            }

            var newPoint = Instantiate(prefab_point, Vector3.zero, Quaternion.identity, transform);
            newPoint.gameObject.name += $"-{orderVisualPoints.Count}";
            newPoint.gameObject.EnableGameobject(true);
            orderVisualPoints.Add(newPoint);

            return newPoint;
        }

        private void event_onTick(int tick)
        {

            //Refresh();
        }


        private void Refresh()
        {

            foreach(var visualPoint in orderVisualPoints)
            {
                visualPoint.gameObject.SetActive(false);
            }

            Dictionary<int, OrderVisualPoint> similarPoints = new Dictionary<int, OrderVisualPoint>();
            //assign every wire to one unit

            foreach (var unit in Selection.AllSelectedUnits)
            {
                var order_ = unit.OrderHandler.GetCurrentOrder();
                if (order_ == null) continue;

                var point = GetVisualPoint();

                point.wire.linerenderer.widthMultiplier = .1f;
                point.wire.origin = unit.transform;
                point.wire.posTarget = order_.TargetPosition();
                point.wire.InstantUpdate();
                point.attachedUnit = unit;
                point.attachedUnitOrder = order_;
                point.bigWire = false;

                int i = 0;
                Orders.UnitOrder prevOrder = order_;

                //additional orders
                foreach(var additionalOrder in unit.OrderHandler.orders)
                {
                    if (i == 0)
                    {
                        i++;
                        prevOrder = additionalOrder;
                        continue;
                    }

                    {

                        var dji = FindSimilarPoint( additionalOrder.TargetPosition());
                        if (dji != null)
                        {
                            Debug.Log(additionalOrder.TargetPosition().ToString());
                            dji.bigWireOrders.Add(additionalOrder);
                            continue;
                        }
                    }

                    var point2 = GetVisualPoint();

                    //point2.attachedUnit = unit;
                    //point2.attachedUnitOrder = additionalOrder;
                    point2.bigWire = true;
                    point2.wire.linerenderer.widthMultiplier = .2f;
                    point2.wire.origin = null;
                    if (i > 0) point2.wire.posOrigin = prevOrder.TargetPosition(); else point2.wire.posOrigin = unit.transform.position;
                    point2.wire.posTarget = additionalOrder.TargetPosition();
                    point2.wire.InstantUpdate();
                    point2.bigWire_MultipleUnitPos = additionalOrder.TargetPosition();


                    similarPoints.TryAdd(prevOrder.TargetPosition().magnitude.ToInt(), point2);
                    Debug.Log($"key: {prevOrder.TargetPosition().magnitude.ToInt()} | {point2.bigWire_MultipleUnitPos}");
                    prevOrder = additionalOrder;
                    i++;

                }
            }
        }

        public OrderVisualPoint FindSimilarPoint(Vector3 targetPosition)
        {
            foreach (var orderPoint in orderVisualPoints)
            {
                float dist = Vector3.Distance(orderPoint.bigWire_MultipleUnitPos, targetPosition);

                if (dist < 0.1f) return orderPoint;
            }

            return null;
        }

        private void Update()
        {



            foreach (var point in orderVisualPoints)
            {
                if (point == null) continue;
                if (!point.gameObject.activeSelf) continue;

                if (point.bigWire)
                {
                    bool allowDeletion = true;

                    foreach(var b_Order in point.bigWireOrders)
                    {
                        if (b_Order.isCompleted == false)
                        {
                            allowDeletion = false;
                            break;
                        }
                    }

                    if (allowDeletion)
                    {
                        //Debug.Log($"123_{point.attachedUnit}");
                        point.gameObject.SetActive(false);
                        continue;
                    }
                }
                else
                {
                    if (point.attachedUnit == null)
                    {
                        point.gameObject.SetActive(false);
                        continue;
                    }
                    if (point.attachedUnitOrder.isCompleted == true)
                    {
                        point.gameObject.SetActive(false);
                        var order_ = point.attachedUnit.OrderHandler.GetCurrentOrder();

                        if (order_ != null && Selection.AllSelectedUnits.Contains(point.attachedUnit))
                        {
                            //get new one
                            var newPoint = GetVisualPoint();

                            newPoint.wire.linerenderer.widthMultiplier = .1f;
                            newPoint.wire.origin = point.attachedUnit.transform;
                            newPoint.wire.posTarget = order_.TargetPosition();
                            newPoint.wire.InstantUpdate();
                            newPoint.attachedUnit = point.attachedUnit;
                            newPoint.attachedUnitOrder = order_;
                            newPoint.bigWire = false;

                        }

                        continue;
                    }

                    if (point.attachedUnitOrder == point.attachedUnit.OrderHandler.GetCurrentOrder() == false) point.currentOrder = false; else point.currentOrder = true;

                    if (point.currentOrder)
                    {
                        //point.wire.posTarget = point.attachedUnitOrder.TargetPosition();
                        point.wire.origin = point.attachedUnit.transform;
                    }
                }
             

                //point.startpointCircle.transform.position = point.attachedUnit.transform.position;
                //point.endpointCircle.transform.position = point.attachedUnitOrder.TargetPosition();

            }

        }
    }
}