using KSP.Localization;
using System;
using System.Collections.Generic;

namespace WarpDrive
{
    internal static class CalculatePowerGeneration
    {
        public static double am_prod = 0; 

        internal static double VesselPowerGeneration(Vessel vessel)
        {
            am_prod = 0;

            try
            {
                if (vessel.parts == null)
                    return 0;
            }
            catch (NullReferenceException e)
            {
                if (e.Source != null)
                    return 0;
            }


            foreach (Part p in vessel.parts)
            {
                bool currentEngActive = false;

                if (p.Modules.Count < 1)
                    continue;
                bool kopernicusSolarPanelModule = false;

#if DEBUG
                UnityEngine.Debug.Log("VesselPowerGeneration, part: " + p.partName);
#endif
                foreach (PartModule tmpPM in p.Modules)
                {
                    switch (tmpPM.moduleName)
                    {
                        case "KopernicusSolarPanel":
                        case "weatherDrivenSolarPanel":
                            {
                                double.TryParse(tmpPM.Fields.GetValue("currentOutput").ToString(), out double results);
                                am_prod += results;
#if DEBUG
                                UnityEngine.Debug.Log("VesselPowerGeneration." + tmpPM.moduleName + ", flowRate: " + results);
#endif
                                kopernicusSolarPanelModule = true;
                            }
                            break;
                        case "ModuleDeployableSolarPanel":
                            if (!kopernicusSolarPanelModule)
                            {
                                ModuleDeployableSolarPanel tmpSol = (ModuleDeployableSolarPanel)tmpPM;
                                am_prod += tmpSol.flowRate;
#if DEBUG
                                UnityEngine.Debug.Log("VesselPowerGeneration." + tmpPM.moduleName + ", flowRate: " + tmpSol.flowRate);
#endif
                            }
                            break;
                        case "ModuleGenerator":
                            {
                                ModuleGenerator tmpGen = (ModuleGenerator)tmpPM;
                                foreach (ModuleResource outp in tmpGen.resHandler.outputResources)
                                {
                                    if (outp.name == "ElectricCharge")
                                    {
                                        if (tmpGen.isAlwaysActive || tmpGen.generatorIsActive)
                                        {
                                            am_prod += outp.rate * tmpGen.efficiency;
#if DEBUG
                                            UnityEngine.Debug.Log("VesselPowerGeneration." + tmpPM.moduleName + ", chargeRate: " + outp.rate);
#endif
                                        }
                                    }
                                }
                            }
                            break;
                        case "ModuleResourceConverter":
                        case "FissionReactor":
                            {
                                ModuleResourceConverter tmpGen = (ModuleResourceConverter)tmpPM;
                                foreach (ResourceRatio outp in tmpGen.outputList)
                                {
                                    if (outp.ResourceName == "ElectricCharge")
                                    {
                                        if (tmpGen.AlwaysActive || tmpGen.IsActivated)
                                        {
                                            am_prod += outp.Ratio; // might need efficiency in flight
#if DEBUG
                                            UnityEngine.Debug.Log("VesselPowerGeneration." + tmpPM.moduleName + ", chargeRate: " + outp.Ratio);
#endif
                                        }
                                    }
                                }
                            }
                            break;
                          
                        case "ModuleSystemHeatFissionReactor": // SystemHeat
                            {
                                double.TryParse(tmpPM.Fields.GetValue("MaxElectricalGeneration").ToString(), out double results);
                                am_prod += results;
#if DEBUG
                                UnityEngine.Debug.Log("VesselPowerGeneration." + tmpPM.moduleName + ", flowRate: " + results);
#endif

                            }

                            break;
                        case "ModuleResourceHarvester":
                            {
                                ModuleResourceHarvester tmpHar = (ModuleResourceHarvester)tmpPM;
                                foreach (ResourceRatio outp in tmpHar.outputList)
                                {
                                    if (outp.ResourceName == "ElectricCharge")
                                        if (tmpHar.AlwaysActive || tmpHar.IsActivated)
                                        {
                                            am_prod += outp.Ratio; // might need efficiency in flight
#if DEBUG
                                            UnityEngine.Debug.Log("VesselPowerGeneration." + tmpPM.moduleName + ", chargeRate: " + outp.Ratio);
#endif
                                        }

                                }
                            }
                            break;
                        case "ModuleEnginesFX":
                        case "ModuleEngines":
                            ModuleEngines tmpEng = (ModuleEngines)tmpPM;

                            currentEngActive = tmpEng.isOperational && (tmpEng.currentThrottle > 0);
                            break;
#if false
                            ModuleEnginesFX tmpEngFX = (ModuleEnginesFX)tmpPM;

                            currentEngActive = tmpEngFX.isOperational && (tmpEngFX.currentThrottle > 0);
                            break;
#endif
                        case "ModuleAlternator":
                            {
                                ModuleAlternator tmpAlt = (ModuleAlternator)tmpPM;
                                if (currentEngActive)
                                    foreach (ModuleResource r in tmpAlt.resHandler.outputResources)
                                    {
                                        if (r.name == "ElectricCharge")
                                        {
                                            am_prod += r.rate;
#if DEBUG
                                            UnityEngine.Debug.Log("VesselPowerGeneration." + tmpPM.moduleName + ", chargeRate: " + r.rate);
#endif
                                        }
                                    }
                            }
                            break;

                        case "FissionGenerator":
                            {
                                am_prod += double.Parse(tmpPM.Fields.GetValue("CurrentGeneration").ToString());
                            }
                            break;

                    }
                }
            }
            UnityEngine.Debug.Log("VesselPowerGeneration, total chargeRate: " + am_prod);

            return am_prod;
        }
    }
}