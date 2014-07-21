using System.Drawing;

namespace TileSharp.Symbolizers
{
	public class PointSymbolizer : Symbolizer
	{
		public readonly Color Color;
		public readonly int Diameter;

		public PointSymbolizer(Color color, int diameter)
		{
			Color = color;
			Diameter = diameter;
		}
	}
}
