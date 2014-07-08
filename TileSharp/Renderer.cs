﻿using System;
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
			var spanX = _config.Envelope.MaxX - _config.Envelope.MinX;
			var spanY = _config.Envelope.MaxY - _config.Envelope.MinY;

			var res = new PointF[coords.Length];
			for (var i = 0; i < coords.Length; i++)
			{
				var c = coords[i];
				res[i] = new PointF(
					(float)((c.X - _config.Envelope.MinX) * EnvelopeCalculator.TileSize / spanX),
					(float)((c.Y - _config.Envelope.MinY) * EnvelopeCalculator.TileSize / spanY)
					);
			}
			return res;
		}

		private void RenderLine(LineLayer layer, List<IGeometry> data)
		{
			//TODO: cache this
			var pen = new Pen(layer.StrokeStyle.Color, layer.StrokeStyle.Thickness);
			pen.DashPattern = layer.StrokeStyle.DashPattern;

			foreach (var line in data.Cast<ILineString>())
			{
				var points = Project(line.Coordinates);
				_graphics.DrawLines(pen, points);
			}
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
				var points = Project(polygon.Coordinates);

				if (brush != null)
					_graphics.FillPolygon(brush, points);

				if (pen != null)
					_graphics.DrawLines(pen, points);
			}
			//throw new NotImplementedException();
		}
	}
}
