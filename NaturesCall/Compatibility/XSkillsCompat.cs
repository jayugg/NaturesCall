using JetBrains.Annotations;
using NaturesCall.Bladder;
using NaturesCall.Config;
using Vintagestory.API.Common;
using XLib.XLeveling;

namespace NaturesCall.Compatibility;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public class XSkillsCompat : ModSystem
{
    public override bool ShouldLoad(ICoreAPI api) => api.ModLoader.IsModEnabled("xlib") 
                                                    || api.ModLoader.IsModEnabled("xlibrabite"); // 1.21 unofficial patch
    public override void Start(ICoreAPI api)
    {
        var xLeveling = api.ModLoader.GetModSystem<XLeveling>();
        var survival = xLeveling?.GetSkill("survival");
        if (survival == null) return;
        var elephantBladder = new Ability(
            "elephantbladder", 
            Core.ModId+":ability-elephantbladder",
            Core.ModId+":abilitydesc-elephantbladder",
            1, 2, [750, 1500]);
        if (api.Side == EnumAppSide.Server) elephantBladder.OnPlayerAbilityTierChanged += OnElephantBladder;
        survival.AddAbility(elephantBladder);
    }
    
    public static void OnElephantBladder(PlayerAbility playerAbility, int oldTier)
    {
        var player = playerAbility.PlayerSkill.PlayerSkillSet.Player;
        if (player == null)
            return;
        var side = player.Entity?.Api.Side;
        var enumAppSide = EnumAppSide.Server;
        if (!(side.GetValueOrDefault() == enumAppSide & side.HasValue))
            return;
        var behavior = player.Entity.GetBehavior<EntityBehaviorBladder>();
        if (behavior == null)
            return;
        var factor = 1f + (float) ((double) playerAbility.Value(0)/750)*ConfigSystem.ConfigServer.ElephantBladderCapacityMultiplier;
        behavior.CapacityModifier *= factor;
    }
    
}