using HarmonyLib;
using JetBrains.Annotations;
using NaturesCall.Bladder;
using NaturesCall.BlockEntities;
using NaturesCall.Config;
using NaturesCall.Hud;
using NaturesCall.Util;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Server;

namespace NaturesCall;

[UsedImplicitly]
public class Core : ModSystem
{
    public const string ModId = "naturescall";
    private static ICoreAPI _api;
    public static ILogger Logger { get; set; }
    private static Harmony _harmonyInstance;
    
    public override void StartPre(ICoreAPI api)
    {
        _api = api;
        Logger = Mod.Logger;
        ConfigSystem.LoadConfig(api);
        if (api.ModLoader.IsModEnabled("configlib"))
        {
            _ = new ConfigLibCompat(api);
        }
    }

    public override void Start(ICoreAPI api)
    {
        api.RegisterEntityBehaviorClass($"{ModId}:bladder", typeof(EntityBehaviorBladder));
        api.RegisterBlockEntityClass($"{ModId}.{nameof(BlockEntityStain)}", typeof(BlockEntityStain));
        Patch();
    }
    
    public override void StartServerSide(ICoreServerAPI sapi)
    {
        sapi.Event.OnEntitySpawn += AddEntityBehaviors;
        sapi.Event.OnEntityLoaded += AddEntityBehaviors;
        sapi.Event.PlayerJoin += (player) => OnPlayerJoin(player.Entity);
        Commands.Register(sapi);
        Logger.StoryEvent("Nature calls...");
        Logger.Event("Loaded server side");
    }

    public override void StartClientSide(ICoreClientAPI capi)
    {
        capi.Gui.RegisterDialog(new BladderBarHudElement(capi));
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
        entity.AddBehavior(new EntityBehaviorBladder(entity));
    }

    private static void Patch()
    {
        if (_harmonyInstance != null) return;
        _harmonyInstance = new Harmony(ModId);
        Logger.VerboseDebug("Patching...");
        if (!ConfigSystem.ConfigServer.SpillWashStains) return;
        _harmonyInstance.PatchCategory(PatchCategories.SpillWashStainsCategory);
        Logger.VerboseDebug("Patched SpillWashStains...");
    }

    private static void Unpatch()
    {
        Logger?.VerboseDebug("Unpatching...");
        _harmonyInstance?.UnpatchAll();
        _harmonyInstance = null;
    }
    
    public override void Dispose()
    {
        Unpatch();
        ConfigSystem.Dispose();
        Logger = null;
        _api = null;
        base.Dispose();
    }
}