using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeoAPI.Geometries;
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
			var geometry = new Dictionary<IDataSource, List<IGeometry>>();

			var bitmap = new Bitmap(config.PixelSize, config.PixelSize);
			using (_graphics = Graphics.FromImage(bitmap))
			{
				_graphics.Clear(config.LayerConfig.BackgroundColor);

				foreach (var layer in config.LayerConfig.Layers)
				{
					if (!geometry.ContainsKey(layer.DataSource))
						geometry.Add(layer.DataSource, layer.DataSource.Fetch(config.Envelope));
					var data = geometry[layer.DataSource];

					switch (layer.Type)
					{
						case LayerType.Line:
							RenderLine((LineLayer)layer, data);
							break;
						case LayerType.Polygon:
							RenderPolygon((PolygonLayer)layer, data);
							break;
						case LayerType.Point:
							RenderPoint((PointLayer)layer, data);
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

		private void RenderLine(LineLayer layer, List<IGeometry> data)
		{
			//TODO: cache this
			var pen = new Pen(layer.StrokeStyle.Color, layer.StrokeStyle.Thickness);
			if (layer.StrokeStyle.DashPattern != null)
				pen.DashPattern = layer.StrokeStyle.DashPattern;

			foreach (var line in data.Cast<ILineString>())
			{
				var points = Project(line.Coordinates);
				_graphics.DrawLines(pen, points);
			}
		}

		private void RenderPoint(PointLayer layer, List<IGeometry> data)
		{
			//TODO: cache this
			var brush = new SolidBrush(layer.PointStyle.Color);

			var coords = new Coordinate[data.Count];
			for (var i = 0; i < data.Count; i++)
			{
				coords[i] = data[i].Coordinate;
			}
			var points = Project(coords);

			var diff = layer.PointStyle.Diameter * 0.5f;
			foreach (var p in points)
				_graphics.FillEllipse(brush, p.X - diff, p.Y - diff, layer.PointStyle.Diameter, layer.PointStyle.Diameter);
		}

		private void RenderPolygon(PolygonLayer layer, List<IGeometry> data)
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

			foreach (var polygon in data.Cast<IPolygon>())
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
	}
}
