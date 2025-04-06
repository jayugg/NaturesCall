using NaturesCall.Bladder;
using NaturesCall.Config;
using Vintagestory.API.Common;
using XLib.XLeveling;

namespace NaturesCall.Compatibility;

public class XSkillsCompat : ModSystem
{
    private ICoreAPI _api;

    public override double ExecuteOrder() => 1.05;
    public override bool ShouldLoad(ICoreAPI api) => api.ModLoader.IsModEnabled("xskills");

    public override void Start(ICoreAPI api)
    {
        _api = api;
        var xLeveling = api.ModLoader.GetModSystem("XLib.XLeveling.XLeveling") as XLeveling;
        var survival = xLeveling?.GetSkill("survival");
        var elephantBladder = new Ability("elephantbladder", Core.ModId+":ability-elephantbladder", Core.ModId+":abilitydesc-elephantbladder", 1, 2, new int[] { 750, 1500 });
        elephantBladder.OnPlayerAbilityTierChanged += OnElephantBladder;
        survival?.AddAbility(elephantBladder);
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