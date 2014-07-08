using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TileSharp.Web
{
	class Program
	{
		private static LayerConfig _layerConfig;

		static void Main(string[] args)
		{
			var listener = new HttpListener();
			listener.Prefixes.Add("http://localhost:10808/");
			listener.Start();
			Console.WriteLine("Listening");

			_layerConfig = new LayerConfig(Color.Cyan, new List<Layer>
			{
				new Layer(new RandomLineDataSource(), LayerType.Line),
				new Layer(new RandomPolygonDataSource(), LayerType.Polygon)
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

			var renderer = new Renderer();
			var tile = renderer.GenerateTile(new TileConfig(256, null/*TODO*/, _layerConfig));

			ctx.Response.ContentType = "image/png";
			tile.Save(ctx.Response.OutputStream, ImageFormat.Png);
			ctx.Response.OutputStream.Close();
		}
	}
}
