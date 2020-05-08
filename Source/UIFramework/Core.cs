/*
 * UI Framework licensed under BSD 3-clause license
 * https://github.com/Real-Gecko/Unity5-UIFramework/blob/master/LICENSE.md
*/

using System;
using UnityEngine;

namespace WarpDrive
{
	[KSPAddon(KSPAddon.Startup.Instantly, true)]
	public class Core : MonoBehaviour
	{
		static private Core _instance = null;


		static private bool skinInitialized = false;

		public void Awake()
		{
			if (_instance != null)
			{
				Destroy(this);
				return;
			}
			_instance = this;
		}

		public void OnDestroy()
		{
			if (_instance == this)
				_instance = null;
		}

		public void OnGUI()
		{
			if (skinInitialized)
				return;
			Palette.InitPalette();
			Palette.LoadTextures();
			//Styles.InitStyles();
			skinInitialized = true;
			Destroy(this); // Quit after initialized
		}
	}

	//
	// This is needed for when changing saves, if one save has different skin settings than another
	// It will reload the skin when getting to the SpaceCenter for the first time
	//
	[KSPAddon(KSPAddon.Startup.MainMenu, false)]
	public class CoreMainMenu : MonoBehaviour
	{
		void Start()
		{
			CoreSpaceCenter.initted = false;
		}
	}

	[KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
	public class CoreSpaceCenter : MonoBehaviour
	{
		internal static bool initted = false;

		public void OnGUI()
		{
			if (initted)
				return;
			Styles.InitStyles();
			initted = true;
			Destroy(this); // Quit after initialized
		}

	}

}

