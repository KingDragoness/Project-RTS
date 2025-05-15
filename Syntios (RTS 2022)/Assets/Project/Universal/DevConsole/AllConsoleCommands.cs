using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Random = UnityEngine.Random;
using ProtoRTS.Game;

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
                DevConsole.Instance.SendConsoleMessage($"Test");
            }
            else if (index == 3)
            {
                DevConsole.Instance.SendConsoleMessage($"Inspired by C&C and SC2");
            }
            else if (index == 4)
            {
                DevConsole.Instance.SendConsoleMessage($"This is index 4 of cc.hellow");
            }
            else
            {
                DevConsole.Instance.SendConsoleMessage($"HELLO WORLD!");
            }
        }
    }

    public class CC_Devmenu : CC_Base
    {
        public override string CommandName { get { return "dev"; } }
        public override string Description { get { return "Open/close the developer menu."; } }


        public override void ExecuteCommand(string[] args)
        {
            try
            {
                DevConsole.Instance.ToggleDevMenu();
            }
            catch
            {

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

    public class CC_ResetCamHeight : CC_Base
    {
        public override string CommandName { get { return "reseth"; } }
        public override string Description { get { return "Resets camera's height."; } }


        public override void ExecuteCommand(string[] args)
        {
            RTSCamera.RestoreHeight();

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


    public class CC_KillOneSelected : CC_Base
    {
        public override string CommandName { get { return "kill"; } }
        public override string Description { get { return "Randomly kills one selected unit."; } }


        public override void ExecuteCommand(string[] args)
        {
            foreach (var unit in Selection.AllSelectedUnits)
            {
                unit.KillUnit();
                break;
            }

        }
    }

    public class CC_KillSelected : CC_Base
    {
        public override string CommandName { get { return "killall"; } }
        public override string Description { get { return "Kill all selected units."; } }


        public override void ExecuteCommand(string[] args)
        {
            foreach (var unit in Selection.AllSelectedUnits)
            {
                unit.KillUnit();
            }

            DevConsole.Instance.SendConsoleMessage($"{Selection.AllSelectedUnits.Count} units are killed.");

        }
    }

    public class CC_SetHPPercentage : CC_Base
    {
        public override string CommandName { get { return "sethp"; } }
        public override string Description { get { return "Set all selected units' HP to <int percentageHP %>."; } }


        public override void ExecuteCommand(string[] args)
        {
            try
            {
                if (float.TryParse(args[0], out float hp))
                {

                    foreach (var unit in Selection.AllSelectedUnits)
                    {
                        var hp_target = hp / 100f;

                        //Debug.Log($"{unit.ID} : {hp_target}     [{hp_target * (float)unit.Class.MaxHP}]");
                        unit.stat_HP = Mathf.RoundToInt(hp_target * (float)unit.Class.MaxHP);
                    }
                }

            
            }
            catch
            {
                DevConsole.Instance.SendConsoleMessage($"sethp <int percentageHP %> | HP value not valid!");
            }


        }
    }

    public class CC_FasterTrainingSpeed : CC_Base
    {
        public override string CommandName { get { return "operationsyn"; } }
        public override string Description { get { return "cheat code for x10 training speed."; } }


        public override void ExecuteCommand(string[] args)
        {
            try
            {

                if (SyntiosEngine.MultiplierTrainingSpeed <= 1)
                {
                    DevConsole.Instance.SendConsoleMessage($"Cheat activated.");
                    SyntiosEngine.MultiplierTrainingSpeed = 10f;
                }
                else 
                {
                    DevConsole.Instance.SendConsoleMessage($"Cheat off.");
                    SyntiosEngine.MultiplierTrainingSpeed = 1f;
                } 


            }
            catch
            {
                DevConsole.Instance.SendConsoleMessage($"operationsyn");
            }


        }
    }


    public class CC_Cheat_money : CC_Base
    {
        public override string CommandName { get { return "cc.money"; } }
        public override string Description { get { return "Adds 5k minerals and energy."; } }


        public override void ExecuteCommand(string[] args)
        {
            try
            {
                var myFactionSheet = SyntiosEngine.MyFactionSheet;
                myFactionSheet.Mineral += 5000;
                myFactionSheet.Energy += 5000;

            }
            catch
            {
                DevConsole.Instance.SendConsoleMessage("loadlevel <int index> | Level index not valid!");
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


    #region Save & Load map/game
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



    public class CC_SaveFile : CC_Base
    {
        public override string CommandName { get { return "save"; } }
        public override string Description { get { return "Save savefile [string name]."; } }


        public override void ExecuteCommand(string[] args)
        {
            try
            {

                RTS.instance.SaveGame(args[0]);

            }
            catch
            {
                DevConsole.Instance.SendConsoleMessage("save <string name> | Failed save!");
            }
        }
    }

    public class CC_DEV_UncompressedSaveFile : CC_Base
    {
        public override string CommandName { get { return "devsave"; } }
        public override string Description { get { return "Save uncompressed savefile."; } }


        public override void ExecuteCommand(string[] args)
        {
            try
            {

                RTS.instance.DEVELOPER_UncompressedSave();

            }
            catch
            {
                DevConsole.Instance.SendConsoleMessage("devsave failed");
            }
        }
    }

    public class CC_DEV_LoadUncompressedSaveFile : CC_Base
    {
        public override string CommandName { get { return "devload"; } }
        public override string Description { get { return "Load uncompressed savefile."; } }


        public override void ExecuteCommand(string[] args)
        {
            try
            {

                RTS.instance.DEVELOPER_LoadDevSave();

            }
            catch
            {
                DevConsole.Instance.SendConsoleMessage("devload failed");
            }
        }
    }

    public class CC_LoadSaveFile : CC_Base
    {
        public override string CommandName { get { return "load"; } }
        public override string Description { get { return "Load save file [string name]."; } }


        public override void ExecuteCommand(string[] args)
        {
            try
            {

                RTS.instance.LoadGame(args[0]);

            }
            catch
            {
                DevConsole.Instance.SendConsoleMessage("load <string name> | No save file loaded!");
            }
        }
    }



    #endregion


}