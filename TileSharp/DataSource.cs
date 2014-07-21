using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Features;

namespace TileSharp
{
	public abstract class DataSource
	{
		protected static int DataSourceIdCounter = 1;
		protected readonly int DataSourceId;

		public abstract List<Feature> Fetch(Envelope envelope);

		protected DataSource()
		{
			DataSourceId = DataSourceIdCounter++;
		}
	}
}