using System.Collections.Generic;
using System.Drawing;
using NetTopologySuite.Features;
using TileSharp.Symbolizers;

namespace TileSharp.GdiRenderer
{
	class LineRenderer : RendererPart
	{
		private static readonly Dictionary<Symbolizer, Pen> PenCache = new Dictionary<Symbolizer, Pen>();
 
		public LineRenderer(Renderer renderer) : base(renderer)
		{
		}

		public override void PreCache(Symbolizer symbolizer)
		{
			var lineSymbolizer = (LineSymbolizer)symbolizer;

			Pen pen;
			if (!PenCache.TryGetValue(symbolizer, out pen))
			{
				pen = new Pen(lineSymbolizer.Color, lineSymbolizer.Thickness);
				if (lineSymbolizer.DashPattern != null)
					pen.DashPattern = lineSymbolizer.DashPattern;
				PenCache.Add(symbolizer, pen);
			}
		}

		public override void Render(Symbolizer symbolizer, Feature feature)
		{
			var pen = PenCache[symbolizer];

			var points = Project(feature.Geometry.Coordinates);
			Graphics.DrawLines(pen, points);
		}
	}
}
