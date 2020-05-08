using System;

namespace WarpDrive
{
	public abstract class MouseMoveEvent : EventArgs
	{
		public readonly IButton button;

		internal MouseMoveEvent(IButton button)
		{
			this.button = button;
		}
	}
}
