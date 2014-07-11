using TileSharp.Styles;

namespace TileSharp.Layers
{
	public class LineLabelLayer : Layer
	{
		public readonly string LabelAttribute;
		public readonly LineLabelStyle LabelStyle;

		public LineLabelLayer(IDataSource dataSource, string labelAttribute, LineLabelStyle labelStyle)
			: base(dataSource, LayerType.LineLabel)
		{
			LabelAttribute = labelAttribute;
			LabelStyle = labelStyle;
		}
	}
}
