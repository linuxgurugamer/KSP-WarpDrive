using System;
using System.Collections.Generic;
using UnityEngine;

namespace WarpDrive
{
	public class StandAloneAlcubierreDrive : PartModule
	{
		[KSPField(isPersistant = true)]
		internal bool inWarp;

		[KSPField(isPersistant = true, guiActive = true, guiName = "Upgraded")]
		internal bool isUpgraded;

		[KSPField(isPersistant = true)]
		internal int currentFactor = -1;

		[KSPField(isPersistant = true)]
		internal string serialisedWarpVector = "";

		[KSPField(isPersistant = true)]
		internal bool containmentField;

		[KSPField(isPersistant = true)]
		public double lastTime;

		[KSPField(isPersistant = false)]
		public float innerRadius;

		[KSPField(isPersistant = false)]
		public float outerRadius;

		[KSPField(isPersistant = false)]
		public string upgradeTechReq;

		[KSPField(isPersistant = true)]
		public bool launched;

		private PartResourceDefinition emResource;

		private PartResourceDefinition ecResource;

		private List<StandAloneAlcubierreDrive> alcubierreDrives;

		private WarpFX fx;

		private bool alarm;

		internal int instanceId;

		internal bool isSlave;

		internal double gravityPull;

		internal double speedLimit;

		internal double drivesTotalPower;

		internal double drivesEfficiencyRatio;

		internal double minimumRequiredExoticMatter;

		internal double requiredForCurrentFactor;

		internal double requiredForMaximumFactor;

		private double magnitudeDiff;

		private double magnitudeChange;

		private Vector3d partHeading;

		private Vector3d warpVector;

		private Vector3d previousPartHeading;

		private double[] warpFactors = new double[26]
		{
			0.01,
			0.016,
			0.025,
			0.04,
			0.063,
			0.1,
			0.16,
			0.25,
			0.4,
			0.63,
			1.0,
			1.6,
			2.5,
			4.0,
			6.3,
			10.0,
			16.0,
			25.0,
			40.0,
			63.0,
			100.0,
			160.0,
			250.0,
			400.0,
			630.0,
			1000.0
		};

		private int lowEnergyFactor;

		private int maximumFactor;

		private int previousFactor;

		private bool vesselWasInOuterspace;

		private bool sceneLoaded;

		private AudioSource containmentSound;

		private AudioSource alarmSound;

		private AudioSource warpSound;

		internal double SelectedSpeed => warpFactors[currentFactor];

		internal double MaxAllowedSpeed => warpFactors[maximumFactor];

		public override void OnStart(StartState state)
		{
			if (state != StartState.Editor)
			{
				emResource = PartResourceLibrary.Instance.GetDefinition("ExoticMatter");
				ecResource = PartResourceLibrary.Instance.GetDefinition("ElectricCharge");
				instanceId = base.GetInstanceID();
				if (!launched)
				{
					if (Utils.hasTech(upgradeTechReq))
					{
						isUpgraded = true;
					}
					launched = true;
				}
				lowEnergyFactor = warpFactors.IndexOf(1.0);
				if (currentFactor == -1)
				{
					currentFactor = lowEnergyFactor;
				}
				previousFactor = currentFactor;
				if (!isSlave)
				{
					alcubierreDrives = base.part.vessel.FindPartModulesImplementing<StandAloneAlcubierreDrive>();
					foreach (StandAloneAlcubierreDrive alcubierreDrife in alcubierreDrives)
					{
						if (alcubierreDrife.GetInstanceID() != instanceId)
						{
							alcubierreDrife.isSlave = true;
						}
					}
				}
				if (!isSlave)
				{
					fx = new WarpFX(this);
					if (inWarp)
					{
						fx.StartFX();
					}
					if (serialisedWarpVector != null)
					{
						warpVector = ConfigNode.ParseVector3D(serialisedWarpVector);
					}
					if (lastTime == 0.0)
					{
						lastTime = Planetarium.GetUniversalTime();
					}
					CompensateEM();
					sceneLoaded = false;
					GameEvents.onLevelWasLoaded.Add(onLevelWasLoaded);
					LoadMedia();
					if (containmentField && !inWarp)
					{
						containmentSound.Play();
						containmentSound.loop = true;
					}
					if (inWarp)
					{
						warpSound.Play();
					}
				}
			}
		}

		public void onLevelWasLoaded(GameScenes gameScene)
		{
			sceneLoaded = true;
			GameEvents.onLevelWasLoaded.Remove(onLevelWasLoaded);
		}

		public override void OnUpdate()
		{
			if (!isSlave && !HighLogic.LoadedSceneIsEditor)
			{
				if (inWarp)
				{
					fx.FrameUpdate();
				}
				if (alarm)
				{
					alarmSound.Play();
					alarm = false;
				}
			}
		}

		public void FixedUpdate()
		{
			if (!isSlave && !HighLogic.LoadedSceneIsEditor)
			{
				gravityPull = FlightGlobals.getGeeForceAtPosition(base.vessel.GetWorldPos3D()).magnitude;
				speedLimit = ((base.vessel.mainBody.flightGlobalsIndex != 0) ? (1.0 / (Math.Max(gravityPull - 0.006, 0.001) * 10.0)) : (1.0 / gravityPull));
				if (speedLimit > warpFactors[warpFactors.Length - 1])
				{
					speedLimit = warpFactors[warpFactors.Length - 1];
				}
				maximumFactor = GetMaximumFactor(speedLimit);
				drivesTotalPower = 0.0;
				for (int i = 0; i < alcubierreDrives.Count; i++)
				{
					drivesTotalPower += (double)(alcubierreDrives[i].part.mass * (float)(alcubierreDrives[i].isUpgraded ? 20 : 10));
				}
				drivesEfficiencyRatio = drivesTotalPower / base.vessel.totalMass;
				minimumRequiredExoticMatter = 100.0 * base.vessel.totalMass / drivesEfficiencyRatio;
				requiredForCurrentFactor = GetExoticMatterRequired(warpFactors[currentFactor]);
				requiredForMaximumFactor = GetExoticMatterRequired(warpFactors[maximumFactor]);
				if (sceneLoaded)
				{
					ProduceEM();
				}
				UpdateWarpSpeed();
			}
		}

		public void CompensateEM()
		{
			double universalTime = Planetarium.GetUniversalTime();
			double num = universalTime - lastTime;
			lastTime = universalTime;
			if (!inWarp && containmentField)
			{
				base.vessel.RequestResource(base.part, emResource.id, -0.01 * num, true);
			}
		}

		public void ProduceEM()
		{
			double universalTime = Planetarium.GetUniversalTime();
			double num = universalTime - lastTime;
			lastTime = universalTime;
			if (!inWarp)
			{
				if (!containmentField)
				{
					base.vessel.RequestResource(base.part, emResource.id, 100.0 * num, true);
				}
				else if (base.vessel.RequestResource(base.part, ecResource.id, 100.0 * num, true) == 0.0)
				{
					ScreenMessages.PostScreenMessage("Not enough EC for stable containment field!", 7f);
					ScreenMessages.PostScreenMessage("Containment field is off, EM will decay!", 7f);
					StopContainment();
				}
				else
				{
					base.vessel.RequestResource(base.part, emResource.id, -0.01 * num, true);
				}
			}
		}

		public void UpdateWarpSpeed()
		{
			if (inWarp && !(minimumRequiredExoticMatter <= 0.0))
			{
				if (base.vessel.altitude < base.vessel.mainBody.atmosphereDepth * 3.0)
				{
					if (vesselWasInOuterspace)
					{
						ScreenMessages.PostScreenMessage("Atmosphere is too close! Dropping out of warp!", 7f);
						alarm = true;
						DeactivateWarpDrive();
						return;
					}
				}
				else
				{
					vesselWasInOuterspace = true;
				}
				Vector3d vector3d = new Vector3d((double)base.part.transform.up.x, (double)base.part.transform.up.z, (double)base.part.transform.up.y);
				Vector3d vector3d2 = partHeading - vector3d;
				magnitudeDiff = vector3d2.magnitude;
				vector3d2 = previousPartHeading - vector3d;
				magnitudeChange = vector3d2.magnitude;
				previousPartHeading = vector3d;
				bool flag = magnitudeDiff > 0.05 && magnitudeChange < 0.0001;
				bool flag2 = previousFactor != currentFactor;
				bool flag3 = currentFactor > maximumFactor;
				if (flag3)
				{
					currentFactor = maximumFactor;
					if (currentFactor < lowEnergyFactor)
					{
						ScreenMessages.PostScreenMessage("Gravity too strong, dropping out of warp!", 7f);
						alarm = true;
						DeactivateWarpDrive();
						return;
					}
					ScreenMessages.PostScreenMessage("Gravity pull increased, speed dropped down!", 7f);
				}
				if (flag3 | flag | flag2)
				{
					previousFactor = currentFactor;
					base.vessel.GoOnRails();
					base.vessel.orbit.UpdateFromStateVectors(base.vessel.orbit.pos, base.vessel.orbit.vel - warpVector, base.vessel.orbit.referenceBody, Planetarium.GetUniversalTime());
					warpVector = vector3d * 29979245.8 * warpFactors[currentFactor];
					partHeading = vector3d;
					serialisedWarpVector = ConfigNode.WriteVector(warpVector);
					base.vessel.orbit.UpdateFromStateVectors(base.vessel.orbit.pos, base.vessel.orbit.vel + warpVector, base.vessel.orbit.referenceBody, Planetarium.GetUniversalTime());
					base.vessel.GoOffRails();
				}
			}
		}

		public bool ExoticMatterDiff()
		{
			double exoticMatterRequired = GetExoticMatterRequired(warpFactors[previousFactor]);
			double exoticMatterRequired2 = GetExoticMatterRequired(warpFactors[currentFactor]);
			double num = 0.0;
			if (exoticMatterRequired2 > exoticMatterRequired)
			{
				num = exoticMatterRequired2 - exoticMatterRequired;
			}
			base.part.GetConnectedResourceTotals(emResource.id, out double num2, out double _, true);
			if (num2 < num)
			{
				ScreenMessages.PostScreenMessage("Not enough Exotic Matter to change warp factor!", 7f);
				return false;
			}
			if (num > 0.0)
			{
				base.part.RequestResource("ExoticMatter", num);
				ScreenMessages.PostScreenMessage(num.ToString("F3") + " Exotic Matter consumed!", 7f);
			}
			return true;
		}

		public void ActivateWarpDrive()
		{
			if (!inWarp)
			{
				if (base.vessel.altitude <= getMaxAtmosphericAltitude(base.vessel.mainBody) && base.vessel.mainBody.flightGlobalsIndex != 0)
				{
					ScreenMessages.PostScreenMessage("Cannot activate warp drive within the atmosphere!", 7f);
				}
				else if (drivesEfficiencyRatio < 1.0)
				{
					ScreenMessages.PostScreenMessage("Not enough drives power to initiate warp!", 7f);
				}
				else
				{
					base.part.GetConnectedResourceTotals(emResource.id, out double num, out double _, true);
					if (num < requiredForCurrentFactor)
					{
						ScreenMessages.PostScreenMessage("Not enough Exotic Matter to initiate warp!", 7f);
					}
					else
					{
						InitiateWarp();
					}
				}
			}
		}

		private void InitiateWarp()
		{
			base.part.RequestResource("ExoticMatter", requiredForCurrentFactor);
			partHeading = new Vector3d((double)base.part.transform.up.x, (double)base.part.transform.up.z, (double)base.part.transform.up.y);
			warpVector = partHeading * 29979245.8 * warpFactors[currentFactor];
			serialisedWarpVector = ConfigNode.WriteVector(warpVector);
			vesselWasInOuterspace = (base.vessel.altitude > base.vessel.mainBody.atmosphereDepth * 10.0);
			base.vessel.GoOnRails();
			base.vessel.orbit.UpdateFromStateVectors(base.vessel.orbit.pos, base.vessel.orbit.vel + warpVector, base.vessel.orbit.referenceBody, Planetarium.GetUniversalTime());
			base.vessel.GoOffRails();
			inWarp = true;
			previousPartHeading = partHeading;
			previousFactor = currentFactor;
			fx.StartFX();
			containmentSound.Stop();
			warpSound.Play();
		}

		public void DeactivateWarpDrive()
		{
			if (inWarp)
			{
				base.vessel.GoOnRails();
				base.vessel.orbit.UpdateFromStateVectors(base.vessel.orbit.pos, base.vessel.orbit.vel - warpVector, base.vessel.orbit.referenceBody, Planetarium.GetUniversalTime());
				base.vessel.GoOffRails();
				inWarp = false;
				fx.StopFX();
				warpSound.Stop();
				if (containmentField)
				{
					containmentSound.Play();
				}
			}
		}

		public void StartContainment()
		{
			if (Utils.CalculateSolarPower(base.vessel) + Utils.CalculateOtherPower(base.vessel) < 100.0)
			{
				ScreenMessages.PostScreenMessage("Not enough power to create stable containment field!", 7f);
			}
			else
			{
				containmentField = true;
				lastTime = Planetarium.GetUniversalTime();
				containmentSound.Play();
				containmentSound.loop = true;
			}
		}

		public void StopContainment()
		{
			containmentField = false;
			containmentSound.Stop();
			ScreenMessages.PostScreenMessage("Containment field is off, EM will decay!", 7f);
		}

		public void ReduceFactor()
		{
			if (currentFactor != lowEnergyFactor)
			{
				if (currentFactor < lowEnergyFactor)
				{
					IncreaseFactor();
				}
				else if (currentFactor > lowEnergyFactor)
				{
					DecreaseFactor();
				}
			}
		}

		public void IncreaseFactor()
		{
			previousFactor = currentFactor;
			currentFactor++;
			if (currentFactor > maximumFactor)
			{
				currentFactor = maximumFactor;
			}
			if (currentFactor >= warpFactors.Length)
			{
				currentFactor = warpFactors.Length - 1;
			}
			if (inWarp && !ExoticMatterDiff())
			{
				currentFactor = previousFactor;
			}
		}

		public void DecreaseFactor()
		{
			previousFactor = currentFactor;
			currentFactor--;
			if (currentFactor < 0)
			{
				currentFactor = 0;
			}
			if (inWarp && !ExoticMatterDiff())
			{
				currentFactor = previousFactor;
			}
		}

		private int GetMaximumFactor(double speed)
		{
			int result = 0;
			for (int i = 0; i < warpFactors.Length; i++)
			{
				if (warpFactors[i] >= speed)
				{
					return result;
				}
				result = i;
			}
			return result;
		}

		private double GetExoticMatterRequired(double warpFactor)
		{
			double num = Math.Sqrt(warpFactor);
			return ((warpFactor < 1.0) ? (1.0 / num) : num) * minimumRequiredExoticMatter;
		}

		public static double getMaxAtmosphericAltitude(CelestialBody body)
		{
			if (!body.atmosphere)
			{
				return 0.0;
			}
			return body.atmosphereDepth;
		}

		internal void PlayAlarm()
		{
			alarmSound.Play();
		}

		private void LoadMedia()
		{
			containmentSound = base.gameObject.AddComponent<AudioSource>();
			containmentSound.clip = GameDatabase.Instance.GetAudioClip("WarpDrive/Sounds/containment");
			containmentSound.volume = GameSettings.SHIP_VOLUME;
			containmentSound.panStereo = 0f;
			containmentSound.Stop();
			warpSound = base.gameObject.AddComponent<AudioSource>();
			warpSound.clip = GameDatabase.Instance.GetAudioClip("WarpDrive/Sounds/warp");
			warpSound.volume = GameSettings.SHIP_VOLUME;
			warpSound.panStereo = 0f;
			warpSound.loop = true;
			warpSound.Stop();
			alarmSound = base.gameObject.AddComponent<AudioSource>();
			alarmSound.clip = GameDatabase.Instance.GetAudioClip("WarpDrive/Sounds/alarm");
			alarmSound.volume = GameSettings.SHIP_VOLUME;
			alarmSound.panStereo = 0f;
			alarmSound.loop = false;
			alarmSound.Stop();
		}
	}
}
