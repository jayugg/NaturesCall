using NaturesCall.Hud;
using NaturesCall.Util;
using Vintagestory.API.Common;

namespace NaturesCall.Config;

public class ConfigClient : IModConfig
{
    public EnumPeeMode PeeMode { get; set; }
    public float HideBladderBarAt { get; set; } = 0.0f;
    public string BladderBarColor { get; set; } = ModGuiStyle.BladderBarColor.ToHex();
    public string BladderBarOverloadColor { get; set; } = ModGuiStyle.BladderBarOverloadColor.ToHex();
    public float BladderBarX { get; set; }
    public float BladderBarY { get; set; }
    public string UrineColor { get; set; } = "default";
    public bool OnlyPeeWithHotkey { get; set; } = false;
    
    public ConfigClient(ICoreAPI api, ConfigClient previousConfig = null)
    {
        if (previousConfig == null)
            return;
        PeeMode = previousConfig.PeeMode;
        BladderBarColor = previousConfig.BladderBarColor;
        BladderBarOverloadColor = previousConfig.BladderBarOverloadColor;
        HideBladderBarAt = previousConfig.HideBladderBarAt;
        UrineColor = previousConfig.UrineColor;
    }
}