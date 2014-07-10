using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeoAPI.Geometries;
using NetTopologySuite.Features;
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
					if (layer.MaxZoom.HasValue && layer.MaxZoom.Value > config.ZoomLevel)
						continue;

					if (!features.ContainsKey(layer.DataSource))
						features.Add(layer.DataSource, layer.DataSource.Fetch(config.Envelope));
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
			//ref http://msdn.microsoft.com/en-us/library/xwf9s90b(v=vs.110).aspx
			//var font = new Font(FontFamily.GenericSansSerif, emSize, FontStyle.Bold);
			var ascent = emSize * FontFamily.GenericSansSerif.GetCellAscent(FontStyle.Bold) / FontFamily.GenericSansSerif.GetEmHeight(FontStyle.Bold);

			//_graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
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
				//_graphics.DrawString(str, new Font("Arial", 12), Brushes.Black, coord.X, coord.Y);
			}
			_graphics.SmoothingMode = SmoothingMode.Default;
			//_graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
		}
	}
}
