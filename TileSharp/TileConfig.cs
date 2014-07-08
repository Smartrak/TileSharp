using GeoAPI.Geometries;

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

		/// <summary>
		/// Layers and rendering settings for the tile
		/// </summary>
		public readonly LayerConfig LayerConfig;

		public TileConfig(int pixelSize, Envelope envelope, LayerConfig layerConfig)
		{
			PixelSize = pixelSize;
			Envelope = envelope;
			LayerConfig = layerConfig;
		}
	}
}
