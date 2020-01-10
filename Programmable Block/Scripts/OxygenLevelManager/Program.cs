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


namespace IngameScript {
    partial class Program : MyGridProgram {
        // USAGE
        //-------
        // This Script will keep the oxygen level in the connected tanks between 
        // the minimum and maximum thresholds. It will also assist in pressurization
        // by looking at the connected vents. If any of the vents are set differently
        // than the current pressurization state, this script will set all vents to
        // the desired state and enable the tank to help with the operation.


        // CONFIGURATION
        //---------------
        const string DISPLAY_TAG = "[o2]"; // The tag that identifies panels to be used 

        const string O2_SYSTEM_TAG = "[o2]"; // Tag to indicate Tanks, Vents and OxyGenerators relevant to this script 
        const string LIGHT_TAG = "[o2]"; //Tag identifying the interior light(s) to use as an O2 tank pressure indicator

        const int TANK_O2_MIN = 60; // pressure level at which to turn on 02 generation (The minimum value you want to have in your tank at all times)
        const int TANK_O2_MAX = 80; // pressure at which to turn off 02 generation (This needs to be equal to or greater than TANK_O2_MIN and below 100% if you want depressurisation to work)
        //-------------------
        // END CONFIGURATION

        Color greenColor = new Color(0, 175, 0);
        Color redColor = new Color(175, 0, 0);

        private List<IMyInteriorLight> lights;
        private List<IMyAirVent> vents;
        private List<IMyGasTank> tanks;
        private List<IMyGasGenerator> h2o2generators;
        //private List<IMyDoor> airlocks;
        private List<IMyTextPanel> panels;
        private PanelManager panelManager;
        private StringBuilder text;

        public Program() {
            try { 

                Runtime.UpdateFrequency = UpdateFrequency.Update10;

                lights = new List<IMyInteriorLight>();
                vents = new List<IMyAirVent>();
                tanks = new List<IMyGasTank>();
                h2o2generators = new List<IMyGasGenerator>();
                panels = new List<IMyTextPanel>();

                text = new StringBuilder();

                GridTerminalSystem.GetBlocksOfType(lights, light => light.IsSameConstructAs(Me) && light.CustomName.Contains(LIGHT_TAG));
                GridTerminalSystem.GetBlocksOfType(vents, vent => vent.IsSameConstructAs(Me) && vent.CustomName.Contains(O2_SYSTEM_TAG));
                GridTerminalSystem.GetBlocksOfType(tanks, tank => tank.IsSameConstructAs(Me) && tank.CustomName.Contains(O2_SYSTEM_TAG));
                GridTerminalSystem.GetBlocksOfType(h2o2generators, generator => generator.IsSameConstructAs(Me) && generator.CustomName.Contains(O2_SYSTEM_TAG));
                GridTerminalSystem.GetBlocksOfType(panels, panel => panel.IsSameConstructAs(Me) && panel.CustomName.Contains(DISPLAY_TAG));

                panelManager = new PanelManager(panels);
            

                SetOxygenProduction(false);
                SetLightColor(redColor);
                panelManager.SetStatus(true);
                panelManager.SetBackColor(new Color(10, 10, 30));
                panelManager.SetFontColor(new Color(255, 125, 0));
                panelManager.SetFontSize(1.5f);
                panelManager.SetTitle("[O2] Life Support Systems", false);
                panelManager.SetText("Initializing...", false);
            }
            catch (Exception e) {
                // Dump the exception content to the 
                Echo("An error occurred during script execution.");
                Echo($"Exception: {e}\n---");

                // Rethrow the exception to make the programmable block halt execution properly
                throw;
            }
        }



        public void Main(string argument, UpdateType updateSource) {
            try { 
                bool shouldOpenTanks = true;
                bool shouldRunGenerators = true;

                //todo: change O2 to o2
                //TODO: Add the h2o2 generator and turn off when in the middle fo pressurizing or depressurizing





                // Tank fill level
                //------------------
                // Tank fill will override the vent status
                double fillLevel = CheckTankStatus();
                if (fillLevel < TANK_O2_MIN / 100.0) {
                    shouldOpenTanks = true;
                } else if (fillLevel > TANK_O2_MAX / 100.0) {
                    shouldOpenTanks = false;
                }


                // Pressurization systems
                //------------------------
                //make all of vents the same (allows changing just one vent)
                SetVents(DesiredVentStatusIsPressurize());
                VentStatus ventStatus = GetVentStatus(vents[0]);
                //When pressurizing or depressurizing, force the generators off and the tanks open
                switch (ventStatus) {                  
                    case VentStatus.Depressurizing:
                        shouldOpenTanks = true;
                        shouldRunGenerators = false;
                        break;
                    case VentStatus.Pressurizing:
                        shouldOpenTanks = true;
                        shouldRunGenerators = false;
                        break;
                }

                // Lights
                //--------
                if (ventStatus == VentStatus.Pressurized) {
                    SetLightColor(greenColor);
                } else {
                    SetLightColor(redColor);
                }
            

                // Set the O2 tanks and generators
                //---------------------------------
                SetOxygenProduction(shouldOpenTanks);
                SetGenerators(shouldRunGenerators);



                // Display info
                //--------------
                int tankPercentage = (int)(CheckTankStatus() * 100);
                string o2Production = "Off";
                if (shouldOpenTanks)
                    o2Production = "On";

                string pressurized = "---";
                if (ventStatus == VentStatus.Pressurized)
                    pressurized = "YES";
                else if (ventStatus == VentStatus.Pressurizing)
                    pressurized = "PRESSURIZING";
                else if (ventStatus == VentStatus.Depressurized)
                    pressurized = "NO";
                else if (ventStatus == VentStatus.Depressurizing)
                    pressurized = "DEPRESSURIZING";

                string generators = "Off";
                if (shouldRunGenerators)
                    generators = "On";


                text.Clear();
                text.Append("Life Support System Status\n\n");

                text.Append("Tank level: " + tankPercentage + "%\n");
                text.Append("O2 Production: " + o2Production + "\n");
                text.Append("Pressurized: " + pressurized + "\n");
                text.Append("Generators: " + generators + "\n");


                panelManager.SetText(text.ToString(), false);
            } 
            catch (Exception e) {
                // Dump the exception content to the 
                Echo("An error occurred during script execution.");
                Echo($"Exception: {e}\n---");

                // Rethrow the exception to make the programmable block halt execution properly
                throw;
            }
        }
        
        public void SetLightColor(Color color) {
            foreach (IMyInteriorLight light in lights) {
                light.Color = color;
            }           
        }

        private void SetOxygenProduction(bool enabled) {
            foreach (IMyGasTank tank in tanks) {
                tank.Enabled = enabled;
            }
        }

        private void SetGenerators(bool enabled) {
            foreach(IMyGasGenerator generator in h2o2generators) {
                generator.Enabled = enabled;
            }
        }

        private double CheckTankStatus() {
            if (tanks.Count() == 0)
                return 0;

            double total = 0;
            foreach (IMyGasTank tank in tanks) {
                total += tank.FilledRatio;
            }
            double average = total / (double)tanks.Count();
            return average;
        }
        
        private void SetVents(bool pressurize) {
            foreach (IMyAirVent vent in vents) {
                vent.Depressurize = !pressurize;
            }
        }

        //If one vent is changed, all of the others will follow suit.
        //If all of the vents are the same, it should stay that way
        private bool DesiredVentStatusIsPressurize() {
            int pressurize = 0;
            int depressurize = 0;
            foreach (IMyAirVent vent in vents) {
                if (vent.Depressurize == true) {
                    depressurize++;                    
                } else {
                    pressurize++;
                }
            }
            if (pressurize == 0 || depressurize == 0) {
                return pressurize >= depressurize;
            } else {
                return pressurize <= depressurize;
            }
        }

        private VentStatus GetVentStatus(IMyAirVent vent) {
            if (vent.Status != VentStatus.Depressurizing) // don't trust depressurizing. It could be depressurized
                return vent.Status;

            if (vent.GetOxygenLevel() < 0.01)
                return VentStatus.Depressurized;
            else
                return VentStatus.Depressurizing;
        }
    }
}


