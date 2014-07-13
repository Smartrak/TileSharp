using GeoAPI.Geometries;

namespace TileSharp.LabelOverlapPreventers
{
	public class LabelDetails
	{
		public readonly IPolygon Outline;
		public readonly string Text;

		public LabelDetails(IPolygon outline, string text)
		{
			Outline = outline;
			Text = text;
		}
	}
}
