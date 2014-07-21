using GeoAPI.Geometries;
using NetTopologySuite.Features;

namespace TileSharp.LabelOverlapPreventers
{
	public class LabelDetails
	{
		public readonly IPolygon Outline;
		public readonly long UniqueId;

		public LabelDetails(IPolygon outline, Feature feature)
		{
			Outline = outline;
			UniqueId = (long)feature.Attributes["__featureid"];
		}
	}
}
