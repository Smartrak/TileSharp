using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using GeoAPI.Geometries;
using NetTopologySuite.Features;
using TileSharp.Symbolizers;

namespace TileSharp.GdiRenderer
{
	class PolygonRenderer : RendererPart
	{
		private static readonly Dictionary<Symbolizer, Brush> BrushCache = new Dictionary<Symbolizer, Brush>();
		private static readonly Dictionary<Symbolizer, Pen> PenCache = new Dictionary<Symbolizer, Pen>();

		public PolygonRenderer(Renderer renderer)
			: base(renderer)
		{
		}

		public override void Render(Symbolizer symbolizer, Feature feature)
		{
			var polygonSymbolizer = (PolygonSymbolizer)symbolizer;

			Brush brush;
			if (!BrushCache.TryGetValue(symbolizer, out brush))
			{
				lock (symbolizer)
				{
					if (!BrushCache.TryGetValue(symbolizer, out brush))
					{
						brush = new SolidBrush(polygonSymbolizer.FillColor);
						BrushCache.Add(symbolizer, brush);
					}
				}
			}

			Pen pen;
			if (!PenCache.TryGetValue(symbolizer, out pen))
			{
				lock (symbolizer)
				{
					if (!PenCache.TryGetValue(symbolizer, out pen))
					{
						if (polygonSymbolizer.LineWidth.HasValue)
						{
							pen = new Pen(polygonSymbolizer.LineColor.Value, polygonSymbolizer.LineWidth.Value);
						}
						PenCache.Add(symbolizer, pen);
					}
				}
			}

			var polygon = (IPolygon)feature.Geometry;

			//TODO: Do we need two version of the code here, or can we just always use a graphics path?
			if (polygon.Holes.Length > 0)
			{
				using (var gp = new GraphicsPath())
				{
					gp.AddPolygon(Project(polygon.ExteriorRing.Coordinates));

					for (var i = 0; i < polygon.Holes.Length; i++)
						gp.AddPolygon(Project(polygon.Holes[i].Coordinates));

					Graphics.FillPath(brush, gp);

					if (pen != null)
						Graphics.DrawPath(pen, gp);
				}
			}
			else
			{
				var points = Project(polygon.Coordinates);

				Graphics.FillPolygon(brush, points);

				if (pen != null)
					Graphics.DrawLines(pen, points);
			}
		}
	}
}
