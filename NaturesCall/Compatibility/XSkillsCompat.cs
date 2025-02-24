using NaturesCall;
using NaturesCall.Bladder;
using NaturesCall.Config;
using Vintagestory.API.Common;
using XLib.XLeveling;

namespace BalancedThirst.Compatibility;

public class XSkillsCompat : ModSystem
{
    private ICoreAPI _api;

    public override double ExecuteOrder() => 1.05;
    public override bool ShouldLoad(ICoreAPI api) => api.ModLoader.IsModEnabled("xskills");

    public override void Start(ICoreAPI api)
    {
        this._api = api;
        XLeveling xLeveling = api.ModLoader.GetModSystem("XLib.XLeveling.XLeveling") as XLeveling;
        Skill survival = xLeveling?.GetSkill("survival");
        Ability elephantBladder = new Ability("elephantbladder", Core.Modid+":ability-elephantbladder", Core.Modid+":abilitydesc-elephantbladder", 1, 2, new int[] { 750, 1500 });
        elephantBladder.OnPlayerAbilityTierChanged += OnElephantBladder;
        survival?.AddAbility(elephantBladder);
    }
    
    public static void OnElephantBladder(PlayerAbility playerAbility, int oldTier)
    {
        IPlayer player = playerAbility.PlayerSkill.PlayerSkillSet.Player;
        if (player == null)
            return;
        EnumAppSide? side = player.Entity?.Api.Side;
        EnumAppSide enumAppSide = EnumAppSide.Server;
        if (!(side.GetValueOrDefault() == enumAppSide & side.HasValue))
            return;
        EntityBehaviorBladder behavior = player.Entity.GetBehavior<EntityBehaviorBladder>();
        if (behavior == null)
            return;
        var factor = 1f + (float) ((double) playerAbility.Value(0)/750)*ConfigSystem.ConfigServer.ElephantBladderCapacityMultiplier;
        behavior.CapacityModifier *= factor;
    }
    
}