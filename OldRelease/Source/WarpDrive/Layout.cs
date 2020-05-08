using System;
using UnityEngine;

namespace WarpDrive
{
	public class Layout
	{
		public static void Label(string text, params GUILayoutOption[] options)
		{
			Styles.label.normal.textColor = Color.white;
			Styles.label.alignment = TextAnchor.MiddleLeft;
			Styles.label.stretchWidth = false;
			GUILayout.Label(text, Styles.label, options);
		}

		public static void Label(string text, Color color, params GUILayoutOption[] options)
		{
			Styles.label.normal.textColor = color;
			Styles.label.alignment = TextAnchor.MiddleLeft;
			Styles.label.stretchWidth = false;
			GUILayout.Label(text, Styles.label, options);
		}

		public static void LabelCentered(string text, params GUILayoutOption[] options)
		{
			Styles.label.normal.textColor = Color.white;
			Styles.label.alignment = TextAnchor.MiddleCenter;
			Styles.label.stretchWidth = true;
			GUILayout.Label(text, Styles.label, options);
		}

		public static void LabelCentered(string text, Color color, params GUILayoutOption[] options)
		{
			Styles.label.normal.textColor = color;
			Styles.label.alignment = TextAnchor.MiddleCenter;
			Styles.label.stretchWidth = true;
			GUILayout.Label(text, Styles.label, options);
		}

		public static void LabelRight(string text, params GUILayoutOption[] options)
		{
			Styles.label.normal.textColor = Color.white;
			Styles.label.alignment = TextAnchor.MiddleRight;
			Styles.label.stretchWidth = false;
			GUILayout.Label(text, Styles.label, options);
		}

		public static void LabelRight(string text, Color color, params GUILayoutOption[] options)
		{
			Styles.label.normal.textColor = color;
			Styles.label.alignment = TextAnchor.MiddleRight;
			Styles.label.stretchWidth = false;
			GUILayout.Label(text, Styles.label, options);
		}

		public static bool Button(string text, params GUILayoutOption[] options)
		{
			Styles.button.normal.textColor = Color.white;
			Styles.button.alignment = TextAnchor.MiddleCenter;
			Styles.button.stretchWidth = true;
			return GUILayout.Button(text, Styles.button, options);
		}

		public static bool Button(string text, Color color, params GUILayoutOption[] options)
		{
			Styles.button.normal.textColor = color;
			Styles.button.alignment = TextAnchor.MiddleCenter;
			Styles.button.stretchWidth = true;
			return GUILayout.Button(text, Styles.button, options);
		}

		public static bool ButtonLeft(string text, params GUILayoutOption[] options)
		{
			Styles.button.normal.textColor = Color.white;
			Styles.button.alignment = TextAnchor.MiddleLeft;
			Styles.button.stretchWidth = true;
			return GUILayout.Button(text, Styles.button, options);
		}

		public static bool ButtonLeft(string text, Color color, params GUILayoutOption[] options)
		{
			Styles.button.normal.textColor = color;
			Styles.button.alignment = TextAnchor.MiddleLeft;
			Styles.button.stretchWidth = true;
			return GUILayout.Button(text, Styles.button, options);
		}

		public static void LabelAndText(string label, string text)
		{
			GUILayout.BeginHorizontal();
			Label(label + ": ", Palette.blue);
			Label(text, Color.white);
			GUILayout.EndHorizontal();
		}

		public static void Margin(int width)
		{
			GUILayout.Label("", Styles.label, GUILayout.Width((float)width));
		}

		public static Vector2 BeginScrollView(Vector2 scrollPos, params GUILayoutOption[] options)
		{
			return GUILayout.BeginScrollView(scrollPos, false, true, Styles.verticalScrollbarThumb, Styles.verticalScrollbarThumb, Styles.scrollView, options);
		}

		public static void HR(int height = 20)
		{
			GUILayout.Label("", Styles.label, GUILayout.Height((float)height));
		}

		public static int SelectionGrid(int selected, string[] captions, int count, params GUILayoutOption[] options)
		{
			return GUILayout.SelectionGrid(selected, captions, count, Styles.selectionGrid, options);
		}

		public static bool Toggle(bool value, string text, params GUILayoutOption[] options)
		{
			string str = value ? "● " : "○ ";
			return GUILayout.Toggle(value, str + text, Styles.toggle, options);
		}

		public static Rect Window(int id, Rect screenRect, GUI.WindowFunction func, string title, params GUILayoutOption[] options)
		{
			screenRect.width = (float)Math.Floor((double)screenRect.width);
			screenRect.height = (float)Math.Floor((double)screenRect.height);
			return GUILayout.Window(id, screenRect, func, title, Styles.window, options);
		}

		public static float HorizontalSlider(float value, float leftValue, float rightValue, params GUILayoutOption[] options)
		{
			return GUILayout.HorizontalSlider(value, leftValue, rightValue, Styles.horizontalSlider, Styles.horizontalSliderThumb, options);
		}

		public static double HorizontalSlider(double value, double leftValue, double rightValue, params GUILayoutOption[] options)
		{
			return (double)GUILayout.HorizontalSlider((float)value, (float)leftValue, (float)rightValue, Styles.horizontalSlider, Styles.horizontalSliderThumb, options);
		}
	}
}
