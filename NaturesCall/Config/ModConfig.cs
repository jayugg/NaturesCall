using System;
using System.IO;
using Vintagestory.API.Common;

namespace NaturesCall.Config;

// From https://github.com/Craluminum-Mods/DanaTweaks/
public static class ModConfig
{
    public static T ReadConfig<T>(ICoreAPI api, string jsonConfig) where T : IModConfig
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
            else
            {
                GenerateConfig(api, jsonConfig, config);
            }
        }
        catch
        {
            GenerateConfig<T>(api, jsonConfig);
            config = LoadConfig<T>(api, jsonConfig);
        }

        return config;
    }

    public static void WriteConfig<T>(ICoreAPI api, string jsonConfig, T config) where T : IModConfig
    {
        GenerateConfig(api, jsonConfig, config);
    }

    private static T LoadConfig<T>(ICoreAPI api, string jsonConfig) where T : IModConfig
    {
        return api.LoadModConfig<T>(jsonConfig);
    }

    private static void GenerateConfig<T>(ICoreAPI api, string jsonConfig, T previousConfig = null) where T : IModConfig
    {
        api.StoreModConfig(CloneConfig<T>(api, previousConfig), jsonConfig);
    }

    private static T CloneConfig<T>(ICoreAPI api, T config = null) where T : IModConfig
    {
        return (T)Activator.CreateInstance(typeof(T), new object[] { api, config });
    }
}