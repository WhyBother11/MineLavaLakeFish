using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Locations;

namespace MineLavaLakeFish.Data;

public static class LavaLakeFishDataAsset
{
    
    /// <summary>
    /// Data asset path for custom fish support.
    /// </summary>
    public static readonly string MineLavaLakeFishAsset = $"Mods/{ModProperties.UniqueID}/MineLavaLakeFish";
    
    private static Dictionary<string, SpawnFishData>? _data;

    /// <summary>
    /// Stored custom fish data.
    /// </summary>
    public static Dictionary<string, SpawnFishData> MineLavaLakeFishData
    {
        get
        {
            if (_data == null)
            {
                _data = Game1.content.Load<Dictionary<string, SpawnFishData>>(MineLavaLakeFishAsset);
            }
            return _data;
        }
    }

    public static void AddEventsForAsset(IModHelper helper)
    {
        helper.Events.Content.AssetRequested += OnAssetRequested;
        helper.Events.Content.AssetReady += OnAssetReady;
        helper.Events.Content.AssetsInvalidated += OnAssetsInvalidated;
    }

    /// <summary>
    /// Create base floor 100 <see cref="StardewValley.GameData.Locations.SpawnFishData"/> dictionary.
    /// </summary>
    /// <param name="sender">Event sender</param>
    /// <param name="e">Event args, see <see cref="AssetRequestedEventArgs"/></param>
    public static void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (e.NameWithoutLocale.IsEquivalentTo(MineLavaLakeFishAsset))
        {
            e.LoadFrom(() => new Dictionary<string, SpawnFishData>(), AssetLoadPriority.Exclusive);
        }
    }

    /// <summary>
    /// Update floor 100 <see cref="StardewValley.GameData.Locations.SpawnFishData"/> dictionary.
    /// </summary>
    /// <param name="sender">Event sender</param>
    /// <param name="e">Event args, see <see cref="AssetReadyEventArgs"/></param>
    public static void OnAssetReady(object? sender, AssetReadyEventArgs e)
    {
        if (e.NameWithoutLocale.IsEquivalentTo(MineLavaLakeFishAsset))
        {
            _data = Game1.content.Load<Dictionary<string, SpawnFishData>>(MineLavaLakeFishAsset);
        }
    }

    /// <summary>
    /// Invalidate floor 100 <see cref="StardewValley.GameData.Locations.SpawnFishData"/> dictionary.
    /// </summary>
    /// <param name="sender">Event sender</param>
    /// <param name="e">Event args, see <see cref="AssetsInvalidatedEventArgs"/></param>
    public static void OnAssetsInvalidated(object? sender, AssetsInvalidatedEventArgs e)
    {
        foreach (var assetName in e.NamesWithoutLocale)
        {
            if (assetName.IsEquivalentTo(MineLavaLakeFishAsset))
            {
                ModEntry.Instance.Log.Trace($"Invalidated asset {assetName}");
                _data = null;
            }
        }
    }
}