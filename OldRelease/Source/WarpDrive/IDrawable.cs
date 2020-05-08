using UnityEngine;

namespace WarpDrive
{
	public interface IDrawable
	{
		void Update();

		Vector2 Draw(Vector2 position);
	}
}
