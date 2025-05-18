using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Newtonsoft.Json;
using UnityEditor;
using System;

namespace ProtoRTS.Game
{

    //TRAINING A UNIT IS NOT AN ORDER!
    [System.Serializable]
    public class TrainingQueue
    {
        [JsonIgnore] public SO_GameUnit gameUnitClass;
        public string gameUnitID;
        public float timeTrained = 0;

        public TrainingQueue(SO_GameUnit gameUnitClass)
        {
            this.gameUnitClass = gameUnitClass;
        }

        public void Save()
        {
            gameUnitID = gameUnitClass.ID;
        }

        public void ResolveReference()
        {
            gameUnitClass = DynamicAssetStorage.Instance.FindGameUnitClass(gameUnitID);
        }
    }

    [System.Serializable]
    public class OrderEntry
    {
        [JsonIgnore] public GameUnit targetUnit;
        [JsonIgnore] public bool isCompleted;
        public OrderClass orderClass;
        public string buttonID;
        public string targetUnitID;
        public Vector3 targetPosition;
        public Vector3 targetPosition2;

        //public static OrderEntry NewEntry(OrderClass orderClass, GameUnit targetUnit, Vector3 targetPosition)
        //{
        //    OrderEntry entry = new OrderEntry();
        //    entry.orderClass = orderClass;
        //    entry.targetUnit = targetUnit;
        //    entry.targetPosition = targetPosition;
        //    return entry;
        //}

        public virtual Vector3 TargetPosition()
        {
            if (targetUnit == null)
            {
                return targetPosition;
            }
            return targetUnit.transform.position;
        }

        public void PassData(OrderUnit order)
        {
            targetPosition = order.targetPos;
            targetPosition2 = order.targetPos2;
            orderClass = order.OrderClassType;
            buttonID = order.buttonID;
            if (targetUnit != null) targetUnitID = order.targetUnit.Class.ID;
        }
    }


    [System.Serializable]
    public abstract class OrderUnit : MonoBehaviour
    {

        public GameUnit gameUnit;
        public string buttonID = "";
        public OrderClass OrderClassType;

        //RUNTIME
        public int TickPassed = 0;
        public Vector3 targetPos;
        public Vector3 targetPos2;
        public GameUnit targetUnit;
        public bool isCompleted;
        //


        public abstract void Active();
        public abstract void CheckBackground();
        public abstract bool IsButtonAllowed();

        public virtual OrderEntry Save()
        {
            OrderEntry saveDat = new OrderEntry();
            saveDat.PassData(this);
            return saveDat;
        }
        public virtual void LoadData(OrderEntry saveData)
        {

        }
        public virtual bool IgnoreQueue()
        {
            return false;
        }

     

        public virtual OrderEntry NewOrder(OrderClass orderClass, string buttonID, GameUnit targetUnit, Vector3 targetPosition)
        {
            OrderEntry saveDat = new OrderEntry();
            saveDat.targetPosition = targetPosition;
            saveDat.targetUnit = targetUnit;
            saveDat.orderClass = orderClass;
            saveDat.buttonID = buttonID;
            return saveDat;
        }

        public virtual void InitializeButton(UnitButtonCommand button)
        {
            buttonID = button.buttonID;
        }
        public abstract Vector3 TargetPosition();
        public abstract UnitAbility.TargetType[] TargetCriteria();


        public OrderClass GetClassType()
        {

            if (this is Order_move)
            {
                return OrderClass.order_move;
            }
            if (this is Order_stop)
            {
                return OrderClass.order_stop;
            }
            if (this is Order_patrol)
            {
                return OrderClass.order_patrol;
            }
            if (this is Order_holdPosition)
            {
                return OrderClass.order_holdPosition;
            }
            if (this is Order_trainUnit)
            {
                return OrderClass.order_train_unit;
            }
            if (this is Order_setRallyPoint)
            {
                return OrderClass.order_setRallyPoint;
            }
            return OrderClass.Null;
        }
        public static OrderUnit NewClassUnit(OrderClass orderClass)
        {

            switch (orderClass)
            {
                case OrderClass.Null:
                    break;
                case OrderClass.order_idle:
                    break;
                case OrderClass.order_move:
                    return new GameObject().AddComponent<Order_move>();
                case OrderClass.order_stop:
                    return new GameObject().AddComponent<Order_stop>();
                case OrderClass.order_attack:
                    break;
                case OrderClass.order_holdPosition:
                    return new GameObject().AddComponent<Order_holdPosition>();
                case OrderClass.order_patrol:
                    return new GameObject().AddComponent<Order_patrol>();
                case OrderClass.order_castGenericSpell:
                    break;
                case OrderClass.order_returnCargo:
                    break;
                case OrderClass.order_mineResources:
                    break;
                case OrderClass.order_switchState:
                    break;
                case OrderClass.order_repair:
                    break;
                case OrderClass.order_train_unit:
                    return new GameObject().AddComponent<Order_trainUnit>();
                case OrderClass.order_morph_unit:
                    break;
                case OrderClass.order_build_Dionarian:
                    break;
                case OrderClass.order_build_Soviet:
                    break;
                case OrderClass.order_build_Mobius:
                    break;
                case OrderClass.order_build_TitanSixtus:
                    break;
                case OrderClass.order_constructing:
                    break;
                case OrderClass.order_enter_building:
                    break;
                case OrderClass.order_exit_building:
                    break;
                case OrderClass.order_building_liftoff:
                    break;
                case OrderClass.order_building_land:
                    break;
                case OrderClass.order_transport_load:
                    break;
                case OrderClass.order_transport_exitOne:
                    break;
                case OrderClass.order_transport_exitAll:
                    break;
                case OrderClass.order_setRallyPoint:
                    return new GameObject().AddComponent<Order_setRallyPoint>();

            }

            if (SyntiosEngine.Instance.engine_PrintErrorMissingBehavior) Debug.LogError("BEHAVIOR NOT YET IMPLEMENTED");
            return null;
        }
    }

    public enum OrderClass
    {
        Null,
        order_idle,
        order_move,
        order_stop,
        order_attack,
        order_holdPosition,
        order_patrol,
        order_castGenericSpell,
        order_returnCargo,
        order_mineResources,
        order_switchState,
        order_repair,
        order_train_unit,
        order_morph_unit,
        order_build_Dionarian = 100,
        order_build_Soviet,
        order_build_Mobius,
        order_build_TitanSixtus,
        order_constructing,
        order_setRallyPoint,
        order_enter_building = 150,
        order_exit_building,
        order_building_liftoff,
        order_building_land,
        order_transport_load,
        order_transport_exitOne,
        order_transport_exitAll,
    }


}