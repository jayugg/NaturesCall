using JetBrains.Annotations;
using NaturesCall.Config;
using NaturesCall.Hud;
using NaturesCall.Util;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;

namespace NaturesCall.Network;

[UsedImplicitly]
public partial class BladderNetwork : ModSystem
{
    public override double ExecuteOrder() => 2.02;

    #region Client

    private HudChoice _choiceHud;
    private IClientNetworkChannel _clientChannel;
    private ICoreClientAPI _capi;
    private const int InteractionHelpBufferMs = 2000;
    private const int PeeActionMs = 250;
    private static SimpleParticleProperties _waterParticles;
    private long _lastPeeTime;
    private long? _lastPeeMessageTime;
    public override void StartClientSide(ICoreClientAPI api)
    {
        _capi = api;
        _clientChannel =
            api.Network.RegisterChannel(Core.ModId + ".pee")
                .RegisterMessageType(typeof(PeeMessage.Request))
                .RegisterMessageType(typeof(PeeMessage.Response))
                .SetMessageHandler<PeeMessage.Response>(OnServerPeeMessage);

        api.Input.InWorldAction += OnEntityAction;
        api.Event.AfterActiveSlotChanged += _ => OnClientTick(0);
        api.Event.LevelFinalize += Event_LevelFinalize;
        api.World.RegisterGameTickListener(OnClientTick, 200);
        api.Gui.RegisterDialog(new HudInteractionHelp(api));
        api.Input.RegisterHotKey(Constants.PeeKeyCode, Lang.Get(Constants.PeeKeyCode), GlKeys.Unknown);
        api.Input.SetHotKeyHandler(Constants.PeeKeyCode, OnPeeKeyPressed);
    }
    
    private void Event_LevelFinalize()
    {
        _choiceHud = new HudChoice(_capi);
    }

    private void OnClientTick(float dt)
    {
        var player = _capi.World.Player;
        if (!(player.IsBladderOverloaded() ||
              _capi.World.ElapsedMilliseconds - _lastPeeTime < InteractionHelpBufferMs ) ||
            !player.Entity.RightHandItemSlot.Empty) return;
        var interactionId = GetInteractionId(player.Entity.Controls);
        if (interactionId != null)
        {
            _capi.Event.PushEvent(EventIds.Interaction, new StringAttribute(interactionId));
        }
        if (ConfigSystem.ConfigClient.PeeMode != EnumPeeMode.None) return;
        _choiceHud.TryOpen();
        player.IngameError(player, "peemodenotset", Lang.Get(Core.ModId+":peemodenotset") );
    }

    [CanBeNull]
    private static string GetInteractionId(EntityControls controls)
    {
        // Could probably simplify further but this works -.-
        if (controls.TriesToMove) return null;
        if (ConfigSystem.ConfigClient.PeeMode.IsSitting())
        {
            if (ConfigSystem.ConfigClient.OnlyPeeWithHotkey)
            {
                return controls.FloorSitting ?
                    Constants.InteractionIds.PeeHotKey :
                    Constants.InteractionIds.PeeHotKeySit;
            }
            return controls.FloorSitting ?
                Constants.InteractionIds.Pee :
                Constants.InteractionIds.PeeSit;
        }
        if (ConfigSystem.ConfigClient.PeeMode.IsStanding())
        {
            if (ConfigSystem.ConfigClient.OnlyPeeWithHotkey)
                return Constants.InteractionIds.PeeHotKey;
            return controls.CtrlKey ?
                Constants.InteractionIds.Pee :
                Constants.InteractionIds.PeeStand;
        }
        return null;
    }

    #endregion
    
    #region Server

    private IServerNetworkChannel _serverChannel;

    public override void StartServerSide(ICoreServerAPI api)
    {
        _serverChannel =
            api.Network.RegisterChannel(Core.ModId + ".pee")
                .RegisterMessageType(typeof(PeeMessage.Request))
                .RegisterMessageType(typeof(PeeMessage.Response))
                .SetMessageHandler<PeeMessage.Request>(HandlePeeAction);
    }
    
    #endregion

    public override void Dispose()
    {
        _choiceHud?.Dispose();
        base.Dispose();
    }
}