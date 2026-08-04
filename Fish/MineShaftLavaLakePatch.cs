using HarmonyLib;
using Microsoft.Xna.Framework;
using MineLavaLakeFish.Data;
using StardewValley;
using StardewValley.GameData.Locations;
using StardewValley.Locations;

namespace MineLavaLakeFish.Fish;

public static class MineShaftLavaLakePatch
{
    
    private static ModLog? Log { get; set; }

    /// <summary>
    /// Unique ID for fake location for proper fishing handling.
    /// </summary>
    public static readonly string FakeMineLavaLakeLocation = $"{ModProperties.UniqueID}_MineLavaLake";

    public static void ApplyPatch(Harmony harmony, ModLog log)
    {
        Log = log;
        log.Trace($"Applying Harmony prefix patch {nameof(MineShaft_getFish_Postfix)}");
        harmony.Patch(
            original: AccessTools.Method(typeof(MineShaft), nameof(MineShaft.getFish)),
            postfix: new HarmonyMethod(typeof(MineShaftLavaLakePatch), nameof(MineShaft_getFish_Postfix))
        );
    }

    /// <summary>
    /// Replaces caught trash with custom fish logic
    /// </summary>
    /// <param name="__instance">MineShaft instance</param>
    /// <param name="millisecondsAfterNibble">See <see cref="GameLocation.getFish"/></param>
    /// <param name="bait">See <see cref="GameLocation.getFish"/></param>
    /// <param name="waterDepth">See <see cref="GameLocation.getFish"/></param>
    /// <param name="who">See <see cref="GameLocation.getFish"/></param>
    /// <param name="baitPotency">See <see cref="GameLocation.getFish"/></param>
    /// <param name="bobberTile">See <see cref="GameLocation.getFish"/></param>
    /// <param name="__result">Result of MineShaft::getFish</param>
    public static void MineShaft_getFish_Postfix(
        MineShaft __instance,
        float millisecondsAfterNibble,
        string bait,
        int waterDepth,
        Farmer who,
        double baitPotency,
        Vector2 bobberTile,
        ref Item __result
    )
    {
        if (__instance.getMineArea() == 80 && __result.Category == StardewValley.Object.junkCategory)
        {
            __result = GetLavaLakeCustomFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile);
        }
    }

    /// <summary>
    /// Run custom fish logic if neither lava eel nor cave jelly were caught.
    /// </summary>
    /// <param name="millisecondsAfterNibble">See <see cref="GameLocation.getFish"/></param>
    /// <param name="bait">See <see cref="GameLocation.getFish"/></param>
    /// <param name="waterDepth">See <see cref="GameLocation.getFish"/></param>
    /// <param name="who">See <see cref="GameLocation.getFish"/></param>
    /// <param name="baitPotency">See <see cref="GameLocation.getFish"/></param>
    /// <param name="bobberTile">See <see cref="GameLocation.getFish"/></param>
    /// <returns></returns>
    public static Item GetLavaLakeCustomFish(
        float millisecondsAfterNibble,
        string bait,
        int waterDepth,
        Farmer who,
        double baitPotency,
        Vector2 bobberTile)
    {
        var fishChance = ModEntry.Instance.Config.MineLavaLakeFishChance;
        if (Game1.random.NextDouble() >= fishChance)
        {
            return ItemRegistry.Create("(O)" + Game1.random.Next(167, 173));
        }
        var fakeLocation = Game1.locations.ToList().Find(loc => loc.Name == FakeMineLavaLakeLocation);
        if (fakeLocation == null)
        {
            Log?.Error($"Cannot find {FakeMineLavaLakeLocation} location to load custom fish, defaulting to trash instead.");
            return ItemRegistry.Create("(O)" + Game1.random.Next(167, 173));
        }
        
        var lavaLakeFishData = LavaLakeFishDataAsset.MineLavaLakeFishData.Values.ToList();
        Log?.Trace($"Loaded {lavaLakeFishData.Count} custom fish for Mines floor 100");
        if (Game1.locationData.TryGetValue(FakeMineLavaLakeLocation, out var data)) 
            data.Fish = lavaLakeFishData;
        else
            Game1.locationData[FakeMineLavaLakeLocation] = new LocationData { Fish = lavaLakeFishData};

        return fakeLocation.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile);
    }
}
