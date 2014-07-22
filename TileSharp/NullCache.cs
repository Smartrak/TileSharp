using System.Collections.Generic;
using NetTopologySuite.Features;

namespace TileSharp
{
	public class NullCache : IFeatureCache
	{
		public List<Feature> Fetch(TileConfig config, DataSource dataSource)
		{
			return dataSource.Fetch(config.PaddedEnvelope);
		}
	}
}
