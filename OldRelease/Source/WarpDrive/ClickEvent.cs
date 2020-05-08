using System;
using System.Reflection;

namespace WarpDrive
{
	public class ClickEvent : EventArgs
	{
		public readonly IButton Button;

		public readonly int MouseButton;

		internal ClickEvent(object realEvent, IButton button)
		{
			Type type = realEvent.GetType();
			Button = button;
			MouseButton = (int)type.GetField("MouseButton", BindingFlags.Instance | BindingFlags.Public).GetValue(realEvent);
		}
	}
}
