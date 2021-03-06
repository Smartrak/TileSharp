﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using GeoAPI.Geometries;
using NetTopologySuite.Features;
using TileSharp.Symbolizers;

namespace TileSharp.GdiRenderer
{
	/// <summary>
	/// Each renderer is single threaded, but you can have one for each thread.
	/// </summary>
	public class Renderer
	{
		internal readonly ILabelOverlapPreventer LabelOverlapPreventer;

		internal Graphics Graphics;
		internal TileConfig Config;

		private readonly Dictionary<Type, RendererPart> _renderers;
		private readonly IFeatureCache _featureCache;

		public Renderer(ILabelOverlapPreventer labelOverlapPreventer, IFeatureCache featureCache)
		{
			LabelOverlapPreventer = labelOverlapPreventer;
			_featureCache = featureCache;

			_renderers = new Dictionary<Type, RendererPart>
			{
				{ typeof(LineSymbolizer), new LineRenderer(this) },
				{ typeof(PointSymbolizer), new PointRenderer(this) },
				{ typeof(PolygonSymbolizer), new PolygonRenderer(this) },
				{ typeof(TextSymbolizer), new TextRenderer(this) },
			};
		}

		public Bitmap GenerateTile(TileConfig config)
		{
			Config = config;
			CacheSymbolizers(config.LayerConfig);
			var features = new Dictionary<DataSource, List<Feature>>();

			long fetchingData = 0;
			long rendering = 0;

			var bitmap = new Bitmap(SphericalMercator.TileSize, SphericalMercator.TileSize);
			using (Graphics = Graphics.FromImage(bitmap))
			{
				Graphics.Clear(config.LayerConfig.BackgroundColor);
				Graphics.SmoothingMode = SmoothingMode.HighQuality;

				foreach (var layer in config.LayerConfig.Layers)
				{
					if (layer.MaxZoom.HasValue && layer.MaxZoom.Value < config.ZoomLevel)
						continue;
					if (layer.MinZoom.HasValue && layer.MinZoom.Value > config.ZoomLevel)
						continue;

					if (!features.ContainsKey(layer.DataSource))
					{
						var timer = Stopwatch.StartNew();
						features.Add(layer.DataSource, _featureCache.Fetch(config, layer.DataSource));//.Fetch(config.PaddedEnvelope)));
						fetchingData += timer.ElapsedTicks;
						timer.Stop();
					}
					var featureList = features[layer.DataSource];

					var timer2 = Stopwatch.StartNew();
					foreach (var rule in layer.Rules)
					{
						if (rule.MaxZoom.HasValue && rule.MaxZoom.Value < config.ZoomLevel)
							continue;
						if (rule.MinZoom.HasValue && rule.MinZoom.Value > config.ZoomLevel)
							continue;

						//Can't have a symbolizer render twice at the same time due to gdi resource reuse (You can't use a Brush/Pen on multiple threads at the same time...)
						lock (rule.Symbolizer)
						{
							foreach (var feature in featureList)
							{
								if (rule.Filter != null && !rule.Filter.AcceptanceFilter(feature))
									continue;


								Render(rule.Symbolizer, feature);
							}
						}
					}
					rendering += timer2.ElapsedTicks;
					timer2.Stop();
				}
			}
			Graphics = null;

			//Console.WriteLine("fetching " + fetchingData);
			//Console.WriteLine("rendering " + rendering);
			return bitmap;
		}

		private void CacheSymbolizers(LayerConfig layerConfig)
		{
			if (!layerConfig.RenderersAreCached)
			{
				lock (layerConfig)
				{
					if (!layerConfig.RenderersAreCached)
					{
						foreach (var layer in layerConfig.Layers)
						{
							foreach (var rule in layer.Rules)
							{
								var symbolizer = rule.Symbolizer;
								var type = symbolizer.GetType();
#if DEBUG
								if (!_renderers.ContainsKey(type))
									throw new Exception("Don't know how to render symbolizer " + type);
#endif
								_renderers[type].PreCache(symbolizer);
							}
						}
						layerConfig.RenderersAreCached = true;
					}
				}
			}
		}

		private void Render(Symbolizer symbolizer, Feature feature)
		{
			var type = symbolizer.GetType();
#if DEBUG
			if (!_renderers.ContainsKey(type))
				throw new Exception("Don't know how to render symbolizer " + type);
#endif
			_renderers[type].Render(symbolizer, feature);
		}

		internal PointF[] Project(Coordinate[] coords)
		{
			//TODO: Could consider simplifying https://github.com/mourner/simplify-js
			//TODO: Clip polygons to map edge?

			var spanX = Config.Envelope.MaxX - Config.Envelope.MinX;
			var spanY = Config.Envelope.MaxY - Config.Envelope.MinY;
			var reso = SphericalMercator.Resolution(Config.ZoomLevel);

			var res = new PointF[coords.Length];
			for (var i = 0; i < coords.Length; i++)
			{
				var c = coords[i];
				res[i] = new PointF(
					(float)((c.X - Config.Envelope.MinX) * SphericalMercator.TileSize / spanX),
					(float)((c.Y - Config.Envelope.MaxY) * SphericalMercator.TileSize / -spanY)
					);
			}
			return res;
		}

		internal Coordinate[] ProjectToCoordinate(Coordinate[] coords)
		{
			//TODO: Could consider simplifying https://github.com/mourner/simplify-js
			//TODO: Clip polygons to map edge?

			var spanX = Config.Envelope.MaxX - Config.Envelope.MinX;
			var spanY = Config.Envelope.MaxY - Config.Envelope.MinY;

			var res = new Coordinate[coords.Length];
			for (var i = 0; i < coords.Length; i++)
			{
				var c = coords[i];
				res[i] = new Coordinate(
					((c.X - Config.Envelope.MinX) * SphericalMercator.TileSize / spanX),
					((c.Y - Config.Envelope.MaxY) * SphericalMercator.TileSize / -spanY)
					);
			}
			return res;
		}
	}
}
