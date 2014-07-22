using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using GeoAPI.Geometries;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using TileSharp.LabelOverlapPreventers;
using TileSharp.Symbolizers;

namespace TileSharp.GdiRenderer
{
	class TextRenderer : RendererPart
	{
		private static readonly Dictionary<Symbolizer, Font> FontCache = new Dictionary<Symbolizer, Font>();
		private static readonly Dictionary<Symbolizer, Brush> BrushCache = new Dictionary<Symbolizer, Brush>();
		private static readonly Dictionary<Symbolizer, Pen> PenCache = new Dictionary<Symbolizer, Pen>();
		
		const int FontSize = 14;

		public TextRenderer(Renderer renderer)
			: base(renderer)
		{
		}

		public override void PreCache(Symbolizer symbolizer)
		{
			var textSymbolizer = (TextSymbolizer)symbolizer;

			if (!FontCache.ContainsKey(symbolizer))
			{
				Font font = new Font(FontFamily.GenericSansSerif, FontSize, FontStyle.Bold);
				FontCache.Add(symbolizer, font);
			}

			if (!PenCache.ContainsKey(symbolizer))
			{
				var pen = new Pen(textSymbolizer.TextHaloColor, 3);
				pen.LineJoin = LineJoin.Round;
				PenCache.Add(symbolizer, pen);
			}

			if (!BrushCache.ContainsKey(symbolizer))
			{
				var brush = new SolidBrush(textSymbolizer.TextColor);
				BrushCache.Add(symbolizer, brush);
			}
		}

		public override void Render(Symbolizer symbolizer, Feature feature)
		{
			var textSymbolizer = (TextSymbolizer)symbolizer;

			switch (textSymbolizer.Placement)
			{
				case TextSymbolizer.PlacementType.Line:
					RenderLineLabel(textSymbolizer, feature);
					break;
				case TextSymbolizer.PlacementType.Point:
					RenderPointLabel(textSymbolizer, feature);
					break;
				default:
					throw new Exception("Don't know how to render text for " + textSymbolizer.Placement);
			}
		}

		private void RenderPointLabel(TextSymbolizer textSymbolizer, Feature feature)
		{
			//TODO labels could be not strings
			var str = feature.Attributes.Exists(textSymbolizer.LabelAttribute) ? feature.Attributes[textSymbolizer.LabelAttribute] as string : null;
			if (string.IsNullOrWhiteSpace(str))
				return;

			var emSize = Graphics.DpiY * FontSize / 72;

			//ref http://msdn.microsoft.com/en-us/library/xwf9s90b(v=vs.110).aspx
			var font = FontCache[textSymbolizer];
			var ascent = emSize * font.FontFamily.GetCellAscent(FontStyle.Bold) / font.FontFamily.GetEmHeight(FontStyle.Bold);

			var coord = Project(feature.Geometry.Coordinates)[0];

			var size = new SizeF(Graphics.MeasureString(str, font).Width, ascent);

			var offset = new SizeF();
			switch (textSymbolizer.Alignment)
			{
				case ContentAlignment.MiddleCenter:
					//no offset
					break;
				case ContentAlignment.TopRight:
					offset = new SizeF(size.Width * 0.5f, -size.Height * 0.5f);
					break;
				default:
					throw new NotImplementedException();
			}

			TryRenderText(textSymbolizer, feature, emSize, str, coord, offset, size, -Config.Angle);
		}

		private void RenderLineLabel(TextSymbolizer textSymbolizer, Feature feature)
		{
			//TODO labels could be not strings
			var str = feature.Attributes.Exists(textSymbolizer.LabelAttribute) ? feature.Attributes[textSymbolizer.LabelAttribute] as string : null;
			if (string.IsNullOrWhiteSpace(str))
				return;

			var emSize = Graphics.DpiY * FontSize / 72;

			//ref http://msdn.microsoft.com/en-us/library/xwf9s90b(v=vs.110).aspx
			var font = FontCache[textSymbolizer];
			var ascent = emSize * font.FontFamily.GetCellAscent(FontStyle.Bold) / font.FontFamily.GetEmHeight(FontStyle.Bold);

			Graphics.SmoothingMode = SmoothingMode.HighQuality;

			var size = Graphics.MeasureString(str, font);
			float spacing = textSymbolizer.Spacing;

			var coords = ProjectToCoordinate(feature.Geometry.Coordinates);
			var coordsAsLine = new LineString(coords);
			var lengthIndexed = new NetTopologySuite.LinearReferencing.LengthIndexedLine(coordsAsLine);

			var labelCount = (int)((coordsAsLine.Length - spacing) / (size.Width + spacing));
			if (labelCount < 1 || textSymbolizer.Spacing == 0)
				labelCount = 1;

			//work out spacing based on the amount of labels we'll be putting on
			spacing = ((float)coordsAsLine.Length / labelCount) - size.Width;

			for (var i = 0; i < labelCount; i++)
			{
				var labelCenterLength = (spacing + size.Width) * (0.5f + i);
				var subLine = lengthIndexed.ExtractLine(labelCenterLength - (size.Width / 2), labelCenterLength + (size.Width / 2));
				if (subLine.Coordinates.Length < 2)
					continue;

				var firstCoord = subLine.Coordinates[0];
				var lastCoord = subLine.Coordinates[subLine.Coordinates.Length - 1];
				var middleOfLabelLinePoint = lengthIndexed.ExtractPoint(labelCenterLength);


				var midPoint = new PointF((float)(firstCoord.X + lastCoord.X + middleOfLabelLinePoint.X + middleOfLabelLinePoint.X) * 0.25f, (float)(firstCoord.Y + lastCoord.Y + middleOfLabelLinePoint.Y + middleOfLabelLinePoint.Y) * 0.25f);

				var angle = (float)(Math.Atan2(lastCoord.Y - firstCoord.Y, lastCoord.X - firstCoord.X) * 180 / Math.PI);
				//Keep the text up the right way
				if (angle + Config.Angle > 90)
					angle -= 180;
				if (angle + Config.Angle < -90)
					angle += 180;

				TryRenderText(textSymbolizer, feature, emSize, str, midPoint, SizeF.Empty, new SizeF(size.Width, ascent), angle);
			}
		}

		private void TryRenderText(Symbolizer symbolizer, Feature feature, float emSize, string str, PointF center, SizeF offset, SizeF size, float angle)
		{
			var pen = PenCache[symbolizer];
			var brush = BrushCache[symbolizer];

			var poly = GetCollisionBox(center, offset, size, angle);

			if (LabelOverlapPreventer.CanPlaceLabel(Config, new LabelDetails(poly, feature)))
			{
				//Graphics.DrawLines(Pens.Green, poly.Coordinates.Select(c => new PointF((float)(c.X - xPlus), (float)(c.Y - yPlus))).ToArray());

				Graphics.SmoothingMode = SmoothingMode.HighQuality;
				using (var path = new GraphicsPath())
				{
					path.AddString(str, FontFamily.GenericSansSerif, (int)FontStyle.Bold, emSize, new PointF(-size.Width * 0.5f, -size.Height * 0.5f), new StringFormat());

					//path.Transform
					Graphics.TranslateTransform(center.X, center.Y);
					Graphics.RotateTransform(angle);
					Graphics.TranslateTransform(offset.Width, offset.Height);
					{
						Graphics.DrawPath(pen, path);
						Graphics.FillPath(brush, path);
					}
					Graphics.ResetTransform();
				}
				Graphics.SmoothingMode = SmoothingMode.Default;
			}
		}

		private Polygon GetCollisionBox(PointF center, SizeF offset, SizeF size, float angle)
		{
			var xPlus = Config.Envelope.MinX / SphericalMercator.Resolution(Config.ZoomLevel);
			var yPlus = -Config.Envelope.MaxY / SphericalMercator.Resolution(Config.ZoomLevel);

			var halfWidth = size.Width * 0.5f;
			var halfHeight = size.Height * 0.5f;

			var rotation = new Matrix();
			rotation.RotateAt(angle, center);

			var points = new[]
			{
				new PointF(center.X + offset.Width + halfWidth, center.Y + offset.Height + halfHeight),
				new PointF(center.X + offset.Width - halfWidth, center.Y + offset.Height + halfHeight),
				new PointF(center.X + offset.Width - halfWidth, center.Y + offset.Height - halfHeight),
				new PointF(center.X + offset.Width + halfWidth, center.Y + offset.Height - halfHeight)
			};
			rotation.TransformPoints(points);

			var poly = new Polygon(new LinearRing(new[]
			{
				new Coordinate(xPlus + points[0].X, yPlus + points[0].Y),
				new Coordinate(xPlus + points[1].X, yPlus + points[1].Y),
				new Coordinate(xPlus + points[2].X, yPlus + points[2].Y),
				new Coordinate(xPlus + points[3].X, yPlus + points[3].Y),
				new Coordinate(xPlus + points[0].X, yPlus + points[0].Y)
			}));
			return poly;
		}
	}
}
