using HarmonyLib;
using MineLavaLakeFish.Framework.Core.Buff;
using MineLavaLakeFish.Framework.Core.Fish;
using MineLavaLakeFish.Framework.Data;
using MineLavaLakeFish.Integrations.Gmcm;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace MineLavaLakeFish;

public class ModEntry : Mod
{
    public static ModEntry Instance { get; private set; }
    
    public ModConfig Config { get; private set; }
    
    public ModLog Log { get; private set; }

    public override void Entry(IModHelper helper)
    {
        Instance = this;
        Config = helper.ReadConfig<ModConfig>();
        Log = new ModLog(Monitor);
        
        Harmony harmony = new(ModManifest.UniqueID);
        MineShaftLavaLakePatch.ApplyPatch(harmony, Log);
        FireResistanceBuff.ApplyPatches(harmony, Log);
        
        //helper.Events.GameLoop.
        helper.Events.GameLoop.GameLaunched += OnGameLaunched_AddGmcmIntegration;
        AddFakeMineLavaLakeLocation.AddEvents(helper);
        LavaLakeFishDataAsset.AddEventsForAsset(helper);
    }
    
    private void OnGameLaunched_AddGmcmIntegration(object? sender, GameLaunchedEventArgs e)
    {
        var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenu>("spacechase0.GenericModConfigMenu");
        if (configMenu is null)
            return;

        configMenu.Register(
            mod: ModManifest,
            reset: () => Config = new ModConfig(),
            save: () => Helper.WriteConfig(Config)
        );

        this.AddGmcmOptions(
            configMenu: configMenu
        );
    }
}