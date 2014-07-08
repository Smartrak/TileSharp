using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeoAPI.Geometries;

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
			var geometry = new Dictionary<IDataSource, IGeometryCollection>();

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
							RenderLine(layer, data);
							break;
						case LayerType.Polygon:
							RenderPolygon(layer, data);
							break;
						default:
							throw new NotImplementedException("Don't know how to render layer type " + layer.Type);
					}
				}
			}
			_graphics = null;
			
			return bitmap;
		}

		private void RenderLine(Layer layer, IGeometryCollection data)
		{
			throw new NotImplementedException();
		}

		private void RenderPolygon(Layer layer, IGeometryCollection data)
		{
			throw new NotImplementedException();
		}
	}
}
