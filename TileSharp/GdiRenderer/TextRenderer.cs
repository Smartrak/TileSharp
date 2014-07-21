using System;
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
		public TextRenderer(Renderer renderer)
			: base(renderer)
		{
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

		const int fontSize = 14;

		private void RenderPointLabel(TextSymbolizer textSymbolizer, Feature feature)
		{
			var emSize = Graphics.DpiY * fontSize / 72;

			//TODO: Cache
			var pen = new Pen(textSymbolizer.TextHaloColor, 3);
			pen.LineJoin = LineJoin.Round;
			var brush = new SolidBrush(textSymbolizer.TextColor);
			//ref http://msdn.microsoft.com/en-us/library/xwf9s90b(v=vs.110).aspx
			var font = new Font(FontFamily.GenericSansSerif, fontSize, FontStyle.Bold);
			var ascent = emSize * FontFamily.GenericSansSerif.GetCellAscent(FontStyle.Bold) / FontFamily.GenericSansSerif.GetEmHeight(FontStyle.Bold);

			Graphics.SmoothingMode = SmoothingMode.HighQuality;

			var coord = Project(feature.Geometry.Coordinates)[0];

			//TODO labels could be not strings
			var str = feature.Attributes.Exists(textSymbolizer.LabelAttribute) ? feature.Attributes[textSymbolizer.LabelAttribute] as string : null;
			if (string.IsNullOrWhiteSpace(str))
				return;

			coord += new SizeF(1, -ascent - 1);
			var size = Graphics.MeasureString(str, font);

			switch (textSymbolizer.Alignment)
			{
				case ContentAlignment.TopRight:
					//Do nothing
					break;
				case ContentAlignment.MiddleCenter:
					coord.X -= size.Width / 2;
					coord.Y += ascent / 2;
					break;
				default:
					throw new NotImplementedException();
			}

			var poly = GetCollisionBox(coord, new SizeF(size.Width, ascent));

			if (LabelOverlapPreventer.CanPlaceLabel(Config, new LabelDetails(poly, feature)))
			{
				//Graphics.DrawLines(Pens.Orange, poly.Coordinates.Select(c => new PointF((float)(c.X - xPlus), (float)(c.Y - yPlus))).ToArray());

				using (var path = new GraphicsPath())
				{
					path.AddString(str, FontFamily.GenericSansSerif, (int)FontStyle.Bold, emSize, coord, new StringFormat());

					Graphics.DrawPath(pen, path);
					Graphics.FillPath(brush, path);
				}
			}
			Graphics.SmoothingMode = SmoothingMode.Default;
		}

		private void RenderLineLabel(TextSymbolizer textSymbolizer, Feature feature)
		{
			var emSize = Graphics.DpiY * fontSize / 72;

			//TODO: Cache
			var pen = new Pen(textSymbolizer.TextHaloColor, 3);
			pen.LineJoin = LineJoin.Round;
			var brush = new SolidBrush(textSymbolizer.TextColor);
			//ref http://msdn.microsoft.com/en-us/library/xwf9s90b(v=vs.110).aspx
			var font = new Font(FontFamily.GenericSansSerif, fontSize, FontStyle.Bold);
			var ascent = emSize * FontFamily.GenericSansSerif.GetCellAscent(FontStyle.Bold) / FontFamily.GenericSansSerif.GetEmHeight(FontStyle.Bold);

			Graphics.SmoothingMode = SmoothingMode.HighQuality;

			//TODO labels could be not strings
			var str = feature.Attributes.Exists(textSymbolizer.LabelAttribute) ? feature.Attributes[textSymbolizer.LabelAttribute] as string : null;
			if (string.IsNullOrWhiteSpace(str))
				return;

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
				if (angle > 90)
					angle -= 180;
				if (angle < -90)
					angle += 180;

				var topLeft = new PointF(-size.Width / 2, -ascent / 2);

				var poly = GetCollisionBox(midPoint, new SizeF(size.Width, ascent), angle);

				if (LabelOverlapPreventer.CanPlaceLabel(Config, new LabelDetails(poly, feature)))
				{
					//Graphics.DrawLines(Pens.Green, poly.Coordinates.Select(c => new PointF((float)(c.X - xPlus), (float)(c.Y - yPlus))).ToArray());

					using (var path = new GraphicsPath())
					{
						path.AddString(str, FontFamily.GenericSansSerif, (int)FontStyle.Bold, emSize, topLeft, new StringFormat());

						//path.Transform
						Graphics.TranslateTransform(midPoint.X, midPoint.Y);
						Graphics.RotateTransform(angle);
						{
							Graphics.DrawPath(pen, path);
							Graphics.FillPath(brush, path);
						}
						Graphics.ResetTransform();
					}
				}
			}
			Graphics.SmoothingMode = SmoothingMode.Default;
		}

		private Polygon GetCollisionBox(PointF center, SizeF size, float angle)
		{
			var xPlus = Config.Envelope.MinX / SphericalMercator.Resolution(Config.ZoomLevel);
			var yPlus = -Config.Envelope.MaxY / SphericalMercator.Resolution(Config.ZoomLevel);

			var rotation = new Matrix();
			rotation.RotateAt(angle, center);

			var halfWidth = size.Width * 0.5f;
			var halfHeight = size.Height * 0.5f;

			var points = new[]
			{
				new PointF(center.X + halfWidth, center.Y + halfHeight),
				new PointF(center.X - halfWidth, center.Y + halfHeight),
				new PointF(center.X - halfWidth, center.Y - halfHeight),
				new PointF(center.X + halfWidth, center.Y - halfHeight),
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

		private Polygon GetCollisionBox(PointF topLeft, SizeF size)
		{
			var xPlus = Config.Envelope.MinX / SphericalMercator.Resolution(Config.ZoomLevel);
			var yPlus = -Config.Envelope.MaxY / SphericalMercator.Resolution(Config.ZoomLevel);

			var poly = new Polygon(new LinearRing(new[]
			{
				new Coordinate(xPlus + topLeft.X, yPlus + topLeft.Y),
				new Coordinate(xPlus + topLeft.X + size.Width, yPlus + topLeft.Y),
				new Coordinate(xPlus + topLeft.X + size.Width, yPlus + topLeft.Y + size.Height),
				new Coordinate(xPlus + topLeft.X, yPlus + topLeft.Y + size.Height),
				new Coordinate(xPlus + topLeft.X, yPlus + topLeft.Y) //TODO: Just pass the first one twice
			}));
			return poly;
		}
	}
}
