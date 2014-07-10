using TileSharp.Styles;

namespace TileSharp.Layers
{
	public class PointLabelLayer : Layer
	{
		public readonly string LabelAttribute;
		public readonly PointLabelStyle LabelStyle;

		public PointLabelLayer(IDataSource dataSource, string labelAttribute, PointLabelStyle labelStyle)
			: base(dataSource, LayerType.PointLabel)
		{
			LabelAttribute = labelAttribute;
			LabelStyle = labelStyle;
		}
	}
}
