using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

namespace ProtoRTS
{
	public class DevConsole : MonoBehaviour
	{

		public InputField inputField;
		public Text consoleText;
        public GameObject consoleInputObject;
        public GameObject DevMenu;

        public delegate bool OnExecuteCommand(string commandName, string[] args); //Return success
        public static event OnExecuteCommand onExecuteCommand;

        private List<string> historyCommands = new List<string>()
    {
        "",
    };

        private int index = 0;

        private static DevConsole _instance;

        public static DevConsole Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<DevConsole>();

                return _instance;
            }

            set
            {
                _instance = value;
            }
        }


        private void Awake()
        {
            Instance = this;
        }

        public void Update()
        {
            if (inputField.isFocused == false)
            {
                inputField.Select();
                inputField.ActivateInputField();
            }

            if (Input.GetKeyUp(KeyCode.Return))
            {
                index = 0;
                CommandInput(inputField.text);
            }

            if (Input.GetKeyUp(KeyCode.UpArrow))
            {
                index++;
                if (index >= historyCommands.Count)
                {
                    index = historyCommands.Count - 1;
                }
                inputField.SetTextWithoutNotify(historyCommands[index]);
            }

            if (Input.GetKeyUp(KeyCode.D) && Input.GetKey(KeyCode.LeftShift))
            {
                ToggleDevMenu();
            }

            if (Input.GetKeyUp(KeyCode.DownArrow))
            {
                index--;
                if (index < 0)
                {
                    index = 0;
                }
                inputField.SetTextWithoutNotify(historyCommands[index]);
            }
        }

        public void ToggleDevMenu()
        {
            DevMenu.gameObject.EnableGameobject(!DevMenu.gameObject.activeSelf);

        }

        public void CommandInput(string command)
        {
            string[] inputSplit = command.Split(' ');

            string commandInput = inputSplit[0];
            string[] args = inputSplit.Skip(1).ToArray();

            SendConsoleMessage("<color=#ffffffcc>" + command + "</color>");

            //take a look at other games (mobius lab building game one)
            ProcessCommand(commandInput, args);

            historyCommands.Insert(0, command);
            inputField.Select();
        }

        public void ProcessCommand(string commandInput, string[] args)
        {
            var listCommands = GetAll().ToList();

            foreach (var command in listCommands)
            {
                if (command.CommandName == commandInput)
                {
                    Debug.Log($"Command executed: {command.CommandName}");
                    command.ExecuteCommand(args);
                    break;
                }
            }
        }


        public void SendConsoleMessage(string msg)
        {
            if (consoleText != null) consoleText.text += "> " + msg + "\n";
        }

        public IEnumerable<CC_Base> GetAll()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsSubclassOf(typeof(CC_Base)))
                .Select(type => Activator.CreateInstance(type) as CC_Base);
        }
    }


    public class CC_Help : CC_Base
    {
        public override string CommandName { get { return "help"; } }
        public override string Description { get { return "List of all commands."; } }


        public override void ExecuteCommand(string[] args)
        {
            string help_strContent = "";
            var listCommands = DevConsole.Instance.GetAll().ToList();

            int count = 0;
            int indexStart = 0;
            int countPer = 10;
            int totalCommand = listCommands.Count;

            if (args.Length >= 1)
            {
                if (int.TryParse(args[0], out int moneyAmount))
                {
                    indexStart = moneyAmount;
                }
                else
                {

                }
            }

            DevConsole.Instance.SendConsoleMessage($"====================== HELP [{indexStart}/{Mathf.Floor(totalCommand / countPer)}] ====================== ");
            DevConsole.Instance.SendConsoleMessage($"Press ENTER to execute command");
            DevConsole.Instance.SendConsoleMessage($"Press ~ key to toggle console");

            for (int x = indexStart * countPer; x < (indexStart + 1) * countPer; x++)
            {
                if (listCommands.Count <= x) break;

                DevConsole.Instance.SendConsoleMessage($"'<color=#ffffffcc>{listCommands[x].CommandName}</color>' {listCommands[x].Description}");

            }

        }
    }
}