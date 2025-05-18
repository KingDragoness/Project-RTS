using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ProtoRTS.Game
{
	public class Order_trainUnit : OrderUnit
	{

        public SO_GameUnit unitToTrain;

        public override void Active()
        {
            if (gameUnit.trainingQueue.Count >= 5)
            {
                //FULL
                
                isCompleted = true;
                return;
            }
            if (!SyntiosEngine.Instance.CheckMineralEnough(unitToTrain.MineralCost))
            {
                UI.PromptHelp.OpenPrompt("You are required to mine more minerals.", 7f);
                isCompleted = true;
                return;
            }
            if (!SyntiosEngine.Instance.CheckEnergyEnough(unitToTrain.EnergyCost))
            {
                UI.PromptHelp.OpenPrompt("You are required to extract more energy.", 7f);
                isCompleted = true;
                return;
            }

            gameUnit.trainingQueue.Add(new TrainingQueue(unitToTrain));
            var myFaction = gameUnit.Unit_FactionSheet();
            myFaction.Mineral -= unitToTrain.MineralCost;
            myFaction.Energy -= unitToTrain.EnergyCost;
            isCompleted = true;
        }

        public override void InitializeButton(UnitButtonCommand button)
        {
            base.InitializeButton(button);
            unitToTrain = button.unitSO;
        }

        public override void CheckBackground()
        {

        }

        public override bool IsButtonAllowed()
        {
            //if not moving
            return true;
        }

        public override Vector3 TargetPosition()
        {
            return Vector3.zero;
        }

        public override UnitAbility.TargetType[] TargetCriteria()
        {
            return new UnitAbility.TargetType[0];
        }
    }
}