using System.Drawing;

namespace TileSharp.Symbolizers
{
	/// <summary>
	/// https://github.com/mapnik/mapnik/wiki/TextSymbolizer
	/// </summary>
	public class TextSymbolizer : Symbolizer
	{
		public readonly string LabelAttribute;
		public readonly PlacementType Placement;
		public readonly ContentAlignment Alignment;

		public readonly Color TextColor;
		public readonly Color TextHaloColor;

		/// <summary>
		/// The distance between repeated labels.
		/// 0: A single label is placed in the center.
		/// Based on Mapnik Spacing
		/// </summary>
		public readonly int Spacing;

		public TextSymbolizer(string labelAttribute, PlacementType placement, ContentAlignment alignment = ContentAlignment.MiddleCenter, int spacing = 0, Color? textColor = null)
		{
			LabelAttribute = labelAttribute;
			Placement = placement;
			Alignment = alignment;
			Spacing = spacing;

			TextColor = textColor ?? Color.Black;
			TextHaloColor = Color.White;
		}


		public enum PlacementType
		{
			Line,
			Point
		}
	}
}
