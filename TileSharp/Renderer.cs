using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using GeoAPI.Geometries;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using TileSharp.Layers;

namespace TileSharp
{
	/// <summary>
	/// Each renderer is single threaded, but you can have one for each thread.
	/// </summary>
	public class Renderer
	{
		private Graphics _graphics;
		private TileConfig _config;

		public Bitmap GenerateTile(TileConfig config)
		{
			_config = config;
			var features = new Dictionary<IDataSource, List<Feature>>();

			var bitmap = new Bitmap(config.PixelSize, config.PixelSize);
			using (_graphics = Graphics.FromImage(bitmap))
			{
				_graphics.Clear(config.LayerConfig.BackgroundColor);

				foreach (var layer in config.LayerConfig.Layers)
				{
					if (layer.MaxZoom.HasValue && layer.MaxZoom.Value < config.ZoomLevel)
						continue;
					if (layer.MinZoom.HasValue && layer.MinZoom.Value > config.ZoomLevel)
						continue;

					if (!features.ContainsKey(layer.DataSource))
						features.Add(layer.DataSource, layer.DataSource.Fetch(config.PaddedEnvelope));
					var featureList = features[layer.DataSource];

					switch (layer.Type)
					{
						case LayerType.Line:
							RenderLine((LineLayer)layer, featureList);
							break;
						case LayerType.Polygon:
							RenderPolygon((PolygonLayer)layer, featureList);
							break;
						case LayerType.Point:
							RenderPoint((PointLayer)layer, featureList);
							break;
						case LayerType.PointLabel:
							RenderPointLabel((PointLabelLayer)layer, featureList);
							break;
						case LayerType.LineLabel:
							RenderLineLabel((LineLabelLayer)layer, featureList);
							break;
						default:
							throw new NotImplementedException("Don't know how to render layer type " + layer.Type);
					}
				}
			}
			_graphics = null;

			return bitmap;
		}

		private PointF[] Project(Coordinate[] coords)
		{
			//TODO: Could consider simplifying https://github.com/mourner/simplify-js
			//TODO: Clip polygons to map edge?

			var spanX = _config.Envelope.MaxX - _config.Envelope.MinX;
			var spanY = _config.Envelope.MaxY - _config.Envelope.MinY;

			var res = new PointF[coords.Length];
			for (var i = 0; i < coords.Length; i++)
			{
				var c = coords[i];
				res[i] = new PointF(
					(float)((c.X - _config.Envelope.MinX) * SphericalMercator.TileSize / spanX),
					(float)((c.Y - _config.Envelope.MaxY) * SphericalMercator.TileSize / -spanY)
					);
			}
			return res;
		}

		private Coordinate[] ProjectToCoordinate(Coordinate[] coords)
		{
			//TODO: Could consider simplifying https://github.com/mourner/simplify-js
			//TODO: Clip polygons to map edge?

			var spanX = _config.Envelope.MaxX - _config.Envelope.MinX;
			var spanY = _config.Envelope.MaxY - _config.Envelope.MinY;

			var res = new Coordinate[coords.Length];
			for (var i = 0; i < coords.Length; i++)
			{
				var c = coords[i];
				res[i] = new Coordinate(
					((c.X - _config.Envelope.MinX) * SphericalMercator.TileSize / spanX),
					((c.Y - _config.Envelope.MaxY) * SphericalMercator.TileSize / -spanY)
					);
			}
			return res;
		}

		private void RenderLine(LineLayer layer, List<Feature> data)
		{
			//TODO: cache this
			var pen = new Pen(layer.StrokeStyle.Color, layer.StrokeStyle.Thickness);
			if (layer.StrokeStyle.DashPattern != null)
				pen.DashPattern = layer.StrokeStyle.DashPattern;

			foreach (var line in data.Select(x => (ILineString)x.Geometry))
			{
				var points = Project(line.Coordinates);
				_graphics.DrawLines(pen, points);
			}
		}

		private void RenderPoint(PointLayer layer, List<Feature> data)
		{
			//TODO: cache this
			var brush = new SolidBrush(layer.PointStyle.Color);

			var coords = new Coordinate[data.Count];
			for (var i = 0; i < data.Count; i++)
			{
				coords[i] = data[i].Geometry.Coordinate;
			}
			var points = Project(coords);

			var diff = layer.PointStyle.Diameter * 0.5f;
			foreach (var p in points)
				_graphics.FillEllipse(brush, p.X - diff, p.Y - diff, layer.PointStyle.Diameter, layer.PointStyle.Diameter);
		}

		private void RenderPolygon(PolygonLayer layer, List<Feature> data)
		{
			//TODO: cache this
			Brush brush = null;
			if (layer.FillStyle != null)
			{
				brush = new SolidBrush(layer.FillStyle.Color);
			}

			//TODO: cache this
			Pen pen = null;
			if (layer.StrokeStyle != null)
			{
				pen = new Pen(layer.StrokeStyle.Color, layer.StrokeStyle.Thickness);
				if (layer.StrokeStyle.DashPattern != null)
					pen.DashPattern = layer.StrokeStyle.DashPattern;
			}

			foreach (var polygon in data.Select(x => (IPolygon)x.Geometry))
			{
				//TODO: Do we need two version of the code here, or can we just always use a graphics path?
				if (polygon.Holes.Length > 0)
				{
					using (var gp = new GraphicsPath())
					{
						gp.AddPolygon(Project(polygon.ExteriorRing.Coordinates));

						for (var i = 0; i < polygon.Holes.Length; i++)
							gp.AddPolygon(Project(polygon.Holes[i].Coordinates));


						if (brush != null)
							_graphics.FillPath(brush, gp);

						if (pen != null)
							_graphics.DrawPath(pen, gp);
					}
				}
				else
				{
					var points = Project(polygon.Coordinates);

					if (brush != null)
						_graphics.FillPolygon(brush, points);

					if (pen != null)
						_graphics.DrawLines(pen, points);
				}
			}
		}

		private void RenderPointLabel(PointLabelLayer layer, List<Feature> data)
		{
			var emSize = _graphics.DpiY * 14 / 72;

			//TODO: Cache
			var pen = new Pen(Color.White, 3);
			pen.LineJoin = LineJoin.Round;
			//ref http://msdn.microsoft.com/en-us/library/xwf9s90b(v=vs.110).aspx
			var ascent = emSize * FontFamily.GenericSansSerif.GetCellAscent(FontStyle.Bold) / FontFamily.GenericSansSerif.GetEmHeight(FontStyle.Bold);

			_graphics.SmoothingMode = SmoothingMode.HighQuality;

			foreach (var point in data)
			{
				var coord = Project(new[] { point.Geometry.Coordinate })[0];
				//TODO labels could be not strings
				var str = point.Attributes.Exists(layer.LabelAttribute) ? point.Attributes[layer.LabelAttribute] as string : null;
				if (string.IsNullOrWhiteSpace(str))
					continue;

				using (var path = new GraphicsPath())
				{
					path.AddString(str, FontFamily.GenericSansSerif, (int)FontStyle.Bold, emSize, coord + new SizeF(1, -ascent - 1), new StringFormat());

					_graphics.DrawPath(pen, path);
					_graphics.FillPath(Brushes.Black, path);
				}
			}
			_graphics.SmoothingMode = SmoothingMode.Default;
		}

		private void RenderLineLabel(LineLabelLayer layer, List<Feature> data)
		{
			var fontSize = 14;
			var emSize = _graphics.DpiY * fontSize / 72;

			//TODO: Cache
			var pen = new Pen(Color.White, 3);
			pen.LineJoin = LineJoin.Round;
			//ref http://msdn.microsoft.com/en-us/library/xwf9s90b(v=vs.110).aspx
			var font = new Font(FontFamily.GenericSansSerif, fontSize, FontStyle.Bold);
			var ascent = emSize * FontFamily.GenericSansSerif.GetCellAscent(FontStyle.Bold) / FontFamily.GenericSansSerif.GetEmHeight(FontStyle.Bold);

			_graphics.SmoothingMode = SmoothingMode.HighQuality;

			foreach (var feature in data)
			{
				//TODO labels could be not strings
				var str = feature.Attributes.Exists(layer.LabelAttribute) ? feature.Attributes[layer.LabelAttribute] as string : null;
				if (string.IsNullOrWhiteSpace(str))
					continue;

				var labelSize = _graphics.MeasureString(str, font);
				float spacing = layer.LabelStyle.Spacing;

				var coords = ProjectToCoordinate(feature.Geometry.Coordinates);
				var coordsAsLine = new LineString(coords);
				var lengthIndexed = new NetTopologySuite.LinearReferencing.LengthIndexedLine(coordsAsLine);

				var labelCount = (int)((coordsAsLine.Length - spacing) / (labelSize.Width + spacing));
				if (labelCount < 1 || layer.LabelStyle.Spacing == 0)
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
						_graphics.TranslateTransform(midPoint.X, midPoint.Y);
						_graphics.RotateTransform(angle);
						{
							_graphics.DrawPath(pen, path);
							_graphics.FillPath(Brushes.Black, path);
						}
						_graphics.ResetTransform();
					}
				}
			}
			_graphics.SmoothingMode = SmoothingMode.Default;
		}
	}
}
