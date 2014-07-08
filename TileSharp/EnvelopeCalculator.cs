using System;
using GeoAPI.Geometries;

namespace TileSharp
{
	public static class EnvelopeCalculator
	{
		public const int TileSize = 256;
		public const double OriginShift = 2 * Math.PI * 6378137 / 2.0;
		public const double InitialResolution = 2 * Math.PI * 6378137 / TileSize;

		// http://www.maptiler.org/google-maps-coordinates-tile-bounds-projection/

		/// <summary>
		/// Resolution (meters/pixel) for given zoom level (measured at Equator)
		/// </summary>
		private static double Resolution(int zoom)
		{
			return InitialResolution / Math.Pow(2, zoom);
		}

		/// <summary>
		/// Converts XY point from Spherical Mercator EPSG:900913 to lat/lon in WGS84 Datum
		/// </summary>
		private static Coordinate MetersToLatLon(double mX, double mY)
		{
			var lon = (mX / OriginShift) * 180.0;
			var lat = (mY / OriginShift) * 180.0;

			lat = 180 / Math.PI * (2 * Math.Atan(Math.Exp(lat * Math.PI / 180.0)) - Math.PI / 2.0);
			return new Coordinate(lon, lat);
		}

		/// <summary>
		/// Converts pixel coordinates in given zoom level of pyramid to EPSG:900913
		/// </summary>
		private static Coordinate PixelsToMeters(int z, int x, int y)
		{
			var res = Resolution(z);
			var mX = x * res - OriginShift;
			var mY = y * res - OriginShift;
			return new Coordinate(mX, mY);
		}

		/// <summary>
		/// Returns bounds of the given tile in EPSG:900913 coordinates
		/// </summary>
		public static Envelope TileBounds(int z, int x, int y)
		{
			var min = PixelsToMeters(z, x * TileSize, y * TileSize);
			var max = PixelsToMeters(z, (x + 1) * TileSize, (y + 1) * TileSize);
			return new Envelope(min.X, max.X, min.Y, max.Y);
		}

		/// <summary>
		/// Returns bounds of the given tile in latutude/longitude using WGS84 datum
		/// </summary>
		public static Envelope TileLatLonBounds(int z, int x, int y)
		{
			var bounds = TileBounds(z, x, y);
			var min = MetersToLatLon(bounds.MinX, bounds.MinY);
			var max = MetersToLatLon(bounds.MaxX, bounds.MaxY);
			return new Envelope(min, max);
		}
	}
}
