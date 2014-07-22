using System;
using System.Collections.Generic;
using CSharpTest.Net.Collections;
using NetTopologySuite.Features;

namespace TileSharp.LruCache
{
	public class LruCache : IFeatureCache
	{
		private readonly LurchTable<Key, List<Feature>> _lurchTable = new LurchTable<Key, List<Feature>>(500);

		public List<Feature> Fetch(TileConfig config, DataSource dataSource)
		{
			//Don't cache high levels
			if (config.ZoomLevel < 12)
				return dataSource.Fetch(config.PaddedEnvelope);

			var key = new Key(dataSource, config);

			return _lurchTable.GetOrAdd(key, delegate
			{
				return dataSource.Fetch(config.PaddedEnvelope);
			});
		}

		private struct Key
		{
			public readonly int DataSourceId, ZoomLevel, TileX, TileY;

			public Key(DataSource dataSource, TileConfig config)
			{
				DataSourceId = dataSource.DataSourceId;
				ZoomLevel = config.ZoomLevel;
				TileX = config.TileX;
				TileY = config.TileY;
			}

			public override int GetHashCode()
			{
				int hash = DataSourceId;

				hash = (hash * 193) + ZoomLevel;
				hash = (hash * 193) + TileX;
				hash = (hash * 193) + TileY;

				return hash;
			}
		}
	}
}
