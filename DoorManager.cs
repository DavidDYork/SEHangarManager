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
        public class DoorManager
        {
            int warningTime;
            IMyBlockGroup hangarDoors;
            List<IMyAirtightHangarDoor> doorList;

            bool startDoorMove = false;
            bool startPressurize = false;
            bool startDepressurize = false;

            State currentState = State.Idle;
            DoorStatus currentDoorState;
            SpeakerManager speakers;
            AirManager airTanks;

            [Flags] enum State
            {
                Idle            = 0,
                Warning         = 1,
                Depressurizing  = 2,
                Opening         = 4,
                Closing         = 8
            }
            public DoorManager(SpeakerManager inSpeakers, AirManager inAirTanks)
            {
                hangarDoors = GridTerminalSystem.GetBlockGroupWithName(hangarDoorGroupName);
                if (hangarDoors == null)
                {
                    Echo("Hangar Doors group not found");
                    throw new ArgumentException("Cannot find Door group: " + hangarDoorGroupName);
                }
                doorList = new List<IMyAirtightHangarDoor>();
                speakers = inSpeakers;
                airTanks = inAirTanks;
            }
            public void CloseDoors()
            {
                if(currentState == State.Depressurizing)
                {
                    airTanks.PressurizeBay();
                }
                else if (currentState != State.Warning)
                {
                    startDoorMove = true;
                }
                else if (currentState == State.Warning)
                {
                    speakers.StopWarning(WarningSource.DOOR);
                }
                currentState = State.Closing;
                CheckState();
            }
            public void OpenDoors()
            {
                if (currentState == State.Idle)
                {
                    currentState = State.Warning;
                }
                else if (currentState == State.Closing)
                {
                    currentState = State.Opening;
                    startDoorMove = true;
                }
                CheckState();
            }
            public void ToggleDoors()
            {
                if (currentState.HasFlag(State.Warning | State.Depressurizing | State.Opening) || ((currentState == State.Idle) && CheckDoorState(DoorStatus.Open)))
                {
                    CloseDoors();
                }
                else if (currentState.HasFlag(State.Closing) || ((currentState == State.Idle) && CheckDoorState(DoorStatus.Closed)))
                {
                    OpenDoors();
                }
            }
                
            public void CheckState()
            {
                bool processCompleted = false;
                switch (currentState)
                {
                    case State.Warning:
                        Echo("Warning");
                        speakers.StartWarning(WarningSource.DOOR, warningTime);
                        break;
                    case State.Depressurizing:
                        Echo("Depressurizing");
                        airTanks.DepressurizeBay(out processCompleted);
                        if (processCompleted)
                        {
                            currentState = State.Opening;
                            startDoorMove = true;
                        }
                        break;
                    case State.Opening:
                        Echo("Opening Doors");
                        ChangeBayDoors(DoorStatus.Open);
                        break;
                    case State.Closing:
                        Echo("Closing Doors");
                        startDoorMove = true;
                        break;
                    default:
                        break;
                }
            }

            public void ChangeBayDoors(DoorStatus state)
            {
                string actionString;
                if (state == DoorStatus.Closed)
                {
                    actionString = "Open_Off";
                }
                else if (state == DoorStatus.Open)
                {
                    actionString = "Open_On";
                }
                else
                {
                    Echo("Invalid End Door State");
                    return;
                }

                hangarDoors.GetBlocksOfType<IMyAirtightHangarDoor>(doorList);

                if (startDoorMove)
                {
                    ITerminalAction action;
                    action = doorList.First().GetActionWithName(actionString);
                    doorList.ForEach((door) => action.Apply(door));
                    startDoorMove = false;
                }

                if (CheckDoorState(state))
                {
                    currentState = State.Idle;
                    currentDoorState = state;
                    Echo("Finished");
                }
            }
            public bool CheckDoorState(DoorStatus desiredState)
            {
                bool returnState = true;
                foreach (IMyAirtightHangarDoor door in doorList)
                {
                    if (door.Status != desiredState)
                    {
                        returnState = false;
                    }
                }
                return returnState
            }
        }
    }
}
