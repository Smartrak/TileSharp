using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Features;

namespace TileSharp
{
	public interface IDataSource
	{
		List<Feature> Fetch(Envelope envelope);
	}
}