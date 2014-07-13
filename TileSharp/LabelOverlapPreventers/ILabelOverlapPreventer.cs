using TileSharp.LabelOverlapPreventers;

namespace TileSharp
{
	/// <summary>
	/// All methods on this class are threadsafe
	/// </summary>
	public interface ILabelOverlapPreventer
	{
		/// <summary>
		/// Returns true if the given polygon (representing the bounding box of a label - a potentially rotated rectangle) can be placed on the grid without intersecting an already placed one at the given zoom level.
		/// If it is successfully placed, this polygon is added to the list of polygons to check against
		/// </summary>
		bool CanPlaceLabel(TileConfig tile, LabelDetails label);
	}
}
