using NaturesCall.Util;
using Vintagestory.API.Common;

namespace NaturesCall.Config;
public static class ConfigSystem
{
    public static ConfigServer ConfigServer { get; set; }
    public static ConfigClient ConfigClient { get; set; }

    public static void LoadConfig(ICoreAPI api)
    {
        if (api.Side.IsClient())
        {
            ConfigClient = ModConfig.ReadConfig<ConfigClient>(api, Constants.ConfigClientName);
        }
        else
        {
            ConfigServer = ModConfig.ReadConfig<ConfigServer>(api, Constants.ConfigServerName);
        }
    }
    public static void Dispose()
    {
        ConfigServer = null;
        ConfigClient = null;
    }
}