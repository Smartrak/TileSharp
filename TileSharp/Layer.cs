namespace TileSharp
{
	public class Layer
	{
		public readonly IDataSource DataSource;
		public readonly LayerType Type;

		public int? MaxZoom { get; private set; }

		protected Layer(IDataSource dataSource, LayerType type)
		{
			DataSource = dataSource;
			Type = type;
		}

		public Layer SetMaxZoom(int maxZoom)
		{
			MaxZoom = maxZoom;
			return this;
		}
	}
}