namespace TileSharp
{
	public class Layer
	{
		public readonly IDataSource DataSource;
		public readonly LayerType Type;

		/// <summary>
		/// The maximum (most zoomed in) level that this layer will show up at
		/// </summary>
		public int? MaxZoom { get; private set; }

		/// <summary>
		/// The minimum (least zoomed in) level that this layer will show up at
		/// </summary>
		public int? MinZoom { get; private set; }

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

		public Layer SetMinZoom(int minZoom)
		{
			MinZoom = minZoom;
			return this;
		}
	}
}