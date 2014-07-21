using System;
using System.Collections.Generic;
using System.IO;
using GeoAPI.Geometries;
using GeoJsonSharp;
using NetTopologySuite.Features;

namespace TileSharp.Data.GeoJson
{
	public class GeoJsonDataSource : DataSource
	{
		private readonly FeatureCollection _featureCollection;

		public GeoJsonDataSource(string fileName)
		{
			_featureCollection = new GeoJsonParser(File.ReadAllText(fileName), new ParserSettings { SkipInvalidGeometry = true }).Parse();

			long featureCounter = DataSourceId << 32;
			foreach (var feature in _featureCollection.Features)
			{
				featureCounter++;
				feature.Attributes.AddAttribute("__featureid", featureCounter);
			}
		}

		public override List<Feature> Fetch(Envelope envelope)
		{
			var res = new List<Feature>();

			foreach (var feature in _featureCollection.Features)
			{
				if (feature.Geometry.EnvelopeInternal.Intersects(envelope))
				{
					//Convert Multi-X to X
					if (feature.Geometry is IMultiLineString || feature.Geometry is IMultiPolygon)
					{
						for (var i = 0; i < feature.Geometry.NumGeometries; i++)
							res.Add(new Feature(feature.Geometry.GetGeometryN(i), feature.Attributes));
					}
					else
					{
						res.Add(feature);
					}
				}
			}

			return res;
		}

		/// <summary>
		/// Remove everything that doesn't match the given predicate
		/// </summary>
		public GeoJsonDataSource Where(Predicate<Feature> keepFilter)
		{
			for (var i = _featureCollection.Count - 1; i >= 0; i--)
			{
				if (!keepFilter(_featureCollection.Features[i]))
					_featureCollection.Features.RemoveAt(i);
			}
			return this;
		}
	}
}
