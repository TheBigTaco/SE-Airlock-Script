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
    partial class Program
    {
        public class Airlock
        {
            // Arguments
            public string DoorSide;
            public string AirlockName;
            // List of doors on grid for this script
            public List<IMyDoor> Doors = new List<IMyDoor>();
            // List of AirVents on grid for this script
            public List<IMyAirVent> AirVents = new List<IMyAirVent>();
            // List of AirVents on grid for this script
            public List<IMyLightingBlock> Lights = new List<IMyLightingBlock>();
            // List of SoundBlocks on grid for this script
            public List<IMySoundBlock> SoundBlocks = new List<IMySoundBlock>();
            // Current Airlock Doors and AirVents
            public List<IMyDoor> OpenDoors = new List<IMyDoor>();
            public List<IMyDoor> ClosedDoors = new List<IMyDoor>();
            public List<IMyLightingBlock> AirlockLights = new List<IMyLightingBlock>();
            public List<IMyAirVent> ThisAirlockAirVents = new List<IMyAirVent>();
            public List<IMySoundBlock> AirlockSoundBlocks = new List<IMySoundBlock>();
            // Which step it is currently on
            public int Step = 1;
            // Count of current check
            public int Count = 0;
            // Closed and Open and Active colors
            Color ready = new Color(0, 255, 0);
            Color active = new Color(255, 0, 0);

            // Flip state of airlock
            public void AirlockSwitch()
            {
                if (Step == 1)
                {
                    foreach (IMyDoor door in OpenDoors)
                    {
                        door.Enabled = true;
                        door.CloseDoor();
                    }
                    foreach(IMyLightingBlock light in AirlockLights)
                    {
                        light.Color = active;
                        light.Intensity = 10;
                        light.Radius = 2;
                        light.Falloff = 0;
                        light.BlinkLength = 50;
                        light.BlinkIntervalSeconds = 1;
                    }
                    foreach(IMySoundBlock sb in AirlockSoundBlocks)
                    {
                        sb.LoopPeriod = 120;
                        sb.Play();
                    }
                    Step++;
                }
                if (Step == 2)
                {
                    foreach (IMyDoor door in OpenDoors)
                    {
                        if (door.OpenRatio == 0)
                        {
                            door.Enabled = false;
                            Count++;
                        }
                    }
                    if (Count == OpenDoors.Count())
                    {
                        Step++;
                        Count = 0;
                        return;
                    }
                    else
                    {
                        Count = 0;
                    }
                }
                if (Step == 3)
                {
                    foreach (IMyAirVent vent in ThisAirlockAirVents)
                    {
                        if (vent.CanPressurize)
                        {
                            if (DoorSide == "Inside")
                            {
                                vent.Depressurize = true;
                            }
                            else if (DoorSide == "Outside")
                            {
                                vent.Depressurize = false;
                            }
                        }
                    }
                    Step++;
                }
                if (Step == 4)
                {
                    foreach (IMyAirVent vent in ThisAirlockAirVents)
                    {
                        if (DoorSide == "Inside")
                        {
                            if (vent.GetOxygenLevel() == 0)
                            {
                                Count++;
                            }
                        }
                        else if (DoorSide == "Outside")
                        {
                            if (vent.GetOxygenLevel() == 1)
                            {
                                Count++;
                            }
                        }
                    }
                    if (Count == ThisAirlockAirVents.Count())
                    {
                        Step++;
                        Count = 0;
                        return;
                    }
                    else
                    {
                        Count = 0;
                    }
                }
                if (Step == 5)
                {
                    foreach (IMyDoor door in ClosedDoors)
                    {
                        door.Enabled = true;
                        door.OpenDoor();
                    }
                    Step++;
                }
                if (Step == 6)
                {
                    foreach (IMyDoor door in ClosedDoors)
                    {
                        if (door.OpenRatio == 1)
                        {
                            door.Enabled = false;
                            Count++;
                        }
                    }
                    if (Count == ClosedDoors.Count())
                    {
                        Step = 0;
                        Count = 0;
                        foreach (IMyLightingBlock light in AirlockLights)
                        {
                            light.Color = ready;
                            light.BlinkLength = 0;
                            light.BlinkIntervalSeconds = 0;
                        }
                        foreach (IMySoundBlock sb in AirlockSoundBlocks)
                        {
                            sb.Stop();
                        }
                        return;
                    }
                    else
                    {
                        Count = 0;
                    }
                }
            }
        }
    }
}
