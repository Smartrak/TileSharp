using TileSharp.Styles;

namespace TileSharp.Layers
{
	public class LineLayer : Layer
	{
		public readonly StrokeStyle StrokeStyle;

		public LineLayer(IDataSource dataSource, StrokeStyle style)
			: base(dataSource, LayerType.Line)
		{
			StrokeStyle = style;
		}
	}
}
