using System;
using System.Reflection;

namespace WarpDrive
{
	internal class ToolbarTypes
	{
		internal readonly Type iToolbarManagerType;

		internal readonly Type functionVisibilityType;

		internal readonly Type functionDrawableType;

		internal readonly ButtonTypes button;

		internal ToolbarTypes()
		{
			iToolbarManagerType = getType("Toolbar.IToolbarManager");
			functionVisibilityType = getType("Toolbar.FunctionVisibility");
			functionDrawableType = getType("Toolbar.FunctionDrawable");
			Type type = getType("Toolbar.IButton");
			button = new ButtonTypes(type);
		}

		internal static Type getType(string name)
		{
			Type type = null;
			AssemblyLoader.loadedAssemblies.TypeOperation(delegate(Type t)
			{
				if (t.FullName == name)
				{
					type = t;
				}
			});
			return type;
		}

		internal static PropertyInfo getProperty(Type type, string name)
		{
			return type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
		}

		internal static PropertyInfo getStaticProperty(Type type, string name)
		{
			return type.GetProperty(name, BindingFlags.Static | BindingFlags.Public);
		}

		internal static EventInfo getEvent(Type type, string name)
		{
			return type.GetEvent(name, BindingFlags.Instance | BindingFlags.Public);
		}

		internal static MethodInfo getMethod(Type type, string name)
		{
			return type.GetMethod(name, BindingFlags.Instance | BindingFlags.Public);
		}
	}
}
