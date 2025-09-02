using System;
using System.IO;
using Vintagestory.API.Common;

namespace NaturesCall.Config;

// From https://github.com/Craluminum-Mods/DanaTweaks/
public class ModConfig
{
    public static T ReadConfig<T>(ICoreAPI api, string jsonConfig) where T : ModConfig
    {
        T config;

        try
        {
            config = LoadConfig<T>(api, jsonConfig);
            if (config == null)
            {
                GenerateConfig<T>(api, jsonConfig);
                config = LoadConfig<T>(api, jsonConfig);
            }
        }
        catch
        {
            GenerateConfig<T>(api, jsonConfig);
            config = LoadConfig<T>(api, jsonConfig);
            Core.Logger?.Error($"[{nameof(ReadConfig)}] Error loading mod config from file {jsonConfig}. Regenerating.");
        }
        
        Core.Logger?.Notification($"[{nameof(ReadConfig)}] Successfully loaded config from file {jsonConfig}.");
        return config;
    }

    public static void WriteConfig<T>(ICoreAPI api, string jsonConfig, T config) where T : ModConfig
    {
        GenerateConfig(api, jsonConfig, config);
    }

    private static T LoadConfig<T>(ICoreAPI api, string jsonConfig) where T : ModConfig
    {
        return api.LoadModConfig<T>(jsonConfig);
    }

    private static void GenerateConfig<T>(ICoreAPI api, string jsonConfig, T previousConfig = null) where T : ModConfig
    {
        api.StoreModConfig(CloneConfig(api, previousConfig), jsonConfig);
        if (previousConfig != null) return;
        Core.Logger?.Warning($"[{nameof(GenerateConfig)}] Config file {jsonConfig} does not exist. Generating.");
    }

    private static T CloneConfig<T>(ICoreAPI api, T config = null) where T : ModConfig
    {
        return (T)Activator.CreateInstance(typeof(T), api, config);
    }
}