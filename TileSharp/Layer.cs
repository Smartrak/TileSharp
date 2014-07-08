namespace TileSharp
{
	public class Layer
	{
		public readonly IDataSource DataSource;
		public readonly LayerType Type;

		public Layer(IDataSource dataSource, LayerType type)
		{
			DataSource = dataSource;
			Type = type;
		}
	}
}