using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ProtoRTS
{
	public class ExecuteConsoleCommand : MonoBehaviour
	{

		public string command = "";

		public void ExecuteCommand()
		{
			DevConsole.Instance.CommandInput(command);
		}

	}
}