using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using TileSharp.Layers;
using TileSharp.Styles;

namespace TileSharp.Web
{
	class Program
	{
		private static LayerConfig _layerConfig;
		public static readonly Random Random = new Random();

		static void Main(string[] args)
		{
			var listener = new HttpListener();
			listener.Prefixes.Add("http://localhost:10808/");
			listener.Start();
			Console.WriteLine("Listening");

			_layerConfig = new LayerConfig(Color.Cyan, new List<Layer>
			{
				new PolygonLayer(new RandomPolygonDataSource(), new FillStyle(Color.Cornsilk), new StrokeStyle(Color.Red, 5)),
				new LineLayer(new RandomLineDataSource(), new StrokeStyle(Color.Blue, 3, new []{ 4.0f, 4.0f }))
			});

			while (true)
			{
				var ctx = listener.GetContext();
				ProcessRequest(ctx);
			}
		}

		private static void ProcessRequest(HttpListenerContext ctx)
		{
			//context.Request.HttpMethod + " " + context.Request.Url

			Console.WriteLine(ctx.Request.Url);

			if (!ctx.Request.RawUrl.EndsWith(".png"))
			{
				ctx.Response.OutputStream.Close();
				return;
			}

			var split = ctx.Request.Url.AbsolutePath.Split(new[] { '.', '/' }, StringSplitOptions.RemoveEmptyEntries);
			var z = int.Parse(split[0]);
			var x = int.Parse(split[1]);
			var y = int.Parse(split[2]);
			var bounds = SphericalMercator.GoogleTileBounds(z, x, y);

			var renderer = new Renderer();
			var tile = renderer.GenerateTile(new TileConfig(256, bounds, _layerConfig));

			ctx.Response.ContentType = "image/png";
			tile.Save(ctx.Response.OutputStream, ImageFormat.Png);

			try
			{
				ctx.Response.OutputStream.Close();
			}
			catch (Exception ex)
			{
				Console.WriteLine("Failed to close connection: " + ex);
			}
		}
	}

	internal class RandomLineDataSource : IDataSource
	{
		public List<IGeometry> Fetch(Envelope envelope)
		{
			var res = new List<IGeometry>();

			var xDif = envelope.MaxX - envelope.MinX;
			var yDif = envelope.MaxY - envelope.MinY;
			for (var i = 0; i < 10; i++)
			{
				List<Coordinate> coords = new List<Coordinate>();
				for (var j = 0; j < 10; j++)
				{
					coords.Add(new Coordinate(envelope.MinX + xDif * Program.Random.NextDouble(), envelope.MinY + yDif * Program.Random.NextDouble()));
				}
				res.Add(new LineString(coords.ToArray()));
			}

			return res;
		}
	}

	internal class RandomPolygonDataSource : IDataSource
	{
		public List<IGeometry> Fetch(Envelope envelope)
		{
			var res = new List<IGeometry>();

			var xDif = envelope.MaxX - envelope.MinX;
			var yDif = envelope.MaxY - envelope.MinY;
			for (var i = 0; i < 4; i++)
			{
				List<Coordinate> coords = new List<Coordinate>();
				for (var j = 0; j < 10; j++)
				{
					coords.Add(new Coordinate(envelope.MinX + xDif * Program.Random.NextDouble(), envelope.MinY + yDif * Program.Random.NextDouble()));
				}
				coords.Add(coords[0]);
				res.Add(new Polygon(new LinearRing(coords.ToArray())));
			}

			return res;
		}
	}
}
