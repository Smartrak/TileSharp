namespace TileSharp
{
	public class Layer
	{
		public readonly IDataSource DataSource;
		public readonly LayerType Type;

		protected Layer(IDataSource dataSource, LayerType type)
		{
			DataSource = dataSource;
			Type = type;
		}
	}
}