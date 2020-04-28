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
using System.IO;

namespace IngameScript
{
    partial class Program
    {
        public class SpeakerManager
        {
            Dictionary<WarningSource, DateTime> endTimes;
            Dictionary<WarningSource, bool> warningRunning;
            int loopTime;
            string soundName;
            IMyBlockGroup speakerGroup;
            List<IMySoundBlock> speakerList;
            DateTime? endLoop;

            public SpeakerManager()
            {
                speakerGroup = GridTerminalSystem.GetBlockGroupWithName(hangarSpeakerGroupName);
                if (speakerGroup == null)
                {
                    Echo("Cannot find speaker group: " + hangarSpeakerGroupName);
                    throw new ArgumentException("Cannot find speaker group: " + hangarSpeakerGroupName);
                }
                speakerList = new List<IMySoundBlock>();
                loopTime = soundLength;
                soundName = alarmName;
            }
            public void StartWarning(WarningSource source, int playTime = 0)
            {
                DateTime endTime;
                if (playTime > 0)
                {
                    endTime = DateTime.Now.AddSeconds(playTime); 
                }
                else
                {
                    endTime = DateTime.MaxValue;
                }
                endTimes[source] = endTime;
                warningRunning[source] = true;
                CheckState();
            }
            public void StopWarning(WarningSource source)
            {
                endTimes.Remove(source);
                warningRunning.Remove(source);
                CheckState();
            }
            public void CheckState()
            {
                foreach(KeyValuePair<WarningSource, DateTime> keyValuePair in endTimes)
                {
                    if(keyValuePair.Value <= DateTime.Now)
                    {
                        endTimes.Remove(keyValuePair.Key);
                        warningRunning.Remove(keyValuePair.Key);
                    }
                }
                bool playSound;
                if (warningRunning.Count > 0)
                {
                    playSound = true;
                }
                else
                {
                    playSound = false;
                }
                speakerList.ForEach((speaker) => setSound(speaker, playSound));
            }
            public void setSound(IMySoundBlock speaker, bool value)
            {
                speakerGroup.GetBlocksOfType<IMySoundBlock>(speakerList);
                speaker.SelectedSound = soundName;
                speaker.LoopPeriod = loopTime;
                if (value)
                {
                    if (endLoop == null)
                    {
                        ITerminalAction action;
                        action = speaker.GetActionWithName("OnOff_On");
                        action.Apply(speaker);
                        endLoop = DateTime.Now.AddSeconds(loopTime);
                        speaker.Play();
                    }
                    else
                    {
                        if (endLoop <= DateTime.Now )
                        {
                            ITerminalAction action;
                            action = speaker.GetActionWithName("OnOff_On");
                            action.Apply(speaker);
                            endLoop = DateTime.Now;
                            speaker.Play();
                        }
                    }
                }
                else
                {
                    endLoop = null;
                    speaker.Stop();
                }
            }

        }
    }
}
