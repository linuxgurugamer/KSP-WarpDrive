using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace WarpDrive
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct Palette
	{
		public static Color white = new Color(1f, 1f, 1f);

		public static Color dimWhite = new Color(0.9f, 0.9f, 0.9f, 0.9f);

		public static Color black = new Color(0f, 0f, 0f, 1f);

		public static Color red = new Color(1f, 0.8f, 0.8f);

		public static Color darkRed = new Color(0.7f, 0.4f, 0.4f);

		public static Color green = new Color(0.6f, 1f, 0.8f);

		public static Color darkGreen = new Color(0.4f, 0.7f, 0.4f);

		public static Color blue = new Color(0.7f, 0.7f, 1f);

		public static Color yellow = new Color(1f, 1f, 0.5f);

		public static Color gray60 = new Color(0.6f, 0.6f, 0.6f, 0.85f);

		public static Color gray50 = new Color(0.5f, 0.5f, 0.5f);

		public static Color gray40 = new Color(0.4f, 0.4f, 0.4f);

		public static Color gray30 = new Color(0.3f, 0.3f, 0.3f, 0.85f);

		public static Color gray20 = new Color(0.2f, 0.2f, 0.2f);

		public static Color gray10 = new Color(0.1f, 0.1f, 0.1f);

		public static Color transparent = new Color(0f, 0f, 0f, 0f);

		public static Texture2D tBlack = new Texture2D(1, 1);

		public static Texture2D tDarkRed = new Texture2D(1, 1);

		public static Texture2D tDarkGreen = new Texture2D(1, 1);

		public static Texture2D tGray60 = new Texture2D(1, 1);

		public static Texture2D tGray50 = new Texture2D(1, 1);

		public static Texture2D tGray40 = new Texture2D(1, 1);

		public static Texture2D tGray30 = new Texture2D(1, 1);

		public static Texture2D tGray20 = new Texture2D(1, 1);

		public static Texture2D tGray10 = new Texture2D(1, 1);

		public static Texture2D tTransparent = new Texture2D(1, 1);

		public static Texture2D tWindowBack = new Texture2D(8, 8);

		public static Texture2D tButtonBack = new Texture2D(8, 8);

		public static Texture2D tButtonHover = new Texture2D(8, 8);

		internal static void InitPalette()
		{
			tBlack.SetPixel(0, 0, black);
			tBlack.Apply();
			tDarkRed.SetPixel(0, 0, darkRed);
			tDarkRed.Apply();
			tDarkGreen.SetPixel(0, 0, darkGreen);
			tDarkGreen.Apply();
			tGray60.SetPixel(0, 0, gray60);
			tGray60.Apply();
			tGray50.SetPixel(0, 0, gray50);
			tGray50.Apply();
			tGray40.SetPixel(0, 0, gray40);
			tGray40.Apply();
			tGray30.SetPixel(0, 0, gray30);
			tGray30.Apply();
			tGray40.SetPixel(0, 0, gray40);
			tGray40.Apply();
			tGray20.SetPixel(0, 0, gray20);
			tGray20.Apply();
			tGray10.SetPixel(0, 0, gray10);
			tGray10.Apply();
			tTransparent.SetPixel(0, 0, transparent);
			tTransparent.Apply();
		}

		internal static void LoadTextures()
		{
			byte[] array = File.ReadAllBytes("GameData/WarpDrive/Textures/window-back.png");
			tWindowBack.LoadImage(array);
			array = File.ReadAllBytes("GameData/WarpDrive/Textures/button-back.png");
			tButtonBack.LoadImage(array);
			array = File.ReadAllBytes("GameData/WarpDrive/Textures/button-hover-back.png");
			tButtonHover.LoadImage(array);
		}
	}
}
