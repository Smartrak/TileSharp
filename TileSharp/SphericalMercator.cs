using System;
using GeoAPI.Geometries;

namespace TileSharp
{
	public static class SphericalMercator
	{
		public const int TileSize = 256;
		public const double OriginShift = 2 * Math.PI * 6378137 / 2.0;
		public const double InitialResolution = 2 * Math.PI * 6378137 / TileSize;

		// http://www.maptiler.org/google-maps-coordinates-tile-bounds-projection/
		// http://stackoverflow.com/questions/12896139/geographic-coordinates-converter

		/// <summary>
		/// Resolution (meters/pixel) for given zoom level (measured at Equator)
		/// </summary>
		public static double Resolution(int zoom)
		{
			return InitialResolution / Math.Pow(2, zoom);
		}

		/// <summary>
		/// Converts XY point from Spherical Mercator EPSG:900913 to lat/lon in WGS84 Datum
		/// </summary>
		public static Coordinate MetersToLatLon(double mX, double mY)
		{
			var lon = (mX / OriginShift) * 180.0;
			var lat = (mY / OriginShift) * 180.0;

			lat = 180 / Math.PI * (2 * Math.Atan(Math.Exp(lat * Math.PI / 180.0)) - Math.PI / 2.0);
			return new Coordinate(lon, lat);
		}
		
		/// <summary>
		/// Converts WGS84 lat/lon to Spherical Mercator EPSG:900913
		/// </summary>
		public static Coordinate LatLonToMeters(double lat, double lon)
		{
			var x = lon * OriginShift / 180;
	
			var y = Math.Log(Math.Tan((90 + lat) * Math.PI / 360)) / (Math.PI / 180);
			y = y * OriginShift / 180;

			return new Coordinate(x, y);
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
		/// Converts TMS tile coordinates to the epsg:900913 bounding envelope of the given tile
		/// </summary>
		private static Envelope TileBounds(int z, int x, int y)
		{
			var min = PixelsToMeters(z, x * TileSize, y * TileSize);
			var max = PixelsToMeters(z, (x + 1) * TileSize, (y + 1) * TileSize);
			return new Envelope(min.X, max.X, min.Y, max.Y);
		}

		/// <summary>
		/// Converts Google tile coordinates to the epsg:900913 bounding envelope of the given tile
		/// </summary>
		public static Envelope GoogleTileBounds(int z, int x, int y)
		{
			//coordinate origin is moved from bottom-left to top-left corner of the extent
			return TileBounds(z, x, (int)Math.Pow(2, z) - 1 - y);
		}
	}
}

/*
 * This code is taken from http://www.maptiler.org/google-maps-coordinates-tile-bounds-projection/
 * Here is their licence:
###############################################################################
# Copyright (c) 2008 Klokan Petr Pridal. All rights reserved.
#
# Permission is hereby granted, free of charge, to any person obtaining a
# copy of this software and associated documentation files (the "Software"),
# to deal in the Software without restriction, including without limitation
# the rights to use, copy, modify, merge, publish, distribute, sublicense,
# and/or sell copies of the Software, and to permit persons to whom the
# Software is furnished to do so, subject to the following conditions:
#
# The above copyright notice and this permission notice shall be included
# in all copies or substantial portions of the Software.
#
# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
# OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
# FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
# THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
# LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
# FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
# DEALINGS IN THE SOFTWARE.
###############################################################################
*/