using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ProtoRTS
{
	public class Unit
	{

		public enum Tag
        {
			Armored,
			Biological,
			Mechanical,
			Heroic,
			Hover,
			Light,
			Boss,
			Object,
			Massive,
			Spellcaster,
			Robotic,
			Structure
        }

		public enum Player
        {
			neutral,
			Player1,
			Player2,
			Player3,
			Player4,
			Player5,
			Player6,
			Player7,
			Player8,
			Player9,//unused
			Observer = 100
        }

	}
}