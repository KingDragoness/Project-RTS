using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ProtoRTS.Game
{

    public class Orders
    {

        //queueable orders:
        //Move
        //Attack
        //Repair
        //Constructing
        //Gather resources
        //Place building
        //Transport unit

        [System.Serializable]
        public abstract class UnitOrder
        {
            public bool isCompleted = false;

            public abstract bool IsObjectiveAchieved(GameUnit myUnit);
            public abstract void Run(GameUnit myUnit);
            public abstract Vector3 TargetPosition();
            public virtual bool IsOrderExistsInUnit(GameUnit myUnit) { return myUnit.OrderHandler.orders.Contains(this); }
        }

        [System.Serializable]
        public class Order_Move : UnitOrder
        {

            public GameUnit target;
            public Vector3 positionTarget;

            public Order_Move(GameUnit target, Vector3 positionTarget)
            {
                this.target = target;
                this.positionTarget = positionTarget;
            }

            public override bool IsObjectiveAchieved(GameUnit myUnit)
            {
                if (target == null)
                {
                    float distance1 = Vector3.Distance(myUnit.transform.position, positionTarget);

                    if (distance1 < myUnit.Class.Radius + 0.5f)
                    {
                        isCompleted = true;
                        return true;
                    }

                    return false;
                }

                float distance = Vector3.Distance(myUnit.transform.position, target.transform.position);

                if (distance < myUnit.Class.Radius + 0.5f)
                {
                    isCompleted = true;
                    return true;
                }

                return false;
            }

            public override void Run(GameUnit myUnit)
            {
                if (target != null)
                {
                    myUnit.move_TargetUnit = target;
                }
                else
                {
                    myUnit.move_Target = positionTarget;
                    myUnit.move_TargetUnit = null;
                }
            }

            public override Vector3 TargetPosition()
            {
                return positionTarget + new Vector3(0,0.07f,0);
            }
        }

        [System.Serializable]
        public class Order_Stop : UnitOrder
        {
            public override bool IsObjectiveAchieved(GameUnit myUnit)
            {
                isCompleted = true;
                return true;
            }

            public override void Run(GameUnit myUnit)
            {
                myUnit.move_Target = myUnit.transform.position;
                myUnit.move_TargetUnit = null;
            }

            public override Vector3 TargetPosition()
            {
                return Vector3.zero;
            }
        }


        [System.Serializable]
        public class Order_Attack : UnitOrder
        {

            public GameUnit enemyUnit;


            public override bool IsObjectiveAchieved(GameUnit myUnit)
            {
                if (enemyUnit == null)
                {
                    isCompleted = true;
                    return true;
                }

                float distance = Vector3.Distance(myUnit.transform.position, enemyUnit.transform.position);

                if (distance < myUnit.Class.Radius)
                {
                    isCompleted = true;
                    return true;
                }

                return false;
            }

            public override void Run(GameUnit myUnit)
            {
                if (enemyUnit == null)
                {
                    //invalid run
                    return;
                }

                myUnit.move_TargetUnit = enemyUnit;

            }

            public override Vector3 TargetPosition()
            {
                return enemyUnit.transform.position;
            }
        }

        [System.Serializable]
        public class Order_Patrol : UnitOrder
        {
            public Vector3 startPoint;
            public Vector3 endPoint;
            bool b = false;

            public Order_Patrol(Vector3 _start, Vector3 _end)
            {
                this.startPoint = _start;
                this.endPoint = _end;
            }

            public override bool IsObjectiveAchieved(GameUnit myUnit)
            {
                //never completes, Patrol will never stack
                return false;
            }

            public override void Run(GameUnit myUnit)
            {
                myUnit.move_TargetUnit = null;

                Vector3 targetPoint = startPoint;

                if (b)
                {
                    targetPoint = startPoint;
                }
                else
                {
                    targetPoint = endPoint;
                }

                float distance = Vector3.Distance(myUnit.transform.position, targetPoint);

                if (distance < myUnit.Class.Radius + 0.5f)
                {
                    b = !b;
                }


                myUnit.move_Target = targetPoint;

            }

            public override Vector3 TargetPosition()
            {
                Vector3 targetPoint = startPoint;

                if (b)
                {
                    targetPoint = startPoint;
                }
                else
                {
                    targetPoint = endPoint;
                }

                targetPoint += new Vector3(0, 0.07f, 0);

                return targetPoint;
            }
        }
    }


}