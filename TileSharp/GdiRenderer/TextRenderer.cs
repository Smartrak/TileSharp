using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeoAPI.Geometries;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using TileSharp.LabelOverlapPreventers;
using TileSharp.Symbolizers;

namespace TileSharp.GdiRenderer
{
	class TextRenderer : RendererPart
	{
		public TextRenderer(Renderer renderer) : base(renderer)
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
			var pen = new Pen(Color.White, 3);
			pen.LineJoin = LineJoin.Round;
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

			var xPlus = Config.Envelope.MinX / SphericalMercator.Resolution(Config.ZoomLevel);
			var yPlus = -Config.Envelope.MaxY / SphericalMercator.Resolution(Config.ZoomLevel);

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

			var poly = new Polygon(new LinearRing(new[]
			{
				new Coordinate(xPlus + coord.X, yPlus + coord.Y),
				new Coordinate(xPlus + coord.X + size.Width, yPlus + coord.Y),
				new Coordinate(xPlus + coord.X + size.Width, yPlus + coord.Y + ascent),
				new Coordinate(xPlus + coord.X, yPlus + coord.Y + ascent),
				new Coordinate(xPlus + coord.X, yPlus + coord.Y) //TODO: Just pass the first one twice
			}));

			if (!LabelOverlapPreventer.CanPlaceLabel(Config, new LabelDetails(poly, str)))
				return;

			using (var path = new GraphicsPath())
			{
				path.AddString(str, FontFamily.GenericSansSerif, (int)FontStyle.Bold, emSize, coord, new StringFormat());

				Graphics.DrawPath(pen, path);
				Graphics.FillPath(Brushes.Black, path);
			}
			Graphics.SmoothingMode = SmoothingMode.Default;
		}

		private void RenderLineLabel(TextSymbolizer textSymbolizer, Feature feature)
		{
			var emSize = Graphics.DpiY * fontSize / 72;

			//TODO: Cache
			var pen = new Pen(Color.White, 3);
			pen.LineJoin = LineJoin.Round;
			//ref http://msdn.microsoft.com/en-us/library/xwf9s90b(v=vs.110).aspx
			var font = new Font(FontFamily.GenericSansSerif, fontSize, FontStyle.Bold);
			var ascent = emSize * FontFamily.GenericSansSerif.GetCellAscent(FontStyle.Bold) / FontFamily.GenericSansSerif.GetEmHeight(FontStyle.Bold);

			Graphics.SmoothingMode = SmoothingMode.HighQuality;

			//TODO labels could be not strings
			var str = feature.Attributes.Exists(textSymbolizer.LabelAttribute) ? feature.Attributes[textSymbolizer.LabelAttribute] as string : null;
			if (string.IsNullOrWhiteSpace(str))
				return;

			var labelSize = Graphics.MeasureString(str, font);
			float spacing = textSymbolizer.Spacing;

			var coords = ProjectToCoordinate(feature.Geometry.Coordinates);
			var coordsAsLine = new LineString(coords);
			var lengthIndexed = new NetTopologySuite.LinearReferencing.LengthIndexedLine(coordsAsLine);

			var labelCount = (int)((coordsAsLine.Length - spacing) / (labelSize.Width + spacing));
			if (labelCount < 1 || textSymbolizer.Spacing == 0)
				labelCount = 1;

			//work out spacing based on the amount of labels we'll be putting on
			spacing = ((float)coordsAsLine.Length / labelCount) - labelSize.Width;

			for (var i = 0; i < labelCount; i++)
			{
				var labelCenterLength = (spacing + labelSize.Width) * (0.5f + i);
				var subLine = lengthIndexed.ExtractLine(labelCenterLength - (labelSize.Width / 2), labelCenterLength + (labelSize.Width / 2));
				if (subLine.Coordinates.Length < 2)
					continue;

				var firstCoord = subLine.Coordinates[0];
				var lastCoord = subLine.Coordinates[subLine.Coordinates.Length -1];
				var middleOfLabelLinePoint = lengthIndexed.ExtractPoint(labelCenterLength);


				var midPoint = new PointF((float)(firstCoord.X + lastCoord.X + middleOfLabelLinePoint.X + middleOfLabelLinePoint.X) * 0.25f, (float)(firstCoord.Y + lastCoord.Y + middleOfLabelLinePoint.Y + middleOfLabelLinePoint.Y) * 0.25f);

				var angle = (float)(Math.Atan2(lastCoord.Y - firstCoord.Y, lastCoord.X - firstCoord.X) * 180 / Math.PI);
				//Keep the text up the right way
				if (angle > 90)
					angle -= 180;
				if (angle < -90)
					angle += 180;

				var topLeft = new PointF(-labelSize.Width / 2, -ascent / 2);

				using (var path = new GraphicsPath())
				{
					path.AddString(str, FontFamily.GenericSansSerif, (int)FontStyle.Bold, emSize, topLeft, new StringFormat());

					//path.Transform
					Graphics.TranslateTransform(midPoint.X, midPoint.Y);
					Graphics.RotateTransform(angle);
					{
						Graphics.DrawPath(pen, path);
						Graphics.FillPath(Brushes.Black, path);
					}
					Graphics.ResetTransform();
				}
			}
			Graphics.SmoothingMode = SmoothingMode.Default;
		}
	}
}
