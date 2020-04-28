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
        public class AirManager
        {
            IMyBlockGroup hangarVents;
            IMyBlockGroup hangarTanks;
            IMyAirVent topOffVent;
            List<IMyAirVent> hangarVentList;
            List<IMyOxygenTank> hangarTankList;

            public AirManage()
            {
                hangarVents = GridTerminalSystem.GetBlockGroupWithName(hangarVentGroupName);
                topOffVent = GridTerminalSystem.GetBlockWithName(hangarTopOffVentName) as IMyAirVent;
                hangarTanks = GridTerminalSystem.GetBlockGroupWithName(hangarTankGroupName);

                if (hangarVents == null)
                {
                    Echo("Hangar Vents group not found");
                    throw new ArgumentException("Cannot find Vent group: " + hangarVentGroupName);
                }
                if (topOffVent == null)
                {
                    Echo("Top off vent could not be found");
                    throw new ArgumentException("Cannot find Top Off Vent: " + hangarTopOffVentName);
                }
                if (hangarTanks == null)
                {
                    Echo("Hangar Oxygen Tank group could not be found");
                    throw new ArgumentException("Cannot find Tank group: " + hangarTankGroupName);
                }
            }

            public void PressurizeBay()
            {

            }
            public void DepressurizeBay(out bool depressurizeComplete)
            {
                //public void depressurizeBay(IMyBlockGroup hangarVents, IMyBlockGroup hangarTanks)
                //{
                    double tankFillLevel = getHangarAirTankLevel(hangarTanks, false);
                    double roomOxygenLevel = getHangarOxygenLevel(hangarVents, true);

                    if (startDepressurize)
                    {
                        List<IMyAirVent> ventList = new List<IMyAirVent>();
                        hangarVents.GetBlocksOfType<IMyAirVent>(ventList);
                        ventList.ForEach((vent) => pressurizeBay(vent, false));
                        startDepressurize = false;
                    }

                    if ((roomOxygenLevel == 0) || (tankFillLevel == 1))
                    {
                        depressurizeComplete = true;
                    }
                //}

            }
            public void TopOffBay()
            {

            }
            public void PressurizeBay(out bool pressurizeComplete)
            {
                double tankFillLevel = getHangarAirTankLevel(hangarTanks, true);
                double roomOxygenLevel = getHangarOxygenLevel(hangarVents, false);

                if (startPressurize)
                {
                    List<IMyAirVent> ventList = new List<IMyAirVent>();
                    hangarVents.GetBlocksOfType<IMyAirVent>(ventList);
                    ventList.ForEach((vent) => pressurizeBay(vent, true));
                    startPressurize = false;
                }

                if (roomOxygenLevel > topOffThreshold)
                {
                    pressurizeComplete = true;
                    if (topOffVent.IsWorking)
                    {
                        topOffBay(topOffVent);
                    }
                }
                else if ((tankFillLevel == 0) && roomOxygenLevel <= topOffThreshold)
                {
                    topOffBay(topOffVent);
                }
            }

            public void pressurizeBay(IMyAirVent vent, bool pressurized)
            {
                vent.Depressurize = !pressurized;
            }

            public void topOffBay(IMyAirVent vent)
            {
                if (!vent.CanPressurize)
                {
                    Echo("Hangar is not airtight");
                    return;
                }

                ITerminalAction action;
                if (vent.GetOxygenLevel() <= topOffThreshold)
                {
                    action = vent.GetActionWithName("OnOff_On");
                    vent.Depressurize = false;
                }
                else
                {
                    action = vent.GetActionWithName("OnOff_Off");
                }
                action.Apply(vent);
            }

            public double getHangarAirTankLevel(IMyBlockGroup hangarTankGroup, bool max)
            {
                List<double> fillValues = new List<double>();
                List<IMyGasTank> tankList = new List<IMyGasTank>();
                hangarTankGroup.GetBlocksOfType<IMyGasTank>(tankList);
                tankList.ForEach((tank) => fillValues.Add(tank.FilledRatio));
                if (max)
                {
                    return fillValues.Max();
                }
                else
                {
                    return fillValues.Min();
                }
            }

            public double getHangarOxygenLevel(IMyBlockGroup hangarVentGroup, bool max)
            {
                List<double> fillValues = new List<double>();
                List<IMyAirVent> ventList = new List<IMyAirVent>();
                hangarVentGroup.GetBlocksOfType<IMyAirVent>(ventList);
                ventList.ForEach((vent) => fillValues.Add(vent.GetOxygenLevel()));
                if (max)
                {
                    return fillValues.Max();
                }
                else
                {
                    return fillValues.Min();
                }
            }
            pub

        }
    }
}
