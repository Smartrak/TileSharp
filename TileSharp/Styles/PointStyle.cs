using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TileSharp.Styles
{
	public class PointStyle
	{
		public readonly Color Color;
		public readonly int Diameter;

		public PointStyle(Color color, int diameter)
		{
			Color = color;
			Diameter = diameter;
		}
	}
}
