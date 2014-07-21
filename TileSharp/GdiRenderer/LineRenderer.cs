using System.Drawing;
using NetTopologySuite.Features;
using TileSharp.Symbolizers;

namespace TileSharp.GdiRenderer
{
	class LineRenderer : RendererPart
	{
		public LineRenderer(Renderer renderer) : base(renderer)
		{
		}

		public override void Render(Symbolizer symbolizer, Feature feature)
		{
			var lineSymbolizer = (LineSymbolizer)symbolizer;

			//TODO: cache this
			var pen = new Pen(lineSymbolizer.Color, lineSymbolizer.Thickness);
			if (lineSymbolizer.DashPattern != null)
				pen.DashPattern = lineSymbolizer.DashPattern;

			var points = Project(feature.Geometry.Coordinates);
			Graphics.DrawLines(pen, points);
		}
	}
}
