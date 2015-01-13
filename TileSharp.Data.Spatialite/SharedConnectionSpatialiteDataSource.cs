using System.Data.SQLite;

namespace TileSharp.Data.Spatialite
{
	/// <summary>
	/// A BaseSpatialiteDataSource that uses an existing connection.
	/// </summary>
	public class SharedConnectionSpatialiteDataSource : BaseSpatialiteDataSource
	{
		public SharedConnectionSpatialiteDataSource(SQLiteConnection connection, string tableName, string geometryColumn, string[] attributeColumns = null)
			: base(connection, tableName, geometryColumn, attributeColumns)
		{
		}

		protected override object Lock
		{
			get { return Connection; }
		}

		public override void Dispose()
		{
			//We don't own the connection
			Connection = null;
		}
	}
}
