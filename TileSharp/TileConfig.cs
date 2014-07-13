using GeoAPI.Geometries;

namespace TileSharp
{
	public class TileConfig
	{
		/// <summary>
		/// Bounds of the tile
		/// </summary>
		public readonly Envelope Envelope;

		/// <summary>
		/// Envelope padded to be 3x the size of Envelope in each dimension, fixed labelling errors
		/// </summary>
		public readonly Envelope PaddedEnvelope;

		public readonly int ZoomLevel;
		public readonly int TileX;
		public readonly int TileY;

		/// <summary>
		/// Layers and rendering settings for the tile
		/// </summary>
		public readonly LayerConfig LayerConfig;

		public TileConfig(int z, int x, int y, LayerConfig layerConfig)
		{
			ZoomLevel = z;
			TileX = x;
			TileY = y;

			Envelope = SphericalMercator.GoogleTileBounds(z, x, y);
			LayerConfig = layerConfig;

			PaddedEnvelope = Envelope.Clone();
			PaddedEnvelope.ExpandBy(Envelope.Width, Envelope.Height);
		}
	}
}
