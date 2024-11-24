using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ProtoRTS
{
	public class SyntiosEvents : MonoBehaviour
	{
		public enum Type
        {
            nullEvent = 0,
			OnGamePause = 1,

            //100-300 UI
		    PlayerDeselectAllUnits = 100,
			PlayerSelectUnit,
			PlayerOrderingUnit,

            //special game condition
            Victory = 2500,
            Defeat
        }



        public class EventClass
        {
            public Type eventType;
            public System.Action action;

            public EventClass(Type eventType)
            {
                this.eventType = eventType;
            }
        }

        private List<EventClass> allEvents = new List<EventClass>();

        private void Awake()
        {
            foreach(var enum1 in System.Enum.GetValues(typeof(Type)))
            {
                //new event
                allEvents.Add(new EventClass((Type)enum1));
            }
        }

        public static void SendUIEvents(SyntiosEvents.Type eventType)
        {

        }

	}
}