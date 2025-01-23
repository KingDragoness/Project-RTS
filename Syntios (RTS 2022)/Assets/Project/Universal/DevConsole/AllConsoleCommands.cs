using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Random = UnityEngine.Random;

namespace ProtoRTS
{
    [Serializable]
    public abstract class CC_Base
    {
        public abstract string CommandName { get; }
        public virtual string Description { get { return ""; } }

        public abstract void ExecuteCommand(string[] args);
    }

    public class CC_PrintTest : CC_Base
    {
        public override string CommandName { get { return "cc.hellow"; } }
        public override string Description { get { return "Print random stuffs."; } }


        public override void ExecuteCommand(string[] args)
        {
            int index = Random.Range(0, 4);

            if (index == 0)
            {
                DevConsole.Instance.SendConsoleMessage($"Proto;RTS started in 2024.");
            }
            else if (index == 1)
            {
                DevConsole.Instance.SendConsoleMessage($"May Lenin guide you under the SER banner!");
            }
            else if (index == 2)
            {
                DevConsole.Instance.SendConsoleMessage($"Test!");
            }
            else if (index == 3)
            {
                DevConsole.Instance.SendConsoleMessage($"Inspired by C&C and SC2!");
            }
            else if (index == 4)
            {
                DevConsole.Instance.SendConsoleMessage($"This is index 4 of cc.hellow!");
            }
            else
            {
                DevConsole.Instance.SendConsoleMessage($"HELLO WORLD!");
            }
        }
    }

    public class CC_LoadScene : CC_Base
    {
        public override string CommandName { get { return "loadlevel"; } }
        public override string Description { get { return "Load level [int index]."; } }


        public override void ExecuteCommand(string[] args)
        {
            try 
            {
                if (int.TryParse(args[0], out int levelIndex))
                {

                    Application.LoadLevel(levelIndex);
                }
            }
            catch
            {
                DevConsole.Instance.SendConsoleMessage("loadlevel <int index> | Level index not valid!");
            }
        }
    }


    public class CC_LoadMap : CC_Base
    {
        public override string CommandName { get { return "lm"; } }
        public override string Description { get { return "Load map from directory folder."; } }


        public override void ExecuteCommand(string[] args)
        {
            if (args.Length >= 1)
            {
                Map.instance.LoadMapFromProtoDir(args[0]);
            }
            else
            {
                DevConsole.Instance.SendConsoleMessage("lm <string mapName> | Name map not valid!");
            }
        }
    }

    public class CC_SummonAllToHere : CC_Base
    {
        public override string CommandName { get { return "summonall"; } }
        public override string Description { get { return "Summon all units to mouse's targeted position."; } }


        public override void ExecuteCommand(string[] args)
        {
            int layer = LayerMask.GetMask("Terrain");
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            Vector3 pointSpawn;

            if (Physics.Raycast(ray, out hit, 500f, layer, QueryTriggerInteraction.Ignore))
            {
                pointSpawn = hit.point + new Vector3(0,1,0);
                Debug.Log(ray);

                foreach (var unit in SyntiosEngine.Instance.ListedGameUnits)
                {
                    unit.transform.position = pointSpawn;
                }

            }

            DevConsole.Instance.SendConsoleMessage("Attempting summon all units to one point.");

        }
    }


    public class CC_SummonSelectedToHere : CC_Base
    {
        public override string CommandName { get { return "summon"; } }
        public override string Description { get { return "Summon selected units to mouse's targeted position."; } }


        public override void ExecuteCommand(string[] args)
        {
            int layer = LayerMask.GetMask("Terrain");
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            Vector3 pointSpawn;

            if (Physics.Raycast(ray, out hit, 500f, layer, QueryTriggerInteraction.Ignore))
            {
                pointSpawn = hit.point + new Vector3(0, 1, 0);
                Debug.Log(ray);

                foreach (var unit in Selection.AllSelectedUnits)
                {
                    unit.transform.position = pointSpawn;
                }

            }

            DevConsole.Instance.SendConsoleMessage("Attempting summon all units to one point.");

        }
    }



    public class CC_Reloadmap : CC_Base
    {
        public override string CommandName { get { return "rm"; } }
        public override string Description { get { return "Reload last loaded map (last loaded map wiped when application close)."; } }


        public override void ExecuteCommand(string[] args)
        {
            if (args.Length >= 1)
            {
                Map.instance.LoadMapFromProtoDir(Map.lastLoadedValidmap);
            }
            else
            {
                DevConsole.Instance.SendConsoleMessage("No map was loaded.");
            }
        }
    }

    public class CC_Clear : CC_Base
    {
        public override string CommandName { get { return "clear"; } }
        public override string Description { get { return "Clear console command."; } }


        public override void ExecuteCommand(string[] args)
        {
            DevConsole.Instance.consoleText.text = ">\n";
        }
    }


}