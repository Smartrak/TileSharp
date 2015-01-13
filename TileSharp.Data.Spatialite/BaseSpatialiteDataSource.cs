using System;
using System.Collections.Generic;
using System.Data.SQLite;
using GeoAPI.Geometries;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace TileSharp.Data.Spatialite
{
	/// <summary>
	/// Provides base fetching of Features from a Spatialite SQLiteConnection.
	/// The connection must be open (and Spatialite loaded) before Fetch is called
	/// </summary>
	public abstract class BaseSpatialiteDataSource : DataSource, IDisposable
	{
		private readonly string _baseSql;
		private readonly GaiaGeoReader _geoReader;
		private readonly string _geometryColumn;
		private readonly string[] _attributeColumns;

		protected SQLiteConnection Connection;

		/// <summary>
		/// During a fetch we will lock on this object. If you are sharing a connection you'd usually lock on the connection
		/// </summary>
		protected abstract object Lock { get; }

		protected BaseSpatialiteDataSource(SQLiteConnection connection, string tableName, string geometryColumn, string[] attributeColumns = null)
		{
			Connection = connection;

			_geoReader = new GaiaGeoReader();
			_geometryColumn = geometryColumn;
			_attributeColumns = attributeColumns ?? new string[0];

			_baseSql = string.Format(
				"SELECT {0}, (ROWID + {1}) as __featureid {2} " +
				"FROM {3} " +
				"WHERE ROWID IN (SELECT ROWID FROM SpatialIndex WHERE f_table_name='{3}' AND search_frame=ST_GeomFromText(:envelope))", geometryColumn, ((long)DataSourceId << 32), attributeColumns == null ? "" : ", " + string.Join(", ", attributeColumns), tableName);
		}

		public override List<Feature> Fetch(Envelope envelope)
		{
			lock (Lock)
			{
				return TryFetch(envelope);
			}
		}

		private List<Feature> TryFetch(Envelope envelope)
		{
			var res = new List<Feature>();

			using (var comm = Connection.CreateCommand())
			{
				comm.CommandText = _baseSql;
				var polygon = new Polygon(new LinearRing(new[]
				{
					new Coordinate((int)envelope.MinX, (int)envelope.MinY),
					new Coordinate((int)envelope.MaxX, (int)envelope.MinY),
					new Coordinate((int)envelope.MaxX, (int)envelope.MaxY),
					new Coordinate((int)envelope.MinX, (int)envelope.MaxY),
					new Coordinate((int)envelope.MinX, (int)envelope.MinY)
				}));
				comm.Parameters.AddWithValue("envelope", polygon);

				using (var reader = comm.ExecuteReader())
				{
					while (reader.Read())
					{
						var feature = new Feature(_geoReader.Read((byte[])reader[_geometryColumn]), new AttributesTable());
						res.Add(feature);
						foreach (var attr in _attributeColumns)
							feature.Attributes.AddAttribute(attr, reader[attr]);
						feature.Attributes.AddAttribute("__featureid", reader["__featureid"]);
					}
				}
			}

			return res;
		}

		public abstract void Dispose();
	}
}
