using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ProtoRTS.MapEditor
{

    public class CC_SaveMap : CC_Base
    {
        public override string CommandName { get { return "savemap"; } }
        public override string Description { get { return "Save map to the directory folder."; } }


        public override void ExecuteCommand(string[] args)
        {
            if (MapEdit.instance == null)
            {
                DevConsole.Instance.SendConsoleMessage("Map editor not loaded!");
                throw new System.Exception("Map editor not loaded!");
            }

            if (args.Length >= 1)
            {
                MapEdit.instance.SaveGame(args[0]);
                DevConsole.Instance.SendConsoleMessage($"Map successfully saved as {args[0]}.map");
            }
            else
            {
                DevConsole.Instance.SendConsoleMessage("Name map not valid!");
            }
        }
    }
}