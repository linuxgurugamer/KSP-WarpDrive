using System;
using System.Reflection;
using UnityEngine;

namespace WarpDrive
{
	internal class Button : IButton
	{
		private object realButton;

		private ToolbarTypes types;

		private Delegate realClickHandler;

		private Delegate realMouseEnterHandler;

		private Delegate realMouseLeaveHandler;

		private IVisibility visibility_;

		private IDrawable drawable_;

		public string Text
		{
			get
			{
				return (string)types.button.textProperty.GetValue(realButton, null);
			}
			set
			{
				types.button.textProperty.SetValue(realButton, value, null);
			}
		}

		public Color TextColor
		{
			get
			{
				return (Color)types.button.textColorProperty.GetValue(realButton, null);
			}
			set
			{
				types.button.textColorProperty.SetValue(realButton, value, null);
			}
		}

		public string TexturePath
		{
			get
			{
				return (string)types.button.texturePathProperty.GetValue(realButton, null);
			}
			set
			{
				types.button.texturePathProperty.SetValue(realButton, value, null);
			}
		}

		public string ToolTip
		{
			get
			{
				return (string)types.button.toolTipProperty.GetValue(realButton, null);
			}
			set
			{
				types.button.toolTipProperty.SetValue(realButton, value, null);
			}
		}

		public bool Visible
		{
			get
			{
				return (bool)types.button.visibleProperty.GetValue(realButton, null);
			}
			set
			{
				types.button.visibleProperty.SetValue(realButton, value, null);
			}
		}

		public IVisibility Visibility
		{
			get
			{
				return visibility_;
			}
			set
			{
				object value2 = null;
				if (value != null)
				{
					value2 = Activator.CreateInstance(types.functionVisibilityType, (Func<bool>)(() => value.Visible));
				}
				types.button.visibilityProperty.SetValue(realButton, value2, null);
				visibility_ = value;
			}
		}

		public bool EffectivelyVisible => (bool)types.button.effectivelyVisibleProperty.GetValue(realButton, null);

		public bool Enabled
		{
			get
			{
				return (bool)types.button.enabledProperty.GetValue(realButton, null);
			}
			set
			{
				types.button.enabledProperty.SetValue(realButton, value, null);
			}
		}

		public bool Important
		{
			get
			{
				return (bool)types.button.importantProperty.GetValue(realButton, null);
			}
			set
			{
				types.button.importantProperty.SetValue(realButton, value, null);
			}
		}

		public IDrawable Drawable
		{
			get
			{
				return drawable_;
			}
			set
			{
				object value2 = null;
				if (value != null)
				{
					value2 = Activator.CreateInstance(types.functionDrawableType, (Action)delegate
					{
						value.Update();
					}, (Func<Vector2, Vector2>)((Vector2 pos) => value.Draw(pos)));
				}
				types.button.drawableProperty.SetValue(realButton, value2, null);
				drawable_ = value;
			}
		}

		public event ClickHandler OnClick;

		public event MouseEnterHandler OnMouseEnter;

		public event MouseLeaveHandler OnMouseLeave;

		internal Button(object realButton, ToolbarTypes types)
		{
			this.realButton = realButton;
			this.types = types;
			realClickHandler = attachEventHandler(types.button.onClickEvent, "clicked", realButton);
			realMouseEnterHandler = attachEventHandler(types.button.onMouseEnterEvent, "mouseEntered", realButton);
			realMouseLeaveHandler = attachEventHandler(types.button.onMouseLeaveEvent, "mouseLeft", realButton);
		}

		private Delegate attachEventHandler(EventInfo @event, string methodName, object realButton)
		{
			MethodInfo method = base.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
			Delegate @delegate = Delegate.CreateDelegate(@event.EventHandlerType, this, method);
			@event.AddEventHandler(realButton, @delegate);
			return @delegate;
		}

		private void clicked(object realEvent)
		{
			if (this.OnClick != null)
			{
				this.OnClick(new ClickEvent(realEvent, this));
			}
		}

		private void mouseEntered(object realEvent)
		{
			if (this.OnMouseEnter != null)
			{
				this.OnMouseEnter(new MouseEnterEvent(this));
			}
		}

		private void mouseLeft(object realEvent)
		{
			if (this.OnMouseLeave != null)
			{
				this.OnMouseLeave(new MouseLeaveEvent(this));
			}
		}

		public void Destroy()
		{
			detachEventHandler(types.button.onClickEvent, realClickHandler, realButton);
			detachEventHandler(types.button.onMouseEnterEvent, realMouseEnterHandler, realButton);
			detachEventHandler(types.button.onMouseLeaveEvent, realMouseLeaveHandler, realButton);
			types.button.destroyMethod.Invoke(realButton, null);
		}

		private void detachEventHandler(EventInfo @event, Delegate d, object realButton)
		{
			@event.RemoveEventHandler(realButton, d);
		}
	}
}
