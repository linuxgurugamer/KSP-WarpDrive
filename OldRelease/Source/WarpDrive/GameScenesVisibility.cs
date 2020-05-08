using System;
using System.Reflection;

namespace WarpDrive
{
	public class GameScenesVisibility : IVisibility
	{
		private object realGameScenesVisibility;

		private PropertyInfo visibleProperty;

		public bool Visible => (bool)visibleProperty.GetValue(realGameScenesVisibility, null);

		public GameScenesVisibility(params GameScenes[] gameScenes)
		{
			Type type = ToolbarTypes.getType("Toolbar.GameScenesVisibility");
			realGameScenesVisibility = Activator.CreateInstance(type, gameScenes);
			visibleProperty = ToolbarTypes.getProperty(type, "Visible");
		}
	}
}
