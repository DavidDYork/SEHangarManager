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
    partial class Program : HangarManager
    {
        const string welderToggleCommand = "Toggle Welder";
        const string welderOnCommand = "Welder On";
        const string welderOffCommand = "Welder Off";
        const string doorToggleCommand = "Toggle Doors";
        const string doorCloseCommand = "Close Doors";
        const string doorOpenCommand = "Open Doors";
        const string hangarSpeakerGroupName = "Hangar Speakers";
        const string hangarWelderGroupName = "Hangar Welders";
        const string hangarTopOffVentName = "Hangar Pressurizing Vent";
        const string hangarTankGroupName = "Hangar Oxygen Tanks";
        const string hangarDoorGroupName = "Hangar Doors";
        const string hangarVentGroupName = "Hangar Air Vents";
        const string alarmName = "Alert 1";
        const int soundLength = 30 * 60;  // Length of the sound loop in seconds
        const int warningPeriod = 10;     // Time to sound the warning before moving to the next state
        const double topOffThreshold = 0.95;
        SpeakerManager speakers;
        DoorManager doors;
        WelderManager welders;
        AirManager airTanks;

        enum WarningSource
        {
            DOOR,
            WELDER
        }
        
        /* Planned future features
        *   Projector controls
        *   Welding zones
        *   Entry Airlock control
        *   Read Custom Data of programming block to get Group Names
        *   Read Custom Data of control panels to get welder/projector group names
        *   Write to LCD status of zones, connectors, doors, air pressure, build progress
        */

        public Program()
        {
            // The constructor, called only once every session and
            // always before any other method is called. Use it to
            // initialize your script. 
            //     
            // The constructor is optional and can be removed if not
            // needed.
            // 
            // It's recommended to set Runtime.UpdateFrequency 
            // here, which will allow your script to run itself without a 
            // timer block.
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
            speakers = new SpeakerManager();
            airTanks = new AirManager();
            welders = new WelderManager();
            doors = new DoorManager(speakers, airTanks);

        }

        public void Save()
        {
            // Called when the program needs to save its state. Use
            // this method to save your state to the Storage field
            // or some other means. 
            // 
            // This method is optional and can be removed if not
            // needed.
        }

        public void Main(string argument, UpdateType updateSource)
        {
            // The main entry point of the script, invoked every time
            // one of the programmable block's Run actions are invoked,
            // or the script updates itself. The updateSource argument
            // describes where the update came from. Be aware that the
            // updateSource is a  bitfield  and might contain more than 
            // one update type.
            // 
            // The method itself is required, but the arguments above
            // can be removed if not needed.

            switch (argument)
            {
                case (doorToggleCommand):
                    if (currentDoorState == DoorStatus.Closed)
                    {
                        currentState = State.Warning;
                    }
                    else
                    {
                        startDoorMove = true;
                        currentState = State.Closing;
                    }
                    break;
                case (doorCloseCommand):
                    startDoorMove = true;
                    currentState = State.Closing;
                    break;
                case (doorOpenCommand):
                    currentState = State.Warning;
                    break;
                case (pressurizeBayCommand):
                    currentState = State.Pressurizing;
                    break;
                default:
                    break;
            }
        }
    }
}
