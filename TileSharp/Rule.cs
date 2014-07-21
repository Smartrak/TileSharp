using TileSharp.Symbolizers;

namespace TileSharp
{
	public class Rule
	{
		public readonly Symbolizer Symbolizer;
		public readonly int? MinZoom;
		public readonly int? MaxZoom;
		public readonly FeatureFilter Filter;

		public Rule(Symbolizer symbolizer, int? minZoom = null, int? maxZoom = null, FeatureFilter filter = null)
		{
			Symbolizer = symbolizer;
			MinZoom = minZoom;
			MaxZoom = maxZoom;
			Filter = filter;
		}
	}
}
