using Kopernicus.Components;
using System;
using System.Collections.Generic;

using SpaceTuxUtility;
using NearFutureSolar;
using Kopernicus; // needed for linux compile

namespace WarpDrive
{
    public class Utils
    {
        internal static double CalculateSolarPower(Vessel vessel)
        {
            double solarPower = 0;

            List<ModuleDeployableSolarPanel> solarPanels =
                vessel.FindPartModulesImplementing<ModuleDeployableSolarPanel>();

            for (int i = 0; i < solarPanels.Count; i++)
            {
                ModuleDeployableSolarPanel solarPanel = solarPanels[i];
                if (solarPanel.deployState != ModuleDeployableSolarPanel.DeployState.BROKEN &&
                    solarPanel.deployState != ModuleDeployableSolarPanel.DeployState.RETRACTED &&
                    solarPanel.deployState != ModuleDeployableSolarPanel.DeployState.RETRACTING)
                {
                    solarPower += solarPanel.flowRate;
                }
            }
            if (SpaceTuxUtility.HasMod.hasMod("Kopernicus"))
                solarPower += CalculateKopernicusSolarPower(vessel);
            if (SpaceTuxUtility.HasMod.hasMod("NearFutureSolar"))
                solarPower += CalculateNearFutureSolarPower(vessel);

            return solarPower;
        }

        internal static double CalculateKopernicusSolarPower(Vessel vessel)
        {
            double solarPower = 0;
            List<KopernicusSolarPanel> soKopernicusSolarPanellarPanels =
                vessel.FindPartModulesImplementing<KopernicusSolarPanel>();

            for (int i = 0; i < soKopernicusSolarPanellarPanels.Count; i++)
            {
                KopernicusSolarPanel solarPanel = soKopernicusSolarPanellarPanels[i];
                if (solarPanel.state != KopernicusSolarPanel.PanelState.Broken &&
                    solarPanel.state != KopernicusSolarPanel.PanelState.Retracted &&
                    solarPanel.state != KopernicusSolarPanel.PanelState.Retracting)
                {
                    solarPower += solarPanel.currentOutput;
                }
            }

            return solarPower;
        }
        internal static double CalculateNearFutureSolarPower(Vessel vessel)
        {
            double solarPower = 0;

            List<ModuleCurvedSolarPanel> CurvedSolarPanel =
            vessel.FindPartModulesImplementing<ModuleCurvedSolarPanel>();

            for (int i = 0; i < CurvedSolarPanel.Count; i++)
            {
                ModuleCurvedSolarPanel solarPanel = CurvedSolarPanel[i];
                if (solarPanel.State != ModuleDeployableSolarPanel.DeployState.BROKEN &&
                    solarPanel.State != ModuleDeployableSolarPanel.DeployState.RETRACTED &&
                    solarPanel.State != ModuleDeployableSolarPanel.DeployState.RETRACTING)
                {
                    solarPower += solarPanel.energyFlow;
                }
            }

            return solarPower;
        }


        internal static double CalculateOtherPower(Vessel vessel)
        {
            double otherPower = 0;
            List<ModuleGenerator> powerModules =
                vessel.FindPartModulesImplementing<ModuleGenerator>();

            for (int i = 0; i < powerModules.Count; i++)
            {
                // Find standard RTGs
                ModuleGenerator powerModule = powerModules[i];
                if (powerModule.generatorIsActive || powerModule.isAlwaysActive)
                {
                    for (int j = 0; j < powerModule.resHandler.outputResources.Count; ++j)
                    {
                        var resource = powerModule.resHandler.outputResources[j];
                        if (resource.name == "ElectricCharge")
                        {
                            otherPower += resource.rate * powerModule.efficiency;
                        }
                    }
                }
            }

            for (int i = 0; i < vessel.parts.Count; i++)
            {
                var part = vessel.parts[i];
                // Search for other generators
                PartModuleList modules = part.Modules;

                for (int j = 0; j < modules.Count; j++)
                {
                    var module = modules[j];

                    // Near future fission reactors
                    if (module.moduleName == "FissionGenerator")
                    {
                        otherPower += double.Parse(module.Fields.GetValue("CurrentGeneration").ToString());
                    }
                }

                // USI reactors
                ModuleResourceConverter converterModule = part.FindModuleImplementing<ModuleResourceConverter>();
                if (converterModule != null)
                {
                    if (converterModule.ModuleIsActive() && converterModule.ConverterName == "Reactor")
                    {
                        for (int j = 0; j < converterModule.outputList.Count; ++j)
                        {
                            var resource = converterModule.outputList[j];
                            if (resource.ResourceName == "ElectricCharge")
                            {
                                otherPower += resource.Ratio * converterModule.GetEfficiencyMultiplier();
                            }
                        }
                    }
                }
            }
            return otherPower;
        } // So many ifs.....

        public static bool hasTech(string techid)
        {
            if (String.IsNullOrEmpty(techid))
                return false;

            if (HighLogic.CurrentGame.Mode == Game.Modes.SANDBOX)
                return true;

            ProtoTechNode techstate = ResearchAndDevelopment.Instance.GetTechState(techid);
            if (techstate != null)
                return (techstate.state == RDTech.State.Available);
            else
                return false;
        }

        public static string Colorize(string text, UnityEngine.Color color, bool bold = false)
        {
            if (bold)
                return $"<b><color=#{UnityEngine.ColorUtility.ToHtmlStringRGB(color)}>{text}</color></b>";
            else
                return $"<color=#{UnityEngine.ColorUtility.ToHtmlStringRGB(color)}>{text}</color>";

        }


    }
}

