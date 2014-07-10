using TileSharp.Styles;

namespace TileSharp.Layers
{
	public class PointLayer : Layer
	{
		public readonly PointStyle PointStyle;

		public PointLayer(IDataSource dataSource, PointStyle pointStyle)
			:base(dataSource, LayerType.Point)
		{
			PointStyle = pointStyle;
		}
	}
}
