using System.Collections.Generic;
using NetTopologySuite.Features;

namespace TileSharp
{
	public interface IFeatureCache
	{
		List<Feature> Fetch(TileConfig config, DataSource dataSource);
	}
}
