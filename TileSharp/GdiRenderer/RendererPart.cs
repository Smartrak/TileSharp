using System.Drawing;
using GeoAPI.Geometries;
using NetTopologySuite.Features;
using TileSharp.Symbolizers;

namespace TileSharp.GdiRenderer
{
	abstract class RendererPart
	{
		private readonly Renderer _renderer;

		public RendererPart(Renderer renderer)
		{
			_renderer = renderer;
		}

		public abstract void PreCache(Symbolizer symbolizer);

		public abstract void Render(Symbolizer symbolizer, Feature feature);

		#region Pass down to _renderer

		protected Graphics Graphics
		{
			get { return _renderer.Graphics; }
		}

		protected ILabelOverlapPreventer LabelOverlapPreventer
		{
			get { return _renderer.LabelOverlapPreventer; }
		}

		protected TileConfig Config
		{
			get { return _renderer.Config; }
		}

		protected PointF[] Project(Coordinate[] coords)
		{
			return _renderer.Project(coords);
		}

		protected Coordinate[] ProjectToCoordinate(Coordinate[] coords)
		{
			return _renderer.ProjectToCoordinate(coords);
		}

		#endregion
	}
}
