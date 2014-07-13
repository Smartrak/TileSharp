using System.Collections.Generic;
using System.Linq;
using GeoAPI.Geometries;
using NetTopologySuite.Index.Quadtree;

namespace TileSharp.LabelOverlapPreventers
{
	public class PerfectQuadtreePreventer : ILabelOverlapPreventer
	{
		private readonly List<Quadtree<LabelDetails>> _quadtrees = new List<Quadtree<LabelDetails>>();

		public PerfectQuadtreePreventer()
		{
			for (var i = 0; i < 20; i++)
				_quadtrees.Add(new Quadtree<LabelDetails>());
		}

		public bool CanPlaceLabel(TileConfig tile, LabelDetails label)
		{
			var qt = _quadtrees[tile.ZoomLevel];

			lock (qt)
			{
				var potentials = qt.Query(label.Outline.EnvelopeInternal);
				foreach (var x in potentials)
				{
					//If we collide with ourself, we can be placed
					if (x.Outline.Intersects(label.Outline))
					{
						if (x.Text == label.Text)
							return true;
						return x.Text == label.Text;
					}
				}
				qt.Insert(label.Outline.EnvelopeInternal, label);
			}
			return true;
		}
	}
}
