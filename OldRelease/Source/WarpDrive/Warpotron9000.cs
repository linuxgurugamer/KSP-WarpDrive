using KSP.IO;
using KSP.UI.Screens;
using UnityEngine;

namespace WarpDrive
{
	[KSPAddon(KSPAddon.Startup.Flight, false)]
	public class Warpotron9000 : MonoBehaviour
	{
		public static Warpotron9000 Instance;

		private PluginConfiguration config;

		private bool gamePaused;

		private AlcubierreDrive masterDrive;

		private ApplicationLauncherButton appLauncherButton;

		private IButton toolbarButton;

		private bool guiVisible;

		private bool maximized;

		private bool refresh;

		private bool globalHidden;

		private Rect windowRect;

		private int guiId;

		private bool useToolbar;

		private const ulong lockMask = 900719925474097919uL;

		public void Awake()
		{
			if ((Object)Instance != (Object)null)
			{
				Object.Destroy(this);
			}
			else
			{
				Instance = this;
			}
		}

		public void Start()
		{
			guiVisible = false;
			globalHidden = false;
			gamePaused = false;
			refresh = true;
			guiId = GUIUtility.GetControlID(FocusType.Passive);
			config = PluginConfiguration.CreateForType<Warpotron9000>(null);
			config.load();
			windowRect = config.GetValue("windowRect", new Rect(0f, 0f, 300f, 400f));
			useToolbar = config.GetValue("useToolbar", false);
			maximized = config.GetValue("maximized", false);
			GameEvents.onGUIApplicationLauncherReady.Add(onGUIApplicationLauncherReady);
			GameEvents.onLevelWasLoaded.Add(onLevelWasLoaded);
			GameEvents.onVesselChange.Add(onVesselChange);
			GameEvents.onHideUI.Add(onHideUI);
			GameEvents.onShowUI.Add(onShowUI);
			GameEvents.onGamePause.Add(onGamePause);
			GameEvents.onGameUnpause.Add(onGameUnpause);
		}

		public void OnDestroy()
		{
			GameEvents.onGUIApplicationLauncherReady.Remove(onGUIApplicationLauncherReady);
			GameEvents.onLevelWasLoaded.Remove(onLevelWasLoaded);
			GameEvents.onVesselChange.Remove(onVesselChange);
			GameEvents.onHideUI.Remove(onHideUI);
			GameEvents.onShowUI.Remove(onShowUI);
			GameEvents.onGamePause.Remove(onGamePause);
			GameEvents.onGameUnpause.Remove(onGameUnpause);
			UnlockControls();
			DestroyLauncher();
			config.SetValue("windowRect", windowRect);
			config.SetValue("useToolbar", useToolbar);
			config.SetValue("maximized", maximized);
			config.save();
			if ((Object)Instance == (Object)this)
			{
				Instance = null;
			}
		}

		private ControlTypes LockControls()
		{
			return InputLockManager.SetControlLock(ControlTypes.ALLBUTCAMERAS, base.name);
		}

		private void UnlockControls()
		{
			InputLockManager.RemoveControlLock(base.name);
		}

		public void onGUIApplicationLauncherReady()
		{
			CreateLauncher();
		}

		public void onLevelWasLoaded(GameScenes scene)
		{
			onVesselChange(FlightGlobals.ActiveVessel);
		}

		public void onGamePause()
		{
			gamePaused = true;
			UnlockControls();
		}

		public void onGameUnpause()
		{
			gamePaused = false;
		}

		private void onHideUI()
		{
			globalHidden = true;
			UnlockControls();
		}

		private void onShowUI()
		{
			globalHidden = false;
		}

		public void onVesselChange(Vessel vessel)
		{
			masterDrive = vessel.FindPartModulesImplementing<AlcubierreDrive>().Find((AlcubierreDrive t) => !t.isSlave);
		}

		public void onAppTrue()
		{
			guiVisible = true;
		}

		public void onAppFalse()
		{
			guiVisible = false;
			UnlockControls();
		}

		public void onToggle()
		{
			guiVisible = !guiVisible;
			if (!guiVisible)
			{
				UnlockControls();
			}
		}

		private void CreateLauncher()
		{
			if (ToolbarManager.ToolbarAvailable && useToolbar)
			{
				toolbarButton = ToolbarManager.Instance.add("Warpotron9000", "AppLaunch");
				toolbarButton.TexturePath = "WarpDrive/Textures/warpdrive-icon-toolbar";
				toolbarButton.ToolTip = "Warpotron 9000";
				toolbarButton.Visible = true;
				toolbarButton.OnClick += delegate
				{
					onToggle();
				};
			}
			else if ((Object)appLauncherButton == (Object)null)
			{
				appLauncherButton = ApplicationLauncher.Instance.AddModApplication(onAppTrue, onAppFalse, null, null, null, null, ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.MAPVIEW, GameDatabase.Instance.GetTexture("WarpDrive/Textures/warpdrive-icon", false));
			}
		}

		private void DestroyLauncher()
		{
			if ((Object)appLauncherButton != (Object)null)
			{
				ApplicationLauncher.Instance.RemoveModApplication(appLauncherButton);
				appLauncherButton = null;
			}
			if (toolbarButton != null)
			{
				toolbarButton.Destroy();
				toolbarButton = null;
			}
		}

		public void Update()
		{
			bool gamePaused2 = gamePaused;
		}

		public void OnGUI()
		{
			if (!gamePaused && !globalHidden && guiVisible)
			{
				if (refresh)
				{
					windowRect.height = 0f;
					refresh = false;
				}
				windowRect = Layout.Window(guiId, windowRect, DrawGUI, "Warpotron 9000", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
				if (windowRect.Contains(Event.current.mousePosition))
				{
					LockControls();
				}
				else
				{
					UnlockControls();
				}
			}
		}

		public void DrawGUI(int guiId)
		{
			if ((Object)masterDrive == (Object)null)
			{
				guiVisible = false;
			}
			else
			{
				GUILayout.BeginVertical();
				Layout.LabelAndText("Upgrade Status", masterDrive.isUpgraded ? "Butterfly" : "Snail");
				Layout.LabelAndText("Current Gravity Force", masterDrive.gravityPull.ToString("F3") + " g");
				Layout.LabelAndText("Speed Restricted by G", masterDrive.speedLimit.ToString("F3") + " C");
				double num = masterDrive.SelectedSpeed;
				Layout.LabelAndText("Current Speed Factor", num.ToString("F3") + " C");
				num = masterDrive.MaxAllowedSpeed;
				Layout.LabelAndText("Maximum Speed Factor", num.ToString("F3") + " C");
				if (maximized)
				{
					if (Layout.Button("Minimize"))
					{
						maximized = false;
						refresh = true;
					}
					Layout.LabelAndText("Minimal Required EM", masterDrive.minimumRequiredExoticMatter.ToString("F3"));
					Layout.LabelAndText("Current Required EM", masterDrive.requiredForCurrentFactor.ToString("F3"));
					Layout.LabelAndText("Maximum Required EM", masterDrive.requiredForMaximumFactor.ToString("F3"));
					Layout.LabelAndText("Current Drives Power", masterDrive.drivesTotalPower.ToString("F3"));
					Layout.LabelAndText("Vessel Total Mass", masterDrive.vessel.totalMass.ToString("F3") + " tons");
					Layout.LabelAndText("Drives Efficiency", masterDrive.drivesEfficiencyRatio.ToString("F3"));
				}
				else if (Layout.Button("Maximize"))
				{
					maximized = true;
				}
				if (Layout.Button("alarm"))
				{
					masterDrive.PlayAlarm();
				}
				if (TimeWarp.CurrentRateIndex == 0)
				{
					GUILayout.BeginHorizontal();
					if (Layout.Button("Decrease Factor", Palette.red, GUILayout.Width(141f)))
					{
						masterDrive.DecreaseFactor();
					}
					if (Layout.Button("Increase Factor", Palette.green, GUILayout.Width(141f)))
					{
						masterDrive.IncreaseFactor();
					}
					GUILayout.EndHorizontal();
					if (Layout.Button("Reduce Factor", Palette.blue))
					{
						masterDrive.ReduceFactor();
					}
					if (!masterDrive.inWarp)
					{
						if (Layout.Button("Activate Warp Drive", Palette.green))
						{
							masterDrive.ActivateWarpDrive();
						}
					}
					else if (Layout.Button("Deactivate Warp Drive", Palette.red))
					{
						masterDrive.DeactivateWarpDrive();
					}
					if (!masterDrive.containmentField)
					{
						if (Layout.Button("Activate Containment Field", Palette.green))
						{
							masterDrive.StartContainment();
						}
					}
					else if (Layout.Button("Deactivate Containment Field", Palette.red))
					{
						masterDrive.StopContainment();
					}
				}
				if (Layout.Button("Close", Palette.red))
				{
					if ((Object)appLauncherButton != (Object)null)
					{
						appLauncherButton.SetFalse(true);
					}
					else
					{
						onToggle();
					}
				}
				if (Layout.Button("Switch Toolbar"))
				{
					useToolbar = !useToolbar;
					DestroyLauncher();
					CreateLauncher();
				}
				GUILayout.EndVertical();
				GUI.DragWindow();
			}
		}
	}
}
