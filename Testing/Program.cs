using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRage;
using VRageMath;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
        }

        // Argument Parser
        MyCommandLine CommandLine = new MyCommandLine();

        // List of all airlocks currently opening
        List<Airlock> Airlocks = new List<Airlock>();
        List<Airlock> AirlockClearList = new List<Airlock>();
        // INI Object
        MyIni ini = new MyIni();
        public void Main(string argument, UpdateType updateType)
        {
            if ((updateType & UpdateType.Trigger) != 0)
            {
                Airlock airlock = new Airlock();
                // Parse Arguments
                if (CommandLine.TryParse(argument))
                {
                    airlock.DoorSide = CommandLine.Argument(0);
                    airlock.AirlockName = CommandLine.Argument(1);
                }

                // Get a list of all doors for this script
                GridTerminalSystem.GetBlocksOfType<IMyDoor>(airlock.Doors, door => MyIni.HasSection(door.CustomData, "AirlockScript"));
                // Get a list of all airvents for this script
                GridTerminalSystem.GetBlocksOfType<IMyAirVent>(airlock.AirVents, vent => MyIni.HasSection(vent.CustomData, "AirlockScript"));
                // Get a list of all lights for this script
                GridTerminalSystem.GetBlocksOfType<IMyLightingBlock>(airlock.Lights, vent => MyIni.HasSection(vent.CustomData, "AirlockScript"));
                // Get a list of all lights for this script
                GridTerminalSystem.GetBlocksOfType<IMySoundBlock>(airlock.SoundBlocks, vent => MyIni.HasSection(vent.CustomData, "AirlockScript"));

                // Go through each door in the list until the custom data matches with the inputed arguments
                foreach (IMyDoor door in airlock.Doors)
                {
                    MyIniParseResult result;
                    if (!ini.TryParse(door.CustomData, out result))
                    {
                        throw new Exception(result.ToString());
                    }

                    if(ini.Get("AirlockScript", "AirlockName").ToString() == airlock.AirlockName)
                    {
                        // If the airlock was triggered by the sensor, determine which door needs to close
                        // Door side passed in should be the door currently open.
                        if (airlock.DoorSide == "Sensor")
                        {
                            if (door.OpenRatio == 1)
                            {
                                airlock.DoorSide = ini.Get("AirlockScript", "DoorSide").ToString();
                            }
                        }
                        if (ini.Get("AirlockScript", "DoorSide").ToString() == airlock.DoorSide)
                        {
                            airlock.OpenDoors.Add(door);
                        }
                        else
                        {
                            airlock.ClosedDoors.Add(door);
                        }
                    }
                }

                foreach (IMyAirVent vent in airlock.AirVents)
                {
                    MyIniParseResult result;
                    if (!ini.TryParse(vent.CustomData, out result))
                    {
                        throw new Exception(result.ToString());
                    }

                    if (ini.Get("AirlockScript", "AirlockName").ToString() == airlock.AirlockName)
                    {
                        airlock.ThisAirlockAirVents.Add(vent);
                    }
                }

                foreach (IMyLightingBlock light in airlock.Lights)
                {
                    MyIniParseResult result;
                    if (!ini.TryParse(light.CustomData, out result))
                    {
                        throw new Exception(result.ToString());
                    }

                    if (ini.Get("AirlockScript", "AirlockName").ToString() == airlock.AirlockName)
                    {
                        airlock.AirlockLights.Add(light);
                    }
                }

                foreach (IMySoundBlock sb in airlock.SoundBlocks)
                {
                    MyIniParseResult result;
                    if (!ini.TryParse(sb.CustomData, out result))
                    {
                        throw new Exception(result.ToString());
                    }

                    if (ini.Get("AirlockScript", "AirlockName").ToString() == airlock.AirlockName)
                    {
                        airlock.AirlockSoundBlocks.Add(sb);
                    }
                }
                Airlocks.Add(airlock);
            }
            if ((updateType & UpdateType.Update10) != 0 && Airlocks.Count() > 0)
            {
                foreach(Airlock airlock in Airlocks)
                {
                    airlock.AirlockSwitch();
                    if (airlock.Step == 0)
                    {
                        AirlockClearList.Add(airlock);
                    }
                }
                foreach (Airlock airlock in AirlockClearList)
                {
                        Airlocks.Remove(airlock);
                }
                AirlockClearList.Clear();
            }
        }
    }
}
