using GeoAPI.Geometries;

namespace TileSharp
{
	public interface IDataSource
	{
		IGeometryCollection Fetch(Envelope envelope);
	}
}