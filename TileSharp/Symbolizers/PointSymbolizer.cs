using System.Drawing;

namespace TileSharp.Symbolizers
{
	public class PointSymbolizer : Symbolizer
	{
		public readonly PointShape Shape;
		public readonly Color Color;
		public readonly int Diameter;

		public PointSymbolizer(PointShape shape, Color color, int diameter)
		{
			Shape = shape;
			Color = color;
			Diameter = diameter;
		}
	}
}
