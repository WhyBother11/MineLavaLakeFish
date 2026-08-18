namespace MineLavaLakeFish.Integrations.Gmcm;

public static class EnableCmcmIntegration
{
    public static void AddGmcmOptions(
        this ModEntry modEntry,
        IGenericModConfigMenu configMenu)
    {
        configMenu.AddNumberOption(
            mod: modEntry.ModManifest,
            name: () => modEntry.Helper.Translation.Get("config.option.lava-lake-fish-chance"),
            tooltip: () => modEntry.Helper.Translation.Get("config.option.lava-lake-fish-chance.tooltip"),
            min: 0.0f,
            max: 1.0f,
            interval: 0.05f,
            getValue: () => modEntry.Config.MineLavaLakeFishChance,
            setValue: value => modEntry.Config.MineLavaLakeFishChance = value
        );
    }
}