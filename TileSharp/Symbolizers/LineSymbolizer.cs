using System.Drawing;

namespace TileSharp.Symbolizers
{
	public class LineSymbolizer : Symbolizer
	{
		public readonly Color Color;
		public readonly float Thickness;

		/// <summary>
		/// null for no dashes. http://msdn.microsoft.com/en-us/library/w34xb12c(v=vs.110).aspx
		/// </summary>
		public readonly float[] DashPattern;

		public LineSymbolizer(Color color, float thickness, float[] dashPattern = null)
		{
			Color = color;
			Thickness = thickness;
			DashPattern = dashPattern;
		}
	}
}
