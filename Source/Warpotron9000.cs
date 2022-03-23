using System;
using UnityEngine;
using KSP.UI.Screens;
using KSP.IO;
using KSP.Localization;
using System.Linq;


namespace WarpDrive
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class Warpotron9000 : MonoBehaviour
    {
        public static Warpotron9000 Instance;

        private PluginConfiguration config;
        private bool gamePaused;
        private StandAloneAlcubierreDrive masterDrive;

        // GUI stuff
        //private ApplicationLauncherButton appLauncherButton;
        //private IButton toolbarButton;
        private bool guiVisible;
        private bool maximized;
        private bool istimewarp;
        private bool refresh;
        private bool globalHidden;
        private Rect windowRect;
        private int guiId;
        //private bool useToolbar;
        private const ulong lockMask = 900719925474097919;

        public string ToolTip;
        public bool ToolTipActive;
        public string lastTooltip = " ";

        /// <summary>
        /// Kinda constructor
        /// </summary>
        public void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }

        /// <summary>
        /// Executed once after Awake
        /// </summary>
        public void Start()
        {
            guiVisible = false;
            globalHidden = false;
            gamePaused = false;
            refresh = true;

            guiId = GUIUtility.GetControlID(FocusType.Passive);
            config = PluginConfiguration.CreateForType<Warpotron9000>();
            config.load();

            windowRect = config.GetValue<Rect>("windowRect", new Rect(0, 0, 300, 400));
            //useToolbar = config.GetValue<bool>("useToolbar", false);
            maximized = config.GetValue<bool>("maximized", false);

            //GameEvents.onGUIApplicationLauncherReady.Add(onGUIApplicationLauncherReady);
            GameEvents.onLevelWasLoaded.Add(onLevelWasLoaded);
            GameEvents.onVesselChange.Add(onVesselChange);
            GameEvents.onHideUI.Add(onHideUI);
            GameEvents.onShowUI.Add(onShowUI);
            GameEvents.onGamePause.Add(onGamePause);
            GameEvents.onGameUnpause.Add(onGameUnpause);
        }

        /// <summary>
        /// Hail to The King, baby
        /// </summary>
        public void OnDestroy()
        {
            //GameEvents.onGUIApplicationLauncherReady.Remove(onGUIApplicationLauncherReady);
            GameEvents.onLevelWasLoaded.Remove(onLevelWasLoaded);
            GameEvents.onVesselChange.Remove(onVesselChange);
            GameEvents.onHideUI.Remove(onHideUI);
            GameEvents.onShowUI.Remove(onShowUI);
            GameEvents.onGamePause.Remove(onGamePause);
            GameEvents.onGameUnpause.Remove(onGameUnpause);

            UnlockControls();

            config.SetValue("windowRect", windowRect);
            //config.SetValue("useToolbar", useToolbar);
            config.SetValue("maximized", maximized);
            config.save();

            if (Instance == this)
                Instance = null;
        }

        private ControlTypes LockControls()
        {
            return InputLockManager.SetControlLock((ControlTypes)lockMask, this.name);
        }

        private void UnlockControls()
        {
            InputLockManager.RemoveControlLock(this.name);
        }

        //public void onGUIApplicationLauncherReady()
        //{
        //    CreateLauncher();
        //}

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
            Logging.LogDebug("onVesselChange");
            var drives = vessel.FindPartModulesImplementing<StandAloneAlcubierreDrive>();
            
            if (drives.Count != 0)
            {
                masterDrive = drives.Find(t => !t.isSlave);
            }
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

        internal const string MODID = "AppLaunch";
        internal const string MODNAME = "Warpotron9000";

#if false
        private void CreateLauncher()
        {
#if false
			if (ToolbarManager.ToolbarAvailable && useToolbar) {
				toolbarButton = ToolbarManager.Instance.add ("Warpotron9000", "AppLaunch");
				toolbarButton.TexturePath = "WarpDrive/Textures/warpdrive-icon-toolbar";
				toolbarButton.ToolTip = "Warpotron 9000";
				toolbarButton.Visible = true;
				toolbarButton.OnClick += (ClickEvent e) => {
					onToggle ();
				};
			} else if (appLauncherButton == null) {
				appLauncherButton = ApplicationLauncher.Instance.AddModApplication (
					onAppTrue,
					onAppFalse,
					null,
					null,
					null,
					null,
					ApplicationLauncher.AppScenes.FLIGHT |
					ApplicationLauncher.AppScenes.MAPVIEW,
					GameDatabase.Instance.GetTexture ("WarpDrive/Textures/warpdrive-icon", false)
				);
#endif
                if (toolbarControl == null)
                {
                    toolbarControl = gameObject.AddComponent<ToolbarControl>();
                    toolbarControl.AddToAllToolbars(onAppTrue, onAppFalse,
                        ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.MAPVIEW,
                        MODID,
                        "waButton",
                        "WarpDrive/Textures/warpdrive-icon",
                        "WarpDrive/Textures/warpdrive-icon"
                    );
                }
            
        }
#endif

        public void Update()
        {
            if (gamePaused)
                return;
        }

        public void OnGUI()
        {
            if (gamePaused || globalHidden || !guiVisible)
                return;

            if (refresh)
            {
                windowRect.height = 0;
                refresh = false;
            }

            windowRect = Layout.Window(
                guiId,
                windowRect,
                DrawGUI,
                Localizer.Format("#WD_Warpotron9000"),
                GUILayout.ExpandWidth(true),
                GUILayout.ExpandHeight(true)
            );

            

            if (windowRect.Contains(Event.current.mousePosition))
            {
                LockControls();
            }
            else
            {
                UnlockControls();
            }
        }

        public void DrawGUI(int guiId)
        {
            if (masterDrive == null)
            {
                guiVisible = false;
                return;
            }

            if (maximized)
            {
                if (Layout.Button("#WD_Minimize"))
                {
                    maximized = false;
                    refresh = true;
                }
            }
            else
            {
                if (Layout.Button("#WD_Maximize"))
                    maximized = true;
            }

            GUILayout.BeginVertical();
            if (maximized)
            {
                Layout.LabelAndText("#WD_currentGravityForce", "#WD_currentGravityForce_t",
                    masterDrive.currentGravityForce.ToString("N4") + Localizer.Format("#WD_Units_g"));
                Layout.LabelAndText("#WD_speedRestrictedbyG", "#WD_speedRestrictedbyG_t",
                    masterDrive.speedRestrictedbyG.ToString("N2") + Localizer.Format("#WD_Units_c"));
            }

            Layout.LabelAndText("#WD_currentSpeedFactor", "#WD_currentSpeedFactor_t",
                masterDrive.currentSpeedFactor.ToString("N2") + Localizer.Format("#WD_Units_c"));

            if (maximized)
            {
                Layout.LabelAndText("#WD_maximumSpeedFactor", "#WD_maximumSpeedFactor_t",
                masterDrive.maximumSpeedFactor.ToString("N2") + Localizer.Format("#WD_Units_c"));
            }

            Layout.LabelAndText("#WD_currentRequiredEM", "#WD_currentRequiredEM_t", masterDrive.requiredForCurrentFactor.ToString("N2"));
            Layout.LabelAndText("#WD_minimalRequiredEM", "#WD_minimalRequiredEM_t", masterDrive.minimalRequiredEM.ToString("N2"));

            if (maximized)
            {
                Layout.LabelAndText("#WD_maximumRequiredEM", "#WD_maximumRequiredEM_t", masterDrive.requiredForMaximumFactor.ToString("N2"));
                Layout.LabelAndText("#WD_drivesTotalPower", "#WD_drivesTotalPower_t",   masterDrive.drivesTotalPower.ToString("N1"));
                Layout.LabelAndText("#WD_containmentFieldPower", "#WD_containmentFieldPower_t", masterDrive.containmentFieldPowerMax.ToString("N1"));
                Layout.LabelAndText("#WD_vesselTotalMass", "#WD_vesselTotalMass_t", masterDrive.vesselTotalMass.ToString("N2") + Localizer.Format("#WD_Units_t"));
                Layout.LabelAndText("#WD_drivesEfficiency", "#WD_drivesEfficiency_t", masterDrive.drivesEfficiency.ToString("N2"));
                //	Layout.LabelAndText ("Magnitude Diff", masterDrive.magnitudeDiff.ToString ());
                //	Layout.LabelAndText ("Magnitude Change", masterDrive.magnitudeChange.ToString ());
            }

            string s = new string('>', masterDrive.lowEnergyFactor) + "1" +
            new string('<', masterDrive.warpFactors.Length - masterDrive.lowEnergyFactor - 1);

            if (masterDrive.maximumFactor >= masterDrive.currentFactor)
                s = s.Substring(0, masterDrive.currentFactor) +
                    Utils.Colorize(s.Substring(masterDrive.currentFactor, 1), Palette.green, bold: true) +
                    s.Substring(masterDrive.currentFactor + 1, masterDrive.maximumFactor - masterDrive.currentFactor) +
                    Utils.Colorize(s.Substring(masterDrive.maximumFactor + 1), Palette.gray50);
            else
                s = s.Substring(0, masterDrive.maximumFactor + 1) +
                    Utils.Colorize(s.Substring(masterDrive.maximumFactor + 1, masterDrive.currentFactor - masterDrive.maximumFactor - 1), Palette.gray50) +
                    Utils.Colorize(s.Substring(masterDrive.currentFactor, 1), Palette.red, bold: true) +
                    Utils.Colorize(s.Substring(masterDrive.currentFactor + 1), Palette.gray50);

            string s_tooltip =
                String.Join(" > ", masterDrive.warpFactors.Where(z => z < 1)) + " > 1 < " +
                String.Join(" < ", masterDrive.warpFactors.Where(z => z > 1));

            Layout.LabelCentered(s, s_tooltip, Palette.blue);


            // a button, that just play alarm sound for no reason
            // probably some unfinished feature
            //if (Layout.Button("alarm"))
            //    masterDrive.PlayAlarm();

            if (TimeWarp.CurrentRateIndex == 0)
            {
                if (istimewarp) refresh = true;
                istimewarp = false;

                GUILayout.BeginHorizontal();
                if (Layout.Button("#WD_DecreaseFactor", color: Palette.red))
                    masterDrive.DecreaseFactor();
                if (Layout.Button("#WD_IncreaseFactor", color: Palette.green))
                    masterDrive.IncreaseFactor();
                GUILayout.EndHorizontal();
                if (maximized)
                {
                    if (Layout.Button("#WD_ReduceFactor", "#WD_ReduceFactor_t", color: Palette.blue))
                        masterDrive.ReduceFactor();
                }
                if (!masterDrive.inWarp)
                {
                    if (Layout.Button("#WD_ActivateWarpDrive", color: Palette.green))
                        masterDrive.ActivateWarpDrive();
                }
                else if (Layout.Button("#WD_DeactivateWarpDrive", color: Palette.red))
                    masterDrive.DeactivateWarpDrive();

                if (!masterDrive.containmentField)
                {
                    if (Layout.Button("#WD_ActivateContainmentField", color: Palette.green))
                        masterDrive.StartContainment();
                }
                else if (Layout.Button("#WD_DeactivateContainmentField", color: Palette.red))
                    masterDrive.StopContainment();
            }
            else
            {
                if (!istimewarp) refresh = true;
                istimewarp = true;
            }

            if (Layout.Button("#WD_Close", color: Palette.red))
                onAppFalse();

            if (HighLogic.CurrentGame.Parameters.CustomParams<WarpDrive>().tooltip)
            {
                // on the next tick, read lastTooltip
                if (!String.IsNullOrWhiteSpace(lastTooltip))
                    Layout.Label(lastTooltip, color:Palette.blue);

                if (Event.current.type == EventType.Repaint && GUI.tooltip != lastTooltip)
                {
                    lastTooltip = GUI.tooltip;
                    refresh = true;
                }
            }

#if false
            if (appLauncherButton != null)
                    appLauncherButton.SetFalse();
                else
                    onToggle();

            if (Layout.Button("Switch Toolbar"))
            {
                useToolbar = !useToolbar;
                DestroyLauncher();
                CreateLauncher();
            }
#endif
            GUILayout.EndVertical();
            GUI.DragWindow();
        }
    }
}
