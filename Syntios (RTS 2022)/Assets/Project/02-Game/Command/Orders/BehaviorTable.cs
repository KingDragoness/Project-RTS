using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

namespace ProtoRTS.Game
{
    public enum QueueOrder
    {
        Override,
        Additive
    }



    public class BehaviorTable : MonoBehaviour
	{
 


        public List<OrderEntry> allQueuedOrders = new List<OrderEntry>();
        public List<OrderUnit> allOrdersAvailable = new List<OrderUnit>();
        public GameUnit gameUnit;

        private bool hasInstallBehaviors = false;

        private void OnEnable()
        {
            Tick.OnTick += event_OnTick;
        }

        private void OnDisable()
        {
            Tick.OnTick -= event_OnTick;
        }

        private void Awake()
        {
            if (gameUnit == null) gameUnit = gameObject.GetComponentInParent<GameUnit>();
            InstallBehaviors(); //RTS.IsLoadFromSaveFile will be clared at start
        }

        public void InstallBehaviors()
        {
            //auto generate idle
            foreach(var cc in gameUnit.Class.commandCards)
            {
                foreach (var command in cc.commands)
                {
                    try
                    {
                        var newClass = AttemptGenerateCommand(command.orderClass);

                        if (newClass == null) continue;
                        newClass.InitializeButton(command);
                        newClass.gameUnit = gameUnit;
                        newClass.OrderClassType = command.orderClass;
                        allOrdersAvailable.Add(newClass);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                    
                }
            }
        }

        public void LoadBehaviorData()
        {
            foreach (var order in allOrdersAvailable)
            {
                try
                {
                    if (RTS.IsLoadFromSaveFile)
                    {
                        var gameUnitDat = RTS.cachedLoadedSave.allUnits.Find(x => x.guid == gameUnit.guid);
                        //Debug.Log(gameUnitDat);

                        if (gameUnitDat != null)
                        {
                            OrderEntry orderDat = gameUnitDat.allOrders.Find(x => x.orderClass == order.OrderClassType && x.buttonID == order.buttonID);

                            if (orderDat != null)
                                order.LoadData(orderDat);

                            //Debug.Log(orderDat);
                        }
                    }

                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }

            }
           

        }

        private OrderUnit AttemptGenerateCommand(OrderClass orderClass)
        {
            //var similar = GetOrderBy(orderClass);
            //if (similar != null) { return null; }

            var newClassUnit = OrderUnit.NewClassUnit(orderClass);
            if (!SyntiosEngine.Instance.engine_PrintErrorMissingBehavior && newClassUnit == null)
            {
                return null;
            }

            var go1 = newClassUnit.gameObject;
            go1.transform.SetParent(transform);
            go1.gameObject.name = newClassUnit.GetClassType().ToString();   

            return newClassUnit;
        }

        private void event_OnTick(int tick)
        {
            if (allQueuedOrders.Count > 0)
            {
                var order_at_1 = allQueuedOrders[0];
                var orderScript = GetOrderBy(order_at_1.orderClass, order_at_1.buttonID);

                if (orderScript.isCompleted)
                {
                    allQueuedOrders[0].isCompleted = true; //only for visual order
                    allQueuedOrders.RemoveAt(0);
                    orderScript.isCompleted = false;
                }
                else
                {
                    orderScript.TickPassed++;
                    orderScript.targetUnit = allQueuedOrders[0].targetUnit;
                    orderScript.targetPos = allQueuedOrders[0].targetPosition;
                    orderScript.targetPos2 = allQueuedOrders[0].targetPosition2;
                    orderScript.Active();
                }
            }

            foreach (var order in allOrdersAvailable)
            {
                if (tick % 2 == 0)
                {
                    order.CheckBackground();
                }
            }
        }

   

        public void IssueOrder(OrderClass orderClass, string buttonID = "", GameUnit targetUnit = null, Vector3 targetPosition = new Vector3(), QueueOrder isQueueing = QueueOrder.Override)
        {
            OrderUnit order = GetOrderBy(orderClass, buttonID); //issue order by shared generic command (move, patrol, attack, cloak)
            

            if (order != null)
            {
                var button_ds = buttonID;

                if (button_ds == null)
                {
                    //if there is multiple same class, do not assign and give up
                }

                OrderEntry entry = order.NewOrder(orderClass, buttonID, targetUnit, targetPosition); //OrderEntry.NewEntry(orderClass, targetUnit, targetPosition);

                if (isQueueing == QueueOrder.Additive)
                {
                    allQueuedOrders.Add(entry);
                }
                else
                {
                    allQueuedOrders.Clear();
                    allQueuedOrders.Add(entry);
                }
            }
        }


        public bool HasOrder(OrderClass orderClass)
        {
            return allOrdersAvailable.Find(x => x.OrderClassType == orderClass) != null ? true : false;
        }

        public OrderUnit GetOrderBy(OrderClass orderClass, string buttonID)
        {
            OrderUnit ou = allOrdersAvailable.Find(x => x.OrderClassType == orderClass && x.buttonID == buttonID);

            if (ou == null)
            {
                return allOrdersAvailable.Find(x => x.OrderClassType == orderClass);
            }
            else
            {
                return ou;
            }

            switch (orderClass)
            {
                //case OrderClass.order_attack:
                //    break;
                //case OrderClass.Null:
                //    break;
                //case OrderClass.order_idle:
                //    break;
                //case OrderClass.order_move:
                //    return allOrdersAvailable.Find(x => x is (Order_move));
                //case OrderClass.order_stop:
                //    return allOrdersAvailable.Find(x => x is (Order_stop));
                //case OrderClass.order_holdPosition:
                //    break;
                //case OrderClass.order_patrol:
                //    return allOrdersAvailable.Find(x => x is (Order_patrol));
                //case OrderClass.order_castGenericSpell:
                //    break;
                //case OrderClass.order_returnCargo:
                //    break;
                //case OrderClass.order_mineResources:
                //    break;
                //case OrderClass.order_switchState:
                //    break;
                //case OrderClass.order_repair:
                //    break;
                //case OrderClass.order_train_unit:
                //    return allOrdersAvailable.Find(x => x is (Order_trainUnit));
                //case OrderClass.order_morph_unit:
                //    break;
                //case OrderClass.order_build_Dionarian:
                //    break;
                //case OrderClass.order_build_Soviet:
                //    break;
                //case OrderClass.order_build_Mobius:
                //    break;
                //case OrderClass.order_build_TitanSixtus:
                //    break;
                //case OrderClass.order_enter_building:
                //    break;
                //case OrderClass.order_exit_building:
                //    break;
                //case OrderClass.order_building_liftoff:
                //    break;
                //case OrderClass.order_building_land:
                //    break;
                //case OrderClass.order_transport_load:
                //    break;
                //case OrderClass.order_transport_exitOne:
                //    break;
                //case OrderClass.order_transport_exitAll:
                //    break;
            }

            Debug.Log("NULL class");
            return null;
        }

        internal OrderEntry GetCurrentOrder()
        {
            if (allQueuedOrders.Count > 0)
            {
                return allQueuedOrders[0];
            }

            return null;
        }

        public OrderUnit GetCurrentBehavior()
        {
            if (allQueuedOrders.Count == 0)
            {
                return null;
            }
            var order_at_1 = allQueuedOrders[0];
            var orderScript = GetOrderBy(order_at_1.orderClass, order_at_1.buttonID);

            return orderScript;
        }

        [FoldoutGroup("Debug")]
        [Button("test1")]
        public bool IsOrderQueued(OrderUnit orderUnit)
        {
            var orderentry = allQueuedOrders.Find(x => x.buttonID == orderUnit.buttonID && x.orderClass == orderUnit.OrderClassType);

            return orderentry != null;
        }

        internal List<OrderEntry> SaveAll()
        {
            List<OrderEntry> savedEntry = new List<OrderEntry>();

            foreach (var order in allQueuedOrders)
            {
                var behavior = GetOrderBy(order.orderClass, order.buttonID);
                savedEntry.Add(behavior.Save());
            }

            return savedEntry;
        }

        internal bool IsRunning(UnitButtonCommand button)
        {
            var orderentry = allQueuedOrders.Find(x => x.buttonID == button.buttonID && x.orderClass == button.orderClass);
            //Debug.Log($"{button.buttonID} : {button.orderClass} [{orderentry}]");
            return orderentry != null ? true : false;
        }
    }
}