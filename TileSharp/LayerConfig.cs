using System.Collections;
using System.Collections.Generic;
using System.Drawing;

namespace TileSharp
{
	public class LayerConfig
	{
		public readonly Color BackgroundColor;
		public readonly List<Layer> Layers;

		internal bool RenderersAreCached;

		public LayerConfig(Color backgroundColor, List<Layer> layers)
		{
			BackgroundColor = backgroundColor;
			Layers = layers;
		}
	}
}