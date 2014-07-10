﻿using GeoAPI.Geometries;

namespace TileSharp
{
	public class TileConfig
	{
		/// <summary>
		/// The Width/Height of the tile image in pixels
		/// </summary>
		public readonly int PixelSize;

		/// <summary>
		/// Bounds of the tile
		/// </summary>
		public readonly Envelope Envelope;

		public readonly int ZoomLevel;

		/// <summary>
		/// Layers and rendering settings for the tile
		/// </summary>
		public readonly LayerConfig LayerConfig;

		public TileConfig(int pixelSize, int zoomLevel, Envelope envelope, LayerConfig layerConfig)
		{
			PixelSize = pixelSize;
			ZoomLevel = zoomLevel;
			Envelope = envelope;
			LayerConfig = layerConfig;
		}
	}
}
