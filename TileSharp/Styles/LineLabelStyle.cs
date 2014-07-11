namespace TileSharp.Styles
{
	public class LineLabelStyle
	{
		//https://github.com/mapnik/mapnik/wiki/TextSymbolizer

		/// <summary>
		/// The distance between repeated labels.
		/// 0: A single label is placed in the center.
		/// Based on Mapnik Spacing
		/// </summary>
		public readonly int Spacing;

		public LineLabelStyle(int spacing = 0)
		{
			Spacing = spacing;
		}
	}
}
