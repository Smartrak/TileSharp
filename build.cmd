msbuild /p:Configuration=Release
.\.nuget\NuGet.exe pack TileSharp\TileSharp.csproj -Prop Configuration=Release
.\.nuget\NuGet.exe pack TileSharp.Data.Spatialite\TileSharp.Data.Spatialite.csproj -Prop Configuration=Release -Exclude **\SQLite.Interop.dll -Exclude **\SpatialiteSharp.dll
.\.nuget\NuGet.exe pack TileSharp.LruCache\TileSharp.LruCache.csproj -Prop Configuration=Release
