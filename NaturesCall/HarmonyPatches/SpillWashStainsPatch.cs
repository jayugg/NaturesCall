using HarmonyLib;
using NaturesCall.Config;
using NaturesCall.Util;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace NaturesCall.HarmonyPatches;

[HarmonyPatch(typeof(BlockLiquidContainerBase),"SpillContents")]
[HarmonyPatchCategory(PatchCategories.SpillWashStainsCategory)]
public class SpillWashStainsPatch
{
    [HarmonyPostfix]
    public static void BlockLiquidContainerBase_SpillContents_Postfix(
        BlockLiquidContainerBase __instance,
        bool __result,
        ItemSlot containerSlot,
        EntityAgent byEntity,
        BlockSelection blockSel)
    {
        if (!__result) return;
        var world = byEntity.World;
        var blockPos = blockSel.Position;
        world.BlockAccessor.GetBlockEntity(blockPos.AddCopy(blockSel.Face))?.GetBehavior<BEBehaviorBurning>()?.KillFire(false);
        world.BlockAccessor.GetBlockEntity(blockPos)?.GetBehavior<BEBehaviorBurning>()?.KillFire(false);
        if (!ConfigSystem.ConfigServer.SpillWashStains) return;
        world.BlockAccessor.BreakDecor(blockSel.Position);
    }
}