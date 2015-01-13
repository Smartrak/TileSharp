using System.Data.SQLite;

namespace TileSharp.Data.Spatialite
{
	/// <summary>
	/// A BaseSpatialiteDataSource that handles its own connection
	/// </summary>
	public class SpatialiteDataSource : BaseSpatialiteDataSource
	{
		public SpatialiteDataSource(string connectionString, string tableName, string geometryColumn, string[] attributeColumns = null)
			: base(new SQLiteConnection(connectionString), tableName, geometryColumn, attributeColumns)
		{
			Connection.Open();
			SpatialiteSharp.SpatialiteLoader.Load(Connection);
		}

		protected override object Lock
		{
			get { return this; }
		}

		public override void Dispose()
		{
			if (Connection != null)
			{
				Connection.Dispose();
				Connection = null;
			}
		}
	}
}
