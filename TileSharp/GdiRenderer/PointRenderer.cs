using System.Collections.Generic;
using System.Drawing;
using NetTopologySuite.Features;
using TileSharp.Symbolizers;

namespace TileSharp.GdiRenderer
{
	class PointRenderer : RendererPart
	{
		private static readonly Dictionary<Symbolizer, Brush> BrushCache = new Dictionary<Symbolizer, Brush>();
		
		public PointRenderer(Renderer renderer)
			: base(renderer)
		{
		}

		public override void PreCache(Symbolizer symbolizer)
		{
			var pointSymbolizer = (PointSymbolizer)symbolizer;

			if (!BrushCache.ContainsKey(symbolizer))
			{
				Brush brush = new SolidBrush(pointSymbolizer.Color);
				BrushCache.Add(symbolizer, brush);
			}
		}

		public override void Render(Symbolizer symbolizer, Feature feature)
		{
			var pointSymbolizer = (PointSymbolizer)symbolizer;

			Brush brush = BrushCache[symbolizer];

			var p = Project(feature.Geometry.Coordinates)[0];
			var diameter = (float)pointSymbolizer.Diameter;
			var radius = diameter * 0.5f;

			switch (pointSymbolizer.Shape)
			{
				case PointShape.Circle:
					Graphics.FillEllipse(brush, p.X - radius, p.Y - radius, diameter, diameter);
					break;
				case PointShape.Square:
					Graphics.FillRectangle(brush, p.X - radius, p.Y - radius, diameter, diameter);
					break;
				case PointShape.Triangle:
					Graphics.FillPolygon(brush, new []
					{
						new PointF(p.X - radius, p.Y + diameter / 3),
						new PointF(p.X, p.Y - radius),
						new PointF(p.X + radius, p.Y + diameter / 3)
					});
					break;
				case PointShape.Star:
					Graphics.FillPolygon(brush, new []
					{
						new PointF(p.X, p.Y - radius), //Out top middle
						
						//Right
						new PointF(p.X + diameter * 0.15f, p.Y - diameter * 0.15f), //in top
						new PointF(p.X + radius, p.Y - diameter * 0.12f), // out top
						new PointF(p.X + diameter * 0.25f, p.Y + diameter * 0.13f), //in mid
						new PointF(p.X + diameter * 0.33f, p.Y + radius), //out bottom
						
						new PointF(p.X, p.Y + diameter * 0.3f), //in bottom mid

						//Left
						new PointF(p.X - diameter * 0.33f, p.Y + radius), //out bottom
						new PointF(p.X - diameter * 0.25f, p.Y + diameter * 0.13f), //in mid
						new PointF(p.X - radius, p.Y - diameter * 0.12f), // out top
						new PointF(p.X - diameter * 0.15f, p.Y - diameter * 0.15f), //in top

					});
					break;
			}
		}
	}
}
