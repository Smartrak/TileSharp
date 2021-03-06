﻿using System;
using NetTopologySuite.Features;

namespace TileSharp
{
	public class FeatureFilter
	{
		/// <summary>
		/// Render the feature if this returns true
		/// </summary>
		public readonly Func<Feature, bool> AcceptanceFilter;

		public FeatureFilter(Func<Feature, bool> acceptanceFilter)
		{
			AcceptanceFilter = acceptanceFilter;
		}

		public static FeatureFilter StringAttrEquals(string attrName, string value)
		{
			return new FeatureFilter(f => (string)f.Attributes[attrName] == value);
		}
	}
}
