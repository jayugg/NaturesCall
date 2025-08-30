using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace NaturesCall.Util;

public class Constants
{
    public const string ConfigServerName = "NaturesCall/server" + ".json";
    public const string ConfigClientName = "NaturesCall/client" + ".json";
    public const string PeeKeyCode = Core.ModId + ":hotkey-pee";
    
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
        public const string PeeStand = "pee-stand";
        public const string PeeSit = "pee-sit";
        public const string Pee = "pee";
        public const string PeeHotKeySit = "pee-hotkey-sit";
        public const string PeeHotKey = "pee-hotkey";
    }
    
    public static readonly Dictionary<string, WorldInteraction> Interactions = new()
    {
        { 
            InteractionIds.PeeStand, new WorldInteraction
            {
                RequireFreeHand = true,
                ActionLangCode = Core.ModId+":interaction-pee",
                MouseButton = EnumMouseButton.Right,
                HotKeyCode = "ctrl"
            }
        },
        { 
            InteractionIds.PeeSit, new WorldInteraction()
            {
                ActionLangCode = Core.ModId+":interaction-pee",
                MouseButton = EnumMouseButton.Right,
                HotKeyCode = "sitdown"
            }
        },
        { 
            InteractionIds.Pee, new WorldInteraction()
            {
                RequireFreeHand = true,
                ActionLangCode = Core.ModId+":interaction-pee",
                MouseButton = EnumMouseButton.Right
            }
        },
        { 
            InteractionIds.PeeHotKeySit, new WorldInteraction()
            {
                RequireFreeHand = true,
                ActionLangCode = Core.ModId+":interaction-pee",
                MouseButton = EnumMouseButton.None,
                HotKeyCodes = [PeeKeyCode, "sitdown"]
            }
        },
        { 
            InteractionIds.PeeHotKey, new WorldInteraction()
            {
                RequireFreeHand = true,
                ActionLangCode = Core.ModId+":interaction-pee",
                MouseButton = EnumMouseButton.None,
                HotKeyCode = PeeKeyCode
            }
        }
    };
}