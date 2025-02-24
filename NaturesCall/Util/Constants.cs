using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace NaturesCall.Util;

public class Constants
{
    public static readonly string ConfigServerName = "NaturesCall/server" + ".json";
    public static readonly string ConfigClientName = "NaturesCall/client" + ".json";
    
    public static readonly List<EnumBlockMaterial> UrineStainableMaterials = new()
    {
        EnumBlockMaterial.Stone,
        EnumBlockMaterial.Soil
    };
    
    public static readonly Dictionary<EnumSoilNutrient, float> UrineNutrientLevels = new()
    {
        { EnumSoilNutrient.N, 0.1f },
        { EnumSoilNutrient.P, 0.0f },
        { EnumSoilNutrient.K, 0.1f }
    };
    
    public struct InteractionIds {
        public const string Drink = "drink";
        public const string PeeStand = "pee-stand";
        public const string PeeSit = "pee-sit";
        public const string Pee = "pee";
    }
    
    public static readonly Dictionary<string, WorldInteraction> Interactions = new()
    {
        { "pee-stand", new WorldInteraction()
            {
                RequireFreeHand = true,
                ActionLangCode = Core.Modid+":interaction-pee",
                MouseButton = EnumMouseButton.Right,
                HotKeyCode = "ctrl"
            }
        },
        { "pee-sit", new WorldInteraction()
            {
                ActionLangCode = Core.Modid+":interaction-pee",
                MouseButton = EnumMouseButton.Right,
                HotKeyCode = "sitdown"
            }
        },
        { "pee", new WorldInteraction()
            {
                RequireFreeHand = true,
                ActionLangCode = Core.Modid+":interaction-pee",
                MouseButton = EnumMouseButton.Right
            }
        }
    };

    public static string PeeKeyCode = Core.Modid + ":hotkey-pee";
}