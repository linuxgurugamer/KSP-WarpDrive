using System;
using System.Reflection;
using UnityEngine;

namespace WarpDrive
{
	public class PopupMenuDrawable : IDrawable
	{
		private object realPopupMenuDrawable;

		private MethodInfo updateMethod;

		private MethodInfo drawMethod;

		private MethodInfo addOptionMethod;

		private MethodInfo addSeparatorMethod;

		private MethodInfo destroyMethod;

		private EventInfo onAnyOptionClickedEvent;

		public event Action OnAnyOptionClicked
		{
			add
			{
				onAnyOptionClickedEvent.AddEventHandler(realPopupMenuDrawable, value);
			}
			remove
			{
				onAnyOptionClickedEvent.RemoveEventHandler(realPopupMenuDrawable, value);
			}
		}

		public PopupMenuDrawable()
		{
			Type type = ToolbarTypes.getType("Toolbar.PopupMenuDrawable");
			realPopupMenuDrawable = Activator.CreateInstance(type, null);
			updateMethod = ToolbarTypes.getMethod(type, "Update");
			drawMethod = ToolbarTypes.getMethod(type, "Draw");
			addOptionMethod = ToolbarTypes.getMethod(type, "AddOption");
			addSeparatorMethod = ToolbarTypes.getMethod(type, "AddSeparator");
			destroyMethod = ToolbarTypes.getMethod(type, "Destroy");
			onAnyOptionClickedEvent = ToolbarTypes.getEvent(type, "OnAnyOptionClicked");
		}

		public void Update()
		{
			updateMethod.Invoke(realPopupMenuDrawable, null);
		}

		public Vector2 Draw(Vector2 position)
		{
			return (Vector2)drawMethod.Invoke(realPopupMenuDrawable, new object[1]
			{
				position
			});
		}

		public IButton AddOption(string text)
		{
			return new Button(addOptionMethod.Invoke(realPopupMenuDrawable, new object[1]
			{
				text
			}), new ToolbarTypes());
		}

		public void AddSeparator()
		{
			addSeparatorMethod.Invoke(realPopupMenuDrawable, null);
		}

		public void Destroy()
		{
			destroyMethod.Invoke(realPopupMenuDrawable, null);
		}
	}
}
