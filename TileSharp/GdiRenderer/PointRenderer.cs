using System.Drawing;
using NetTopologySuite.Features;
using TileSharp.Symbolizers;

namespace TileSharp.GdiRenderer
{
	class PointRenderer : RendererPart
	{
		public PointRenderer(Renderer renderer) : base(renderer)
		{
		}

		public override void Render(Symbolizer symbolizer, Feature feature)
		{
			var pointSymbolizer = (PointSymbolizer)symbolizer;

			//TODO: cache this
			var brush = new SolidBrush(pointSymbolizer.Color);

			var diff = pointSymbolizer.Diameter * 0.5f;
			var p = Project(feature.Geometry.Coordinates)[0];

			Graphics.FillEllipse(brush, p.X - diff, p.Y - diff, pointSymbolizer.Diameter, pointSymbolizer.Diameter);
		}
	}
}
