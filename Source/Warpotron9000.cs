using System;
using System.Collections.Generic;
using UnityEngine;
using KSP.UI.Screens;
using KSP.IO;
using ToolbarControl_NS;

namespace WarpDrive
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class RegisterToolbar : MonoBehaviour
    {
        void Start()
        {
            ToolbarControl.RegisterMod(Warpotron9000.MODID, Warpotron9000.MODNAME);
        }
    }

    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class Warpotron9000 : MonoBehaviour
    {
        public static Warpotron9000 Instance;

        private PluginConfiguration config;
        private bool gamePaused;
        private StandAloneAlcubierreDrive masterDrive;

        // GUI stuff
        ToolbarControl toolbarControl;
        //private ApplicationLauncherButton appLauncherButton;
        //private IButton toolbarButton;
        private bool guiVisible;
        private bool maximized;
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

            GameEvents.onGUIApplicationLauncherReady.Add(onGUIApplicationLauncherReady);
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
            masterDrive = vessel.FindPartModulesImplementing<StandAloneAlcubierreDrive>().Find(t => !t.isSlave);
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

                if (FlightGlobals.ActiveVessel.FindPartModulesImplementing<StandAloneAlcubierreDrive>().Count == 0)
                {
                    Debug.Log("Trying to hide Warpotron");
                    toolbarControl.Enabled = false;

                }

            }
        }

        private void DestroyLauncher()
        {
#if false
			if (appLauncherButton != null) {
				ApplicationLauncher.Instance.RemoveModApplication (appLauncherButton);
				appLauncherButton = null;
			}

			if (toolbarButton != null) {
				toolbarButton.Destroy ();
				toolbarButton = null;
			}
#endif
            if (toolbarControl != null)
            {
                toolbarControl.OnDestroy();
                Destroy(toolbarControl);
            }

        }

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
                "#WD_Warpotron9000",
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

            GUILayout.BeginVertical();
            Layout.LabelAndText("#WD_upgradeStatus", "#WD_upgradeStatus_t",
                masterDrive.upgradeStatus);
            Layout.LabelAndText("#WD_currentGravityForce", "#WD_currentGravityForce_t",
                masterDrive.currentGravityForce.ToString("F3") + "#WD_Units_g");
            Layout.LabelAndText("#WD_speedRestrictedbyG", "#WD_speedRestrictedbyG_t",
                masterDrive.speedRestrictedbyG.ToString("F3") + "#WD_Units_c");
            Layout.LabelAndText("#WD_currentSpeedFactor", "#WD_currentSpeedFactor_t",
                masterDrive.currentSpeedFactor.ToString("F3") + "#WD_Units_c");
            Layout.LabelAndText("#WD_maximumSpeedFactor", "#WD_maximumSpeedFactor_t",
                masterDrive.maximumSpeedFactor.ToString("F3") + "#WD_Units_c");

            if (maximized)
            {
                if (Layout.Button("#WD_Minimize"))
                {
                    maximized = false;
                    refresh = true;
                }

                Layout.LabelAndText("#WD_minimalRequiredEM", "#WD_minimalRequiredEM_t",
                    masterDrive.minimalRequiredEM.ToString("F3"));
                Layout.LabelAndText("#WD_currentRequiredEM", "#WD_currentRequiredEM_t",
                    masterDrive.requiredForCurrentFactor.ToString("F3"));
                Layout.LabelAndText("#WD_maximumRequiredEM", "#WD_maximumRequiredEM_t",
                    masterDrive.requiredForMaximumFactor.ToString("F3"));

                Layout.LabelAndText("#WD_drivesTotalPower", "#WD_drivesTotalPower_t",
                    masterDrive.drivesTotalPower.ToString("F3"));
                Layout.LabelAndText("#WD_vesselTotalMass", "#WD_vesselTotalMass_t",
                    masterDrive.vesselTotalMass.ToString("F3") + "#WD_Units_t");
                Layout.LabelAndText("#WD_drivesEfficiency", "WD_drivesEfficiency_t",
                    masterDrive.drivesEfficiency.ToString("F3"));


                if (!String.IsNullOrWhiteSpace(lastTooltip))
                {
                    //Debug.Log("tooltip: " + GUI.tooltip);
                    GUIContent contTooltip = new GUIContent(lastTooltip);
                    Rect TooltipPosition = new Rect(); // = new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, 200, 200);

                    Vector2d TooltipMouseOffset = new Vector2d();
                    TooltipPosition.x = Event.current.mousePosition.x + (Single)TooltipMouseOffset.x;
                    TooltipPosition.y = Event.current.mousePosition.y + (Single)TooltipMouseOffset.y;

                    Styles.SetSkin();
                    GUIStyle styleTooltip = new GUIStyle(Styles.textArea);
                    //Int32 TooltipMaxWidth = 200;
                    //styleTooltip.CalcMinMaxWidth(contTooltip, out float minwidth, out float maxwidth); // figure out how wide one line would be
                    //TooltipPosition.width = Math.Min(TooltipMaxWidth - styleTooltip.padding.horizontal, maxwidth); //then work out the height with a max width
                    TooltipPosition.width = 200;

                    TooltipPosition.height = styleTooltip.CalcHeight(contTooltip, TooltipPosition.width); // heres the result
                    GUI.Label(TooltipPosition, contTooltip, styleTooltip);
                    //On top of everything
                    GUI.depth = -10;
                }
                    

                if (Event.current.type == EventType.Repaint && GUI.tooltip != lastTooltip)
                {
                    lastTooltip = GUI.tooltip;
                    refresh = true;
                }


                //string s = new string('>', masterDrive.lowEnergyFactor) + "1" +
                //            new string('<', masterDrive.warpFactors.Length - masterDrive.lowEnergyFactor - 1);

                string s = "1>>>>1>>>>1<<<<1<<<<1<<<<1";

                if (masterDrive.maximumFactor >= masterDrive.currentFactor)
                    s = "<b>" + s.Substring(0, masterDrive.currentFactor) +
                        Utils.Colorize(s.Substring(masterDrive.currentFactor, 1), Palette.green) +
                        s.Substring(masterDrive.currentFactor + 1, masterDrive.maximumFactor - masterDrive.currentFactor) +
                        Utils.Colorize(s.Substring(masterDrive.maximumFactor + 1), Palette.gray50) + "</b>";
                else
                    s = "<b>" + s.Substring(0, masterDrive.maximumFactor + 1) +
                    Utils.Colorize(s.Substring(masterDrive.maximumFactor + 1, masterDrive.currentFactor - masterDrive.maximumFactor - 1), Palette.gray50) +
                    Utils.Colorize(s.Substring(masterDrive.currentFactor, 1), Palette.red)+
                     Utils.Colorize(s.Substring(masterDrive.currentFactor + 1), Palette.gray50) + "</b>";

                // Doesn't work
                Layout.LabelCentered(s, Palette.blue, "AAA" );


                //				Layout.LabelAndText ("Magnitude Diff", masterDrive.magnitudeDiff.ToString ());
                //				Layout.LabelAndText ("Magnitude Change", masterDrive.magnitudeChange.ToString ());
            }
            else if (Layout.Button("#WD_Maximize"))
                maximized = true;

            // a button, that just play alarm sound for no reason
            // probably some unfinished feature
            //if (Layout.Button("alarm"))
            //    masterDrive.PlayAlarm();

            if (TimeWarp.CurrentRateIndex == 0)
            {
                GUILayout.BeginHorizontal();
                if (Layout.Button("#WD_DecreaseFactor", Palette.red/*, GUILayout.Width(141)*/))
                {
                    masterDrive.DecreaseFactor();
                }
                if (Layout.Button("#WD_IncreaseFactor", Palette.green/*, GUILayout.Width(141)*/))
                {
                    masterDrive.IncreaseFactor();
                }
                GUILayout.EndHorizontal();

                if (Layout.Button("#WD_ReduceFactor", Palette.blue))
                {
                    masterDrive.ReduceFactor();
                }

                if (!masterDrive.inWarp)
                {
                    if (Layout.Button("#WD_ActivateWarpDrive", Palette.green))
                    {
                        masterDrive.ActivateWarpDrive();
                    }
                }
                else if (Layout.Button("#WD_DeactivateWarpDrive", Palette.red))
                {
                    masterDrive.DeactivateWarpDrive();
                }

                if (!masterDrive.containmentField)
                {
                    if (Layout.Button("#WD_ActivateContainmentField", Palette.green))
                    {
                        masterDrive.StartContainment();
                    }
                }
                else if (Layout.Button("#WD_DeactivateContainmentField", Palette.red))
                {
                    masterDrive.StopContainment();
                }
            }

            if (Layout.Button("#WD_Close", Palette.red))
                toolbarControl.SetFalse();

            if (!String.IsNullOrWhiteSpace(lastTooltip))
                GUILayout.Label(new GUIContent(lastTooltip));


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
