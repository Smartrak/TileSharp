using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TileSharp.Web
{
	class Program
	{
		static void Main(string[] args)
		{
			var listener = new HttpListener();
			listener.Prefixes.Add("http://localhost:10808/");
			listener.Start();
			Console.WriteLine("Listening");

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

			ctx.Response.ContentType = "text";

			ctx.Response.OutputStream.Write(Encoding.UTF8.GetBytes("test"), 0, 4);
			ctx.Response.OutputStream.Close();
		}
	}
}
