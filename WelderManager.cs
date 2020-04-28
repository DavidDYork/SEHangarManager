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
        public class WelderManager
        {
            DateTime? alarmStarted;
            DateTime? commandIssued;
            bool lastWeldersOn = false;

            public Program()
            {
                // The constructor, called only once every session and
                // always before any other method is called. Use it to
                // initialize your script. 
                //     
                // The constructor is optional and can be removed if not
                // needed.
                // 
                // It's recommended to set RuntimeInfo.UpdateFrequency 
                // here, which will allow your script to run itself without a 
                // timer block.
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
                IMyBlockGroup hangarSpeakers = GridTerminalSystem.GetBlockGroupWithName(hangarSpeakerGroupName);
                IMyBlockGroup hangarWelders = GridTerminalSystem.GetBlockGroupWithName(hangarWelderGroupName);
                List<IMyFunctionalBlock> welderList = new List<IMyFunctionalBlock>();
                List<IMySoundBlock> speakerList = new List<IMySoundBlock>();
                bool activeWelder = false;

                if (hangarSpeakers == null)
                {
                    Echo("Cannot find Speaker group");
                    return;
                }
                if (hangarWelders == null)
                {
                    Echo("Cannot find Welder group");
                    return;
                }
                hangarWelders.GetBlocksOfType<IMyFunctionalBlock>(welderList);
                hangarSpeakers.GetBlocksOfType<IMySoundBlock>(speakerList);

                if (argument == command && commandIssued != null)   // Turn off Welders
                {
                    welderList.ForEach((welder) => setWelder(welder, false));
                    commandIssued = null;
                }
                else if (argument == command)                                      // Start the warning
                {
                    commandIssued = DateTime.Now;
                    lastWeldersOn = true;
                }
                else if (commandIssued != null)
                {
                    TimeSpan delta = DateTime.Now.Subtract(commandIssued.Value);   // If the warning time has elapsed, start the welders
                    if (delta.Seconds >= warningPeriod)
                    {
                        welderList.ForEach((welder) => setWelder(welder, true));

                    }
                    activeWelder = true;
                }
                else                                                                                    // Check for active welders
                {
                    welderList.ForEach((welder) => activeWelder |= welder.IsWorking);
                }
                if (lastWeldersOn || activeWelder)
                {
                    speakerList.ForEach((speaker) => setSound(speaker, activeWelder));
                }
                lastWeldersOn = activeWelder;
            }

            public void setSound(IMySoundBlock speaker, bool value)
            {
                speaker.SelectedSound = "Alert 1";
                speaker.LoopPeriod = soundLength;
                if (value)
                {
                    if (alarmStarted == null)
                    {
                        ITerminalAction action;
                        action = speaker.GetActionWithName("OnOff_On");
                        action.Apply(speaker);
                        alarmStarted = DateTime.Now;
                        speaker.Play();
                    }
                    else
                    {
                        TimeSpan delta = DateTime.Now.Subtract(alarmStarted.Value);   // If the warning time has elapsed, start the welders
                        if (delta.TotalSeconds > (soundLength - .9))
                        {
                            ITerminalAction action;
                            action = speaker.GetActionWithName("OnOff_On");
                            action.Apply(speaker);
                            alarmStarted = DateTime.Now;
                            speaker.Play();
                        }
                    }
                }
                else
                {
                    alarmStarted = null;
                    speaker.Stop();
                }
            }

            public void setWelder(IMyFunctionalBlock welder, bool value)
            {
                ITerminalAction action;
                if (value)
                {
                    action = welder.GetActionWithName("OnOff_On");
                }
                else
                {
                    action = welder.GetActionWithName("OnOff_Off");
                }
            }
        }
    }
}
