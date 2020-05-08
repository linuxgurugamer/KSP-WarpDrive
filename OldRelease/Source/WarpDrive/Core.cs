using UnityEngine;

namespace WarpDrive
{
	[KSPAddon(KSPAddon.Startup.Instantly, true)]
	public class Core : MonoBehaviour
	{
		private static Core _instance;

		private static bool skinInitialized;

		public static Core Instance => _instance;

		public void Awake()
		{
			if ((Object)_instance != (Object)null)
			{
				Object.Destroy(this);
			}
			else
			{
				_instance = this;
			}
		}

		public void OnDestroy()
		{
			if ((Object)_instance == (Object)this)
			{
				_instance = null;
			}
		}

		public void OnGUI()
		{
			if (!skinInitialized)
			{
				Palette.InitPalette();
				Palette.LoadTextures();
				Styles.InitStyles();
				skinInitialized = true;
				Object.Destroy(this);
			}
		}
	}
}
