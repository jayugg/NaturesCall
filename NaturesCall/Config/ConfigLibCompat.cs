using ConfigLib;
using ImGuiNET;
using NaturesCall.Util;
using NaturesCall.Util.StatModifier;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace NaturesCall.Config;

// From https://github.com/Craluminum-Mods/DanaTweaks/
public partial class ConfigLibCompat
{
    private const string settingPrefix = "naturescall:Config.Setting.";
    private const string settingsSimple = "naturescall:Config.SettingsSimple";
    private const string settingsAdvanced = "naturescall:Config.SettingsAdvanced";
    private const string settingsCompat = "naturescall:Config.SettingsCompat";
    private const string textSupportsHex = "naturescall:Config.Text.SupportsHex";

    public ConfigLibCompat(ICoreAPI api)
    {
        if (api.Side == EnumAppSide.Server || api is ICoreClientAPI { IsSinglePlayer: true })
            api.ModLoader.GetModSystem<ConfigLibModSystem>().RegisterCustomConfig(Lang.Get($"{Core.Modid}:{Core.Modid}"), (id, buttons) => EditConfigServer(id, buttons, api));
        if (api.Side == EnumAppSide.Client)
            api.ModLoader.GetModSystem<ConfigLibModSystem>().RegisterCustomConfig(Lang.Get($"{Core.Modid}:{Core.Modid}_client"), (id, buttons) => EditConfigClient(id, buttons, api));
    }
    
    private void EditConfigClient(string id, ControlButtons buttons, ICoreAPI api)
    {
        if (buttons.Save) ModConfig.WriteConfig(api, Constants.ConfigClientName, ConfigSystem.ConfigClient);
        if (buttons.Restore) ConfigSystem.ConfigClient = ModConfig.ReadConfig<ConfigClient>(api, Constants.ConfigClientName);
        if (buttons.Reload) api.Event.PushEvent(EventIds.ConfigReloaded);
        if (buttons.Defaults) ConfigSystem.ConfigClient = new(api);
        BuildSettingsClient(ConfigSystem.ConfigClient, id);
    }
    
    private void BuildSettingsClient(ConfigClient config, string id)
    {
        config.PeeMode = OnInputEnum(id, config.PeeMode, nameof(config.PeeMode));
        config.BladderBarX = OnInputFloat(id, config.BladderBarX, nameof(config.BladderBarX), -float.MaxValue);
        config.BladderBarY = OnInputFloat(id, config.BladderBarY, nameof(config.BladderBarY), -float.MaxValue);
        config.HideBladderBarAt = OnInputFloat(id, config.HideBladderBarAt, nameof(config.HideBladderBarAt));
        config.BladderBarColor = OnInputHex(id, config.BladderBarColor, nameof(config.BladderBarColor));
        config.UrineColor = OnInputText(id, config.UrineColor, nameof(config.UrineColor));
    }

    private void EditConfigServer(string id, ControlButtons buttons, ICoreAPI api)
    {
        if (buttons.Save) ModConfig.WriteConfig(api, Constants.ConfigServerName, ConfigSystem.ConfigServer);
        if (buttons.Restore) ConfigSystem.ConfigServer = ModConfig.ReadConfig<ConfigServer>(api, Constants.ConfigServerName);
        if (buttons.Defaults) ConfigSystem.ConfigServer = new(api);
        BuildSettingsServer(ConfigSystem.ConfigServer, id);
    }
    
    private void BuildSettingsServer(ConfigServer config, string id)
    {
        if (ImGui.CollapsingHeader(Lang.Get(settingsSimple) + $"##settingSimple-{id}"))
        {
            ImGui.Indent(); 
            config.EnableBladder = OnCheckBox(id, config.EnableBladder, nameof(config.EnableBladder));
            config.SpillWashStains = OnCheckBox(id, config.SpillWashStains, nameof(config.SpillWashStains));
            config.UrineStains = OnCheckBox(id, config.UrineStains, nameof(config.UrineStains));
            config.BladderWalkSpeedDebuff = OnInputFloat(id, config.BladderWalkSpeedDebuff, nameof(config.BladderWalkSpeedDebuff));
            config.BladderCapacityOverload = OnInputFloat(id, config.BladderCapacityOverload, nameof(config.BladderCapacityOverload));
            config.UrineNutrientChance = OnInputFloat(id, config.UrineNutrientChance, nameof(config.UrineNutrientChance));
            config.UrineDrainRate = OnInputFloat(id, config.UrineDrainRate, nameof(config.UrineDrainRate));
            DisplayEnumFloatDictionary(config.UrineNutrientLevels, nameof(config.UrineNutrientLevels), id);
            ImGui.Unindent();
        }
        if (ImGui.CollapsingHeader(Lang.Get(settingsAdvanced) + $"##settingAdvanced-{id}"))
        {
            ImGui.Indent(); 
            if (ImGui.CollapsingHeader(Lang.Get(settingPrefix + nameof(config.UrineStainableMaterials)) + $"##settingUrineStainableMaterials"))
            {
                config.UrineStainableMaterials = OnInputList(id, config.UrineStainableMaterials, nameof(config.UrineStainableMaterials));
            }
            ImGui.Unindent();
        }
        if (ImGui.CollapsingHeader(Lang.Get(settingsCompat) + $"##settingCompat-{id}"))
        {
            ImGui.Indent(); 
            config.ElephantBladderCapacityMultiplier = OnInputFloat(id, config.ElephantBladderCapacityMultiplier, nameof(config.ElephantBladderCapacityMultiplier));
            ImGui.Unindent();
        }
    }
}