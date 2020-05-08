using System;
using System.Collections.Generic;
using System.Reflection;

namespace WarpDrive
{
	public class ToolbarManager : IToolbarManager
	{
		private static bool? toolbarAvailable;

		private static IToolbarManager instance_;

		private object realToolbarManager;

		private MethodInfo addMethod;

		private Dictionary<object, IButton> buttons = new Dictionary<object, IButton>();

		private ToolbarTypes types = new ToolbarTypes();

		public static bool ToolbarAvailable
		{
			get
			{
				if (!toolbarAvailable.HasValue)
				{
					toolbarAvailable = (Instance != null);
				}
				return toolbarAvailable.Value;
			}
		}

		public static IToolbarManager Instance
		{
			get
			{
				if (toolbarAvailable != false && instance_ == null)
				{
					Type type = ToolbarTypes.getType("Toolbar.ToolbarManager");
					if (type != null)
					{
						instance_ = new ToolbarManager(ToolbarTypes.getStaticProperty(type, "Instance").GetValue(null, null));
					}
				}
				return instance_;
			}
		}

		private ToolbarManager(object realToolbarManager)
		{
			this.realToolbarManager = realToolbarManager;
			addMethod = ToolbarTypes.getMethod(types.iToolbarManagerType, "add");
		}

		public IButton add(string ns, string id)
		{
			object obj = addMethod.Invoke(realToolbarManager, new object[2]
			{
				ns,
				id
			});
			IButton button = new Button(obj, types);
			buttons.Add(obj, button);
			return button;
		}
	}
}
