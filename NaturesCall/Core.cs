using HarmonyLib;
using NaturesCall.Bladder;
using NaturesCall.BlockEntities;
using NaturesCall.Config;
using NaturesCall.Hud;
using NaturesCall.Util;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;

namespace NaturesCall;

public class Core : ModSystem
{
    public static ILogger Logger;
    public static string Modid;
    private static ICoreAPI _api;
    public static Harmony HarmonyInstance;
    
    public override void StartPre(ICoreAPI api)
    {
        _api = api;
        Modid = Mod.Info.ModID;
        Logger = Mod.Logger;
        ConfigSystem.LoadConfig(api);
        if (api.ModLoader.IsModEnabled("configlib"))
        {
            _ = new ConfigLibCompat(api);
        }
    }

    public override void Start(ICoreAPI api)
    {
        api.RegisterEntityBehaviorClass($"{Modid}:bladder", typeof(EntityBehaviorBladder));
        api.RegisterBlockEntityClass($"{Modid}.{nameof(BlockEntityStain)}", typeof(BlockEntityStain));
        Patch();
    }
    
    public override void StartServerSide(ICoreServerAPI sapi)
    {
        sapi.Event.OnEntitySpawn += AddEntityBehaviors;
        sapi.Event.OnEntityLoaded += AddEntityBehaviors;
        sapi.Event.PlayerJoin += (player) => OnPlayerJoin(player.Entity);
        sapi.Event.RegisterEventBusListener(OnConfigReloaded, filterByEventName:EventIds.ConfigReloaded);
        Commands.Register(sapi);
        Logger.StoryEvent("Nature calls...");
        Logger.Event("Loaded server side");
    }

    public override void StartClientSide(ICoreClientAPI capi)
    {
        capi.Gui.RegisterDialog(new ThirstBarHudElement(capi));
        Commands.RegisterClient(capi);
        Logger.Event("Loaded client side");
    }

    private void OnPlayerJoin(EntityPlayer player)
    {
        Commands.ResetModBoosts(player);
    }
    private void AddEntityBehaviors(Entity entity)
    {
        if (entity is not EntityPlayer) return;
        RemoveEntityBehaviors(entity);
        if (ConfigSystem.ConfigServer.EnableBladder)
            entity.AddBehavior(new EntityBehaviorBladder(entity));
    }
    
    private void RemoveEntityBehaviors(Entity entity)
    {
        if (entity is not EntityPlayer) return;
        if (!ConfigSystem.ConfigServer.EnableBladder && entity.HasBehavior<EntityBehaviorBladder>())
            entity.RemoveBehavior(entity.GetBehavior<EntityBehaviorBladder>());
    }
    
    private void OnConfigReloaded(string eventname, ref EnumHandling handling, IAttribute data)
    {
        foreach (IPlayer player in _api.World.AllPlayers)
        {
            if (player.Entity == null) continue;
            RemoveEntityBehaviors(player.Entity);
            AddEntityBehaviors(player.Entity);
        }
    }

    private static void Patch()
    {
        if (HarmonyInstance != null) return;
        HarmonyInstance = new Harmony(Modid);
        Logger.VerboseDebug("Patching...");
        if (ConfigSystem.ConfigServer.SpillWashStains)
        {
            HarmonyInstance.PatchCategory(PatchCategories.SpillWashStainsCategory);
            Logger.VerboseDebug("Patched SpillWashStains...");
        }
    }

    private static void Unpatch()
    {
        Logger?.VerboseDebug("Unpatching...");
        HarmonyInstance?.UnpatchAll();
        HarmonyInstance = null;
    }
    
    public override void Dispose()
    {
        Unpatch();
        ConfigSystem.Dispose();
        Logger = null;
        Modid = null;
        _api = null;
        base.Dispose();
    }
}