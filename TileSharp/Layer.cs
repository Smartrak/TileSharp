using System;

namespace TileSharp
{
	public class Layer
	{
		public readonly DataSource DataSource;
		public readonly Rule[] Rules;

		//Calculated based on child rules
		public readonly int? MinZoom, MaxZoom;

		public Layer(DataSource dataSource, params Rule[] rules)
		{
			DataSource = dataSource;
			Rules = rules;

			#region work out MinZoom / MaxZoom
			bool allMinZoom = true, allMaxZoom = true;

			foreach (var rule in rules)
			{
				if (rule.MinZoom.HasValue)
				{
					MinZoom = MinZoom.HasValue ? Math.Min(MinZoom.Value, rule.MinZoom.Value) : rule.MinZoom.Value;
				}
				else
				{
					allMinZoom = false;
				}


				if (rule.MaxZoom.HasValue)
				{
					MaxZoom = MaxZoom.HasValue ? Math.Min(MaxZoom.Value, rule.MaxZoom.Value) : rule.MaxZoom.Value;
				}
				else
				{
					allMaxZoom = false;
				}
			}

			if (!allMinZoom)
				MinZoom = null;
			if (!allMaxZoom)
				MaxZoom = null;
			#endregion
		}
	}
}