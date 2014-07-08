using TileSharp.Styles;

namespace TileSharp.Layers
{
	public class PolygonLayer : Layer
	{
		public readonly FillStyle FillStyle;
		public readonly StrokeStyle StrokeStyle;

		public PolygonLayer(IDataSource dataSource, FillStyle fillStyle, StrokeStyle strokeStyle)
			: base(dataSource, LayerType.Polygon)
		{
			FillStyle = fillStyle;
			StrokeStyle = strokeStyle;
		}
	}
}
