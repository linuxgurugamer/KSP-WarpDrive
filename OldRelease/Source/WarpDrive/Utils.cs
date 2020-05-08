using System.Collections.Generic;
using UnityEngine;

namespace WarpDrive
{
	public class Utils
	{
		internal static double CalculateSolarPower(Vessel vessel)
		{
			double num = 0.0;
			List<ModuleDeployableSolarPanel> list = vessel.FindPartModulesImplementing<ModuleDeployableSolarPanel>();
			for (int i = 0; i < list.Count; i++)
			{
				ModuleDeployableSolarPanel moduleDeployableSolarPanel = list[i];
				if (moduleDeployableSolarPanel.deployState != ModuleDeployablePart.DeployState.BROKEN && moduleDeployableSolarPanel.deployState != 0 && moduleDeployableSolarPanel.deployState != ModuleDeployablePart.DeployState.RETRACTING)
				{
					num += (double)moduleDeployableSolarPanel.flowRate;
				}
			}
			return num;
		}

		internal static double CalculateOtherPower(Vessel vessel)
		{
			double num = 0.0;
			List<ModuleGenerator> list = vessel.FindPartModulesImplementing<ModuleGenerator>();
			for (int i = 0; i < list.Count; i++)
			{
				ModuleGenerator moduleGenerator = list[i];
				if (moduleGenerator.generatorIsActive || moduleGenerator.isAlwaysActive)
				{
					for (int j = 0; j < moduleGenerator.resHandler.outputResources.Count; j++)
					{
						ModuleResource moduleResource = moduleGenerator.resHandler.outputResources[j];
						if (moduleResource.name == "ElectricCharge")
						{
							num += moduleResource.rate * (double)moduleGenerator.efficiency;
						}
					}
				}
			}
			for (int k = 0; k < vessel.parts.Count; k++)
			{
				Part part = vessel.parts[k];
				PartModuleList modules = part.Modules;
				for (int l = 0; l < modules.Count; l++)
				{
					PartModule partModule = modules[l];
					if (partModule.moduleName == "FissionGenerator")
					{
						num += double.Parse(partModule.Fields.GetValue("CurrentGeneration").ToString());
					}
				}
				ModuleResourceConverter moduleResourceConverter = part.FindModuleImplementing<ModuleResourceConverter>();
				if ((Object)moduleResourceConverter != (Object)null && moduleResourceConverter.ModuleIsActive() && moduleResourceConverter.ConverterName == "Reactor")
				{
					for (int m = 0; m < moduleResourceConverter.outputList.Count; m++)
					{
						ResourceRatio resourceRatio = moduleResourceConverter.outputList[m];
						if (resourceRatio.ResourceName == "ElectricCharge")
						{
							num += resourceRatio.Ratio * moduleResourceConverter.GetEfficiencyMultiplier();
						}
					}
				}
			}
			return num;
		}

		public static bool hasTech(string techid)
		{
			if (string.IsNullOrEmpty(techid))
			{
				return false;
			}
			if (HighLogic.CurrentGame.Mode == Game.Modes.SANDBOX)
			{
				return true;
			}
			ProtoTechNode techState = ResearchAndDevelopment.Instance.GetTechState(techid);
			if (techState != null)
			{
				return techState.state == RDTech.State.Available;
			}
			return false;
		}
	}
}
