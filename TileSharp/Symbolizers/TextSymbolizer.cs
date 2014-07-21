namespace TileSharp.Symbolizers
{
	/// <summary>
	/// https://github.com/mapnik/mapnik/wiki/TextSymbolizer
	/// </summary>
	public class TextSymbolizer : Symbolizer
	{
		public readonly string LabelAttribute;
		public readonly PlacementType Placement;

		/// <summary>
		/// The distance between repeated labels.
		/// 0: A single label is placed in the center.
		/// Based on Mapnik Spacing
		/// </summary>
		public readonly int Spacing;

		public TextSymbolizer(string labelAttribute, PlacementType placement, int spacing = 0)
		{
			LabelAttribute = labelAttribute;
			Placement = placement;
			Spacing = spacing;
		}


		public enum PlacementType
		{
			Line,
			Point
		}
	}
}
