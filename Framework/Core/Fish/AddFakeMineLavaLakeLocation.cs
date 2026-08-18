using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Extensions;

namespace MineLavaLakeFish.Framework.Core.Fish;

public static class AddFakeMineLavaLakeLocation
{
    public static void AddEvents(IModHelper helper)
    {
        helper.Events.GameLoop.DayStarted += OnDayStarted_RegisterFakeMineLavaPoolLocation;
        helper.Events.GameLoop.DayEnding += OnDayEnding_RemoveFakeLocation;
    }

    /// <summary>
    /// Register fake location for proper fishing handling.
    /// </summary>
    /// <param name="sender">Event sender</param>
    /// <param name="e">Event args, see <see cref="SaveLoadedEventArgs"/></param>
    public static void OnDayStarted_RegisterFakeMineLavaPoolLocation(object? sender, DayStartedEventArgs e)
    {
        // just some small placeholder map
        var mapPath = Path.Combine("Maps", "LeoTreeHouse");
        var location = new GameLocation(mapPath, MineShaftLavaLakePatch.FakeMineLavaLakeLocation);
        Game1.locations.Add(location);
        ModEntry.Instance.Log.Trace($"Loaded {MineShaftLavaLakePatch.FakeMineLavaLakeLocation} to store Mines floor 100 fish data");
    }

    /// <summary>
    /// Remove fake location to stop game from loading fake location from save and displaying warning about unknown location
    /// </summary>
    /// <param name="sender">Event sender</param>
    /// <param name="e">Event args, see <see cref="DayEndingEventArgs"/></param>
    public static void OnDayEnding_RemoveFakeLocation(object? sender, DayEndingEventArgs e)
    {
        Game1.locations.RemoveWhere(loc => loc.Name == MineShaftLavaLakePatch.FakeMineLavaLakeLocation);
    }
}