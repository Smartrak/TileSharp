using System.Collections.Generic;
using GeoAPI.Geometries;

namespace TileSharp
{
	public interface IDataSource
	{
		List<IGeometry> Fetch(Envelope envelope);
	}
}