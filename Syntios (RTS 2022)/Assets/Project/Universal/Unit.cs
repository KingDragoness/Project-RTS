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
			Structure,
			Factory,
			Enterable,
			Healer, //if unit has repair or heal ability
			SupplyProvider
        }

		public enum Race
		{
			Neutral,
            Test,
			Mobius,
			Sovzi,
			Sixtus,
			Dionary,
			Titan
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

		public enum TypePlayer
		{
			Player,
			Enemy,
			Neutral		
		}

		public static Color GetColor(Player player)
        {
			if (player == Player.Player1)
            {
				return Player1_Color;
            }

			return Neutral_Color;
        }

		public static Color Neutral_Color = new Color(0.301f, 0.941f, 0.856f, 1f);
		public static Color Player1_Color = new Color(1f, 0f, 0f, 1f);
		public static Color Player2_Color = new Color(0f, 0f, 1f, 1f);
		public static Color Player3_Color = new Color(0f, 1f, 0f, 1f);
		public static Color Player4_Color = new Color(0.7f, 0f, 0.7f, 1f);


		
		public static void Clear()
        {

        }
	}

}