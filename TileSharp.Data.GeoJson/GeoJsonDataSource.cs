using System;
using System.Collections.Generic;
using System.IO;
using GeoAPI.Geometries;
using GeoJsonSharp;

namespace TileSharp.Data.GeoJson
{
	public class GeoJsonDataSource : IDataSource
	{
		private readonly FeatureCollection _featureCollection;

		public GeoJsonDataSource(string fileName)
		{
			_featureCollection = new GeoJsonParser(File.ReadAllText(fileName), new ParserSettings { SkipInvalidGeometry = true }).Parse();
		}

		public List<IGeometry> Fetch(Envelope envelope)
		{
			var res = new List<IGeometry>();

			foreach (var feature in _featureCollection.Features)
			{
				if (feature.Geometry.EnvelopeInternal.Intersects(envelope))
				{
					//Convert Multi-X to X
					if (feature.Geometry is IMultiLineString || feature.Geometry is IMultiPolygon)
					{
						for (var i = 0; i < feature.Geometry.NumGeometries; i++)
							res.Add(feature.Geometry.GetGeometryN(i));
					}
					else
					{
						res.Add(feature.Geometry);
					}
				}
			}

			return res;
		}

		/// <summary>
		/// Remove everything that matches the given predicate
		/// </summary>
		public GeoJsonDataSource ExceptWhere(Predicate<Feature> removeFilter)
		{
			_featureCollection.Features.RemoveAll(removeFilter);
			return this;
		}
	}
}
