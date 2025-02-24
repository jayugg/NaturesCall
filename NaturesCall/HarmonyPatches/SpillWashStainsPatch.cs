using HarmonyLib;
using NaturesCall.Config;
using NaturesCall.Util;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
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
        world.BlockAccessor.GetBlockEntity(blockSel.Position.AddCopy(blockSel.Face))?.GetBehavior<BEBehaviorBurning>()?.KillFire(false);
        world.BlockAccessor.GetBlockEntity(blockSel.Position)?.GetBehavior<BEBehaviorBurning>()?.KillFire(false);
        if (!ConfigSystem.ConfigServer.SpillWashStains) return;
        Vec3i voxelPos = new Vec3i();
        for (int index1 = -2; index1 < 2; ++index1)
        {
            for (int index2 = -2; index2 < 2; ++index2)
            {
                for (int index3 = -2; index3 < 2; ++index3)
                {
                    int num3 = (int) (blockSel.HitPosition.X * 16.0);
                    int num4 = (int) (blockSel.HitPosition.Y * 16.0);
                    int num5 = (int) (blockSel.HitPosition.Z * 16.0);
                    if (num3 + index1 >= 0 && num3 + index1 <= 15 && num4 + index2 >= 0 && num4 + index2 <= 15 && num5 + index3 >= 0 && num5 + index3 <= 15)
                    {
                        voxelPos.Set(num3 + index1, num4 + index2, num5 + index3);
                        int subPosition = CollectibleBehaviorArtPigment.BlockSelectionToSubPosition(blockSel.Face, voxelPos);
                        if (world.BlockAccessor.GetDecor(blockSel.Position, subPosition)?.FirstCodePart() == "caveart")
                            world.BlockAccessor.BreakDecor(blockSel.Position, blockSel.Face, new int?(subPosition));
                    }
                }
            }
        }
    }
}