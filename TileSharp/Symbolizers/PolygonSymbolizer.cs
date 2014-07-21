using System.Drawing;

namespace TileSharp.Symbolizers
{
	public class PolygonSymbolizer : Symbolizer
	{
		public readonly Color FillColor;
		public readonly Color? LineColor;
		public readonly int? LineWidth;

		public PolygonSymbolizer(Color fillColor, Color? lineColor = null, int? lineWidth = null)
		{
			FillColor = fillColor;
			LineColor = lineColor;
			LineWidth = lineWidth;
		}
	}
}
