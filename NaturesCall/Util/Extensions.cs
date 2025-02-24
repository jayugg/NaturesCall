using System;
using System.Collections.Generic;
using System.Reflection;
using NaturesCall.Bladder;
using NaturesCall.Config;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace NaturesCall.Util;

public static class Extensions
{
    public static Vec3d NoY(this Vec3d vec) => new Vec3d(vec.X, 0, vec.Z);
    
    public static void ReceiveCapacity(this Entity entity, float capacity)
    {
        if (!entity.HasBehavior<EntityBehaviorBladder>()) return;
        entity.GetBehavior<EntityBehaviorBladder>().ReceiveFluid(capacity);
    }
    
    public static void IncreaseNutrients(this BlockEntityFarmland be, Dictionary<EnumSoilNutrient, float> addNutrients)
    {
        var nutrientInfo = typeof(BlockEntityFarmland).GetField("nutrients",
            BindingFlags.NonPublic | BindingFlags.Instance);
        if (nutrientInfo?.GetValue(be) is float[] nutrients)
        {
            foreach (var pair in addNutrients)
            {
                switch (pair.Key)
                {
                    case EnumSoilNutrient.N:
                        nutrients[0] += pair.Value;
                        break;
                    case EnumSoilNutrient.P:
                        nutrients[1] += pair.Value;
                        break;
                    case EnumSoilNutrient.K:
                        nutrients[2] += pair.Value;
                        break;
                }
            }
            be.MarkDirty(true);
        }
    }
    
    public static bool IsBladderOverloaded(this IPlayer player)
    {
        var bladderTree = player.Entity.WatchedAttributes.GetTreeAttribute(Core.Modid+":bladder");
        if (bladderTree == null) return false;

        float? currentLevel = bladderTree.TryGetFloat("currentlevel");
        float? capacity = bladderTree.TryGetFloat("capacity");

        if (!currentLevel.HasValue || !capacity.HasValue) return false;
        return currentLevel > capacity;
    }
    
    // From DanaTweaks
    public static void CoolWithWater(this BlockEntityToolMold mold)
    {
        ItemStack stack = mold.MetalContent;
        if (stack != null)
        {
            // No clue why this doesn't work either
            float temperature = stack.Collectible.GetTemperature(mold.Api.World, stack);
            stack.Collectible.SetTemperature(mold.Api.World, stack, Math.Max(20f, temperature - ConfigSystem.ConfigServer.UrineDrainRate));
            mold.Api.World.PlaySoundAt(new AssetLocation("sounds/effect/extinguish2"), mold.Pos.X, mold.Pos.Y, mold.Pos.Z);
        }
    }
    
    // From DanaTweaks
    public static void CoolWithWater(this BlockEntityIngotMold mold)
    {
        ItemStack rightStack = mold.ContentsRight;
        ItemStack leftStack = mold.ContentsLeft;
        if (rightStack != null)
        {
            float temperature = rightStack.Collectible.GetTemperature(mold.Api.World, rightStack);
            rightStack.Collectible.SetTemperature(mold.Api.World, rightStack, Math.Max(20f, temperature - ConfigSystem.ConfigServer.UrineDrainRate));
            mold.Api.World.PlaySoundAt(new AssetLocation("sounds/effect/extinguish1"), mold.Pos.X, mold.Pos.Y, mold.Pos.Z);
        }
        if (leftStack != null)
        {
            float temperature = leftStack.Collectible.GetTemperature(mold.Api.World, leftStack);
            leftStack.Collectible.SetTemperature(mold.Api.World, leftStack, Math.Max(20f, temperature - ConfigSystem.ConfigServer.UrineDrainRate));
            mold.Api.World.PlaySoundAt(new AssetLocation("sounds/effect/extinguish1"), mold.Pos.X, mold.Pos.Y, mold.Pos.Z);
        }
    }
    
    public static void IngameError(this IPlayer byPlayer, object sender, string errorCode, string text)
    {
        (byPlayer.Entity.World.Api as ICoreClientAPI)?.TriggerIngameError(sender, errorCode, text);
    }
}