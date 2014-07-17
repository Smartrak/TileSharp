using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using GeoAPI.Geometries;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace TileSharp.Data.Spatialite
{
	public class SpatialiteDataSource : IDataSource, IDisposable
	{
		private readonly string _baseSql;
		private readonly GaiaGeoReader _geoReader;
		private readonly string _geometryColumn;
		private readonly string[] _attributeColumns;

		private SQLiteConnection _conn;

		public SpatialiteDataSource(string connectionString, string tableName, string geometryColumn, string[] attributeColumns = null)
		{
			_conn = new SQLiteConnection(connectionString);
			_conn.Open();
			SpatialiteSharp.SpatialiteLoader.Load(_conn);

			_geoReader = new GaiaGeoReader();
			_geometryColumn = geometryColumn;
			_attributeColumns = attributeColumns ?? new string[0];

			_baseSql = string.Format("SELECT {0}{1} FROM {2} WHERE ST_INTERSECTS ({0}, ST_GeomFromText(:envelope))", geometryColumn, attributeColumns == null ? "" : ", " + string.Join(", ", attributeColumns), tableName);
		}

		public List<Feature> Fetch(Envelope envelope)
		{

			using (var comm = _conn.CreateCommand())
			{
				comm.CommandText = _baseSql;
				comm.Parameters.AddWithValue("envelope", "SRID=900913;" + new Polygon(new LinearRing(new Coordinate[]
				{
					new Coordinate(envelope.MinX, envelope.MinY),
					new Coordinate(envelope.MaxX, envelope.MinY),
					new Coordinate(envelope.MaxX, envelope.MaxY),
					new Coordinate(envelope.MinX, envelope.MaxY),
					new Coordinate(envelope.MinX, envelope.MinY)
				})));

				using (var reader = comm.ExecuteReader())
				{
					var res = new List<Feature>();
					while (reader.Read())
					{
						var feature = new Feature(_geoReader.Read((byte[])reader[_geometryColumn]), new AttributesTable());
						res.Add(feature);
						foreach (var attr in _attributeColumns)
							feature.Attributes.AddAttribute(attr, reader[attr]);
					}

					return res;
				}
			}
		}

		public void Dispose()
		{
			if (_conn != null)
			{
				_conn.Dispose();
				_conn = null;
			}
		}
	}
}
