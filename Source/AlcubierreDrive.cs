using Expansions.Missions.Adjusters;
using KSP.Localization;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static WarpDrive.Logging;

namespace WarpDrive
{
	public class StandAloneAlcubierreDrive: PartModule, IModuleInfo
	{
		[KSPField(isPersistant = true)]
		internal bool inWarp;

		[KSPField(isPersistant = true)]
		internal int currentFactor = -1;

		[KSPField(isPersistant = true)]
		internal string serialisedWarpVector = "";

		[KSPField(isPersistant = true)]
		internal bool containmentField = false;

		[KSPField(isPersistant = true)]
		public double lastTime = 0;

		[KSPField(isPersistant = false)]
		public float innerRadius;

		[KSPField(isPersistant = false)]
		public float outerRadius;

		private PartResourceDefinition emResource;
		private PartResourceDefinition ecResource;

		double baseEMdecay = 100;
		double baseEMproduce = 0.01;
		double baseECconsume = 100;

		private List<StandAloneAlcubierreDrive> alcubierreDrives;
		private WarpFX fx;


		private bool alarm = false;
		internal int instanceId;
		internal bool isSlave;


		[KSPField(isPersistant = false, guiActive = true, guiName = "#WD_upgradeStatus", groupName = "WarpDrive", groupDisplayName = "WarpDrive")]
		public string upgradeStatus;
		[KSPField(isPersistant = false, guiActive = true, guiName = "#WD_drivePower", groupName = "WarpDrive", groupDisplayName = "WarpDrive")]
		public float drivePower;
		[KSPField(isPersistant = false, guiActive = true, guiName = "#WD_containmentFieldPower", groupName = "WarpDrive", groupDisplayName = "WarpDrive")]
		public float containmentFieldPower;
		

		[KSPField(isPersistant = false)]
		internal double currentGravityForce;
		[KSPField(isPersistant = false)]
		internal double speedRestrictedbyG;
		[KSPField(isPersistant = false)]
		internal double currentSpeedFactor;
		[KSPField(isPersistant = false)]
		internal double maximumSpeedFactor;
		[KSPField(isPersistant = false)]
		internal double minimalRequiredEM;
		[KSPField(isPersistant = false)]
		internal double requiredForCurrentFactor;
		[KSPField(isPersistant = false)]
		internal double requiredForMaximumFactor;
		[KSPField(isPersistant = false)]
		internal double drivesTotalPower;
		[KSPField(isPersistant = false)]
		internal double containmentFieldPowerMax;
		[KSPField(isPersistant = false)]
		internal double vesselTotalMass;
		[KSPField(isPersistant = false)]
		internal double drivesEfficiency;

		[KSPEvent(guiActive = true, guiName = "#WD_Warpotron9000", groupName = "WarpDrive", groupDisplayName = "WarpDrive")]
		public void ShowWarpotron9000()
		{
			Warpotron9000.Instance.onToggle();
		}

		private double magnitudeDiff;
		private double magnitudeChange;

		private Vector3d partHeading;
		private Vector3d warpVector;
		private Vector3d previousPartHeading;

		public double[] warpFactors = { 0.01, 0.016, 0.025, 0.04, 0.063, 0.1, 0.16, 0.25, 0.40, 0.63, 1.0, 1.6, 2.5, 4.0, 6.3, 10, 16, 25, 40, 63, 100, 160, 250, 400, 630, 1000 };
		
		public int lowEnergyFactor;
		public int maximumFactor;
		private int previousFactor;

		private bool vesselWasInOuterspace;
		private bool sceneLoaded;

		// Media stuff
		private AudioSource containmentSound;
		private AudioSource alarmSound;
		private AudioSource warpSound;

		public override void OnStart(PartModule.StartState state) {
			if (state == StartState.Editor)
				return;

			Init();
		}

		private void Init()
		{
			LogDebug("Init()");
			emResource = PartResourceLibrary.Instance.GetDefinition("ExoticMatter");
			ecResource = PartResourceLibrary.Instance.GetDefinition("ElectricCharge");

			instanceId = GetInstanceID();

			if (!isSlave)
			{
				alcubierreDrives = part.vessel.FindPartModulesImplementing<StandAloneAlcubierreDrive>();
				foreach (var drive in alcubierreDrives)
				{
					if (drive.GetInstanceID() != instanceId)
					{
						drive.isSlave = true;
					}
				}
			}
			else
			{
				LogDebug("Init(), the drive is slave, stop");
				return;
			}

			lowEnergyFactor = warpFactors.IndexOf(1.0f);

			if (currentFactor == -1)
				currentFactor = lowEnergyFactor;

			previousFactor = currentFactor;

			fx = new WarpFX(this);
			if (inWarp)
				fx.StartFX();

			if (serialisedWarpVector != null)
				warpVector = ConfigNode.ParseVector3D(serialisedWarpVector);

			if (lastTime == 0)
				lastTime = Planetarium.GetUniversalTime();

			// If we were somewhere else, let's compensate EM production
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

			LogDebug("Init() End");
		}

		public void onLevelWasLoaded(GameScenes gameScene) {
			sceneLoaded = true;
			GameEvents.onLevelWasLoaded.Remove (onLevelWasLoaded);
		}

		public override void OnUpdate() {
			if (isSlave || HighLogic.LoadedSceneIsEditor)
				return;

			if (inWarp)
				fx.FrameUpdate ();
			// Fuck this API
			if (alarm) {
				alarmSound.Play ();
				alarm = false;
			}
		}

		public void FixedUpdate() {
			if (isSlave || HighLogic.LoadedSceneIsEditor)
				return;

			containmentFieldPowerMax = alcubierreDrives.Max(z => z.containmentFieldPower);
			drivesTotalPower = alcubierreDrives.Sum(z => z.drivePower);
			vesselTotalMass = vessel.totalMass;
			drivesEfficiency = drivesTotalPower / vessel.totalMass;
			minimalRequiredEM = 100 * vessel.totalMass / drivesEfficiency;

			currentGravityForce = FlightGlobals.getGeeForceAtPosition(vessel.GetWorldPos3D()).magnitude;

			speedRestrictedbyG = vessel.mainBody.flightGlobalsIndex != 0
				? 1 / (Math.Max(currentGravityForce - 0.006, 0.001) * 10)
				: 1 / currentGravityForce;

			if (speedRestrictedbyG > warpFactors [warpFactors.Length - 1])
				speedRestrictedbyG = warpFactors [warpFactors.Length - 1];

			maximumFactor = GetMaximumFactor(speedRestrictedbyG);

			currentSpeedFactor = warpFactors[currentFactor];
			maximumSpeedFactor = warpFactors[maximumFactor];
		
			requiredForCurrentFactor = GetExoticMatterRequired(warpFactors[currentFactor]);
			requiredForMaximumFactor = GetExoticMatterRequired(warpFactors[maximumFactor]);

			// Fuck this API
			if (sceneLoaded)
				ProduceEM ();
			
			UpdateWarpSpeed();
		}

		public void CompensateEM() {
			double currentTime = Planetarium.GetUniversalTime ();
			double timeDelta = currentTime - lastTime;
			lastTime = currentTime;
			if (inWarp || !containmentField)
				return;

			vessel.RequestResource (part, emResource.id, -0.01 * timeDelta, true);
		}

		public void ProduceEM () {
			double currentTime = Planetarium.GetUniversalTime ();
			double timeDelta = currentTime - lastTime;
			lastTime = currentTime;
			if (inWarp)
				return;

			// Decay exotic matter
			if (!containmentField) {
				vessel.RequestResource (part, emResource.id, containmentFieldPowerMax * baseEMdecay * timeDelta, true);
				return;
			}

			double ecReturned = vessel.RequestResource (part, ecResource.id, containmentFieldPowerMax * baseECconsume * timeDelta, true);

			// No EC, shutdown containment field
			if (ecReturned == 0) {
				ScreenMessages.PostScreenMessage (Localizer.Format("#WD_ContainmentFieldNotEnoughECtoStable"), 7.0f);
				ScreenMessages.PostScreenMessage (Localizer.Format("#WD_ContainmentFieldOff"), 7.0f);
				StopContainment ();
			} else
				vessel.RequestResource (part, emResource.id, -1 * containmentFieldPowerMax * baseEMproduce * timeDelta, true);
		}

		public void UpdateWarpSpeed() {
			if (!inWarp || minimalRequiredEM <= 0) return;

			// Check this out
			if (this.vessel.altitude < this.vessel.mainBody.atmosphereDepth * 3) {
				if (vesselWasInOuterspace) {
					ScreenMessages.PostScreenMessage (Localizer.Format("#WD_AtmOutOfWarp"), 7.0f);
					alarm = true;
					DeactivateWarpDrive ();
					return;
				}
			}
			else
				vesselWasInOuterspace = true;


			// Check for heading changes
			Vector3d newPartHeading = new Vector3d(part.transform.up.x, part.transform.up.z, part.transform.up.y);
			magnitudeDiff = (partHeading - newPartHeading).magnitude;
			magnitudeChange = (previousPartHeading - newPartHeading).magnitude;
			previousPartHeading = newPartHeading;

			bool headingChanged = magnitudeDiff > 0.05 && magnitudeChange < 0.0001;
			bool factorChanged = previousFactor != currentFactor;
			bool gravityDisbalance = currentFactor > maximumFactor;

			if (gravityDisbalance) {
				currentFactor = maximumFactor;
				if (currentFactor < lowEnergyFactor) {
					ScreenMessages.PostScreenMessage (Localizer.Format("#WD_GravityOutOfWarp"), 7.0f);
					alarm = true;
					DeactivateWarpDrive ();
					return;
				}
				ScreenMessages.PostScreenMessage (Localizer.Format("#WD_GravitySpeedDroppedDown"), 7.0f);
			}

			if (gravityDisbalance || headingChanged || factorChanged) {
				previousFactor = currentFactor;

				vessel.GoOnRails ();
				vessel.orbit.UpdateFromStateVectors(vessel.orbit.pos, vessel.orbit.vel - warpVector, vessel.orbit.referenceBody, Planetarium.GetUniversalTime());

				warpVector = newPartHeading * Constants.C * warpFactors [currentFactor];
				partHeading = newPartHeading;
				serialisedWarpVector = ConfigNode.WriteVector(warpVector);

				vessel.orbit.UpdateFromStateVectors(vessel.orbit.pos, vessel.orbit.vel + warpVector, vessel.orbit.referenceBody, Planetarium.GetUniversalTime());
				vessel.GoOffRails ();
			}
		}

		public bool ExoticMatterDiff() {
			double previousLevel = GetExoticMatterRequired (warpFactors [previousFactor]);
			double currentLevel = GetExoticMatterRequired (warpFactors [currentFactor]);
			double emDiff = 0;

			if (currentLevel > previousLevel)
				emDiff = currentLevel - previousLevel;

			double availableExoticMatter;
			double maxExoticMatter;

			part.GetConnectedResourceTotals
			(
				emResource.id,
				out availableExoticMatter,
				out maxExoticMatter
			);

			if (availableExoticMatter < emDiff)
			{
				ScreenMessages.PostScreenMessage(Localizer.Format("#WD_WarpNotEnoughEMToChange"), 7.0f);
				return false;
			}
			if (emDiff > 0) {
				part.RequestResource ("ExoticMatter", emDiff);
				ScreenMessages.PostScreenMessage (Localizer.Format("#WD_EMConsumed", emDiff.ToString ("F2")), 7.0f);
			}
			return true;
		}

		public bool ActivateWarpDrive() {
			if (inWarp)
				return false;

			if (this.vessel.altitude <= getMaxAtmosphericAltitude(this.vessel.mainBody) &&
				this.vessel.mainBody.flightGlobalsIndex != 0)
			{
				ScreenMessages.PostScreenMessage (Localizer.Format("#WD_AtmCannotWarp"), 7.0f);
				return false;
			}

			if (drivesEfficiency < 1)
			{
				ScreenMessages.PostScreenMessage (Localizer.Format("#WD_WarpNotEnoughDrivesPower"), 7.0f);
				return false;
			}

			part.GetConnectedResourceTotals
			(
				emResource.id,
				out double availableExoticMatter,
				out double maxExoticMatter
			);

			if (availableExoticMatter < requiredForCurrentFactor)
			{
				ScreenMessages.PostScreenMessage(Localizer.Format("#WD_WarpNotEnoughEM"), 7.0f);
				return false;
			}

			InitiateWarp ();

			return true;
		}

		private void InitiateWarp() {
			part.RequestResource("ExoticMatter", requiredForCurrentFactor);

			partHeading = new Vector3d(part.transform.up.x, part.transform.up.z, part.transform.up.y);
			warpVector = partHeading * Constants.C * warpFactors [currentFactor];
			serialisedWarpVector = ConfigNode.WriteVector(warpVector);

			vesselWasInOuterspace = (this.vessel.altitude > this.vessel.mainBody.atmosphereDepth * 10);

			vessel.GoOnRails();
			vessel.orbit.UpdateFromStateVectors(vessel.orbit.pos, vessel.orbit.vel + warpVector, vessel.orbit.referenceBody, Planetarium.GetUniversalTime());
			vessel.GoOffRails();

			inWarp = true;
			previousPartHeading = partHeading;
			previousFactor = currentFactor;
			fx.StartFX ();
			containmentSound.Stop ();
			warpSound.Play ();
		}

		public bool DeactivateWarpDrive() {
			if (!inWarp)
				return false;

			vessel.GoOnRails();
			vessel.orbit.UpdateFromStateVectors(vessel.orbit.pos, vessel.orbit.vel - warpVector, vessel.orbit.referenceBody, Planetarium.GetUniversalTime());
			vessel.GoOffRails();
			inWarp = false;
			fx.StopFX ();
			warpSound.Stop ();
			if (containmentField)
				containmentSound.Play ();
			return true;
		}

		public bool StartContainment() {
			double ecProduction = Utils.CalculateSolarPower (vessel) + Utils.CalculateOtherPower (vessel);
			if (ecProduction < containmentFieldPowerMax * 100) {
				ScreenMessages.PostScreenMessage (
					Localizer.Format("#WD_ContainmentFieldNotEnoughECtoCreate", (containmentFieldPowerMax * 100).ToString("F0")), 
					7.0f);
				return false;
			}
			containmentField = true;
			lastTime = Planetarium.GetUniversalTime ();
			containmentSound.Play ();
			containmentSound.loop = true;

			return true;
		}

		public bool StopContainment() {
			if (containmentField == false)
				return false;

			containmentField = false;
			containmentSound.Stop ();
			ScreenMessages.PostScreenMessage (Localizer.Format("#WD_ContainmentFieldOff"), 7.0f);
			return true;
		}

		public void ReduceFactor()
		{
			if (currentFactor == lowEnergyFactor) return;

			if (currentFactor < lowEnergyFactor)
				IncreaseFactor();
			else if (currentFactor > lowEnergyFactor)
				DecreaseFactor();
		}

		public void IncreaseFactor()
		{
			previousFactor = currentFactor;			

			currentFactor++;
			if (currentFactor > maximumFactor)
				currentFactor = maximumFactor;

			if (currentFactor >= warpFactors.Length)
				currentFactor = warpFactors.Length - 1;

			if (inWarp)
			if (!ExoticMatterDiff ())
				currentFactor = previousFactor;
		}

		public void DecreaseFactor()
		{
			previousFactor = currentFactor;			

			currentFactor--;
			if (currentFactor < 0)
				currentFactor = 0;

			if (inWarp)
			if (!ExoticMatterDiff ())
				currentFactor = previousFactor;
		}

		private int GetMaximumFactor(double speed)
		{
			for (int i = warpFactors.Length-1; i > 0; i--)
			{
				if (warpFactors[i] <= speed)
					return i;
			}
			return 0;
		}

		private double GetExoticMatterRequired(double warpFactor)
		{
			var sqrtSpeed = Math.Sqrt(warpFactor);
			var powerModifier = warpFactor < 1 ? 1 / sqrtSpeed : sqrtSpeed;
			return powerModifier * minimalRequiredEM;
		}

		public static double getMaxAtmosphericAltitude(CelestialBody body)
		{
			if (!body.atmosphere) return 0;
			return body.atmosphereDepth;
		}

		//internal void PlayAlarm() {
		//	alarmSound.Play ();
		//}

		private void LoadMedia() {
			// Sounds
			containmentSound = gameObject.AddComponent<AudioSource> ();
			containmentSound.clip = GameDatabase.Instance.GetAudioClip ("WarpDrive/Sounds/containment");
			containmentSound.volume = GameSettings.SHIP_VOLUME;
			containmentSound.panStereo = 0;
			containmentSound.Stop ();

			warpSound = gameObject.AddComponent<AudioSource> ();
			warpSound.clip = GameDatabase.Instance.GetAudioClip ("WarpDrive/Sounds/warp");
			warpSound.volume = GameSettings.SHIP_VOLUME;
			warpSound.panStereo = 0;
			warpSound.loop = true;
			warpSound.Stop ();

			alarmSound = gameObject.AddComponent<AudioSource> ();
			alarmSound.clip = GameDatabase.Instance.GetAudioClip ("WarpDrive/Sounds/alarm");
			alarmSound.volume = GameSettings.SHIP_VOLUME;
			alarmSound.panStereo = 0;
			alarmSound.loop = false;
			alarmSound.Stop ();
		}

		public override string GetInfo() =>
			Localizer.Format("#WD_Info") +
			Localizer.Format("#WD_InfoStatus", upgradeStatus) +
			Localizer.Format("#WD_InfoDrivePower", drivePower) +
			Localizer.Format("#WD_InfoContainmentFieldPower", containmentFieldPower) +
			Localizer.Format("#WD_InfoContainmentField", containmentFieldPower * baseECconsume, containmentFieldPower * baseEMproduce);

		public override string GetModuleDisplayName() => Localizer.Format("#WD_InfoDisplayName");

		/// <summary>
		/// Return a string title for your module.
		/// </summary>
		/// <returns></returns>
		public string GetModuleTitle() => Localizer.Format("#WD_InfoDisplayName");

		/// <summary>
		/// Return a method delegate to draw a custom panel, or null if not necessary.
		/// </summary>
		/// <returns></returns>
		public Callback<UnityEngine.Rect> GetDrawModulePanelCallback() => null;

		/// <summary>
		/// Return a string to be displayed in the main 
		/// information box on the tooltip, 
		/// or null if nothing is that important to be up there.
		/// </summary>
		/// <returns></returns>
		public string GetPrimaryField() => null;

	}
}
