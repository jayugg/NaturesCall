using System.Collections.Generic;
using NaturesCall.Util;
using ProtoBuf;
using Vintagestory.API.Common;

namespace NaturesCall.Config;

public class ConfigServer : IModConfig
{
    public string ThirstModId { get; set; }
    public bool EnableBladder { get; set; } = true;
    public int BladderCapacity { get; set; } = 1500;
    public float BladderCapacityOverload { get; set; } = 0.25f;
    public float BladderWalkSpeedDebuff { get; set; } = 0.5f;
    public float UrineNutrientChance { get; set; } = 0.1f;
    public float UrineDrainRate { get; set; } = 3f;
    public bool UrineStains { get; set; } = true;
    public bool SpillWashStains { get; set; } = true;
    
    // Advanced settings
    public Dictionary<EnumSoilNutrient, float> UrineNutrientLevels { get; set; } = Constants.UrineNutrientLevels;
    public List<EnumBlockMaterial> UrineStainableMaterials { get; set; } = Constants.UrineStainableMaterials;
    
    // Compatibility
    public float ElephantBladderCapacityMultiplier { get; set; } = 1/2f;

    public ConfigServer(ICoreAPI api, ConfigServer previousConfigServer = null)
    {
        if (previousConfigServer == null)
        {
            return;
        }
        ThirstModId = previousConfigServer.ThirstModId;
        EnableBladder = previousConfigServer.EnableBladder;
        BladderCapacity = previousConfigServer.BladderCapacity;
        BladderCapacityOverload = previousConfigServer.BladderCapacityOverload;
        BladderWalkSpeedDebuff = previousConfigServer.BladderWalkSpeedDebuff;
        UrineNutrientChance = previousConfigServer.UrineNutrientChance;
        UrineDrainRate = previousConfigServer.UrineDrainRate;
        UrineStains = previousConfigServer.UrineStains;
        SpillWashStains = previousConfigServer.SpillWashStains;
        UrineNutrientLevels = previousConfigServer.UrineNutrientLevels;
        UrineStainableMaterials = previousConfigServer.UrineStainableMaterials;
        ElephantBladderCapacityMultiplier = previousConfigServer.ElephantBladderCapacityMultiplier;
    }
}