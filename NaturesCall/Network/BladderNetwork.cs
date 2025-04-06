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

    public HudChoice ChoiceHud;
    private IClientNetworkChannel _clientChannel;
    private ICoreClientAPI _capi;
    private const int PeeBufferMs = 2000;
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
        api.Input.RegisterHotKey(Constants.PeeKeyCode, Lang.Get(Constants.PeeKeyCode), GlKeys.Unknown, HotkeyType.CharacterControls);
        api.Input.SetHotKeyHandler(Constants.PeeKeyCode, OnPeeKeyPressed);
    }
    
    private void Event_LevelFinalize()
    {
        ChoiceHud = new HudChoice(_capi);
    }

    private void OnClientTick(float dt)
    {
        var player = _capi.World.Player;
        if (!(player.IsBladderOverloaded() || _capi.World.ElapsedMilliseconds - _lastPeeTime < PeeBufferMs ) || !player.Entity.RightHandItemSlot.Empty) return;
        if (ConfigSystem.ConfigClient.PeeMode.IsSitting())
            _capi.Event.PushEvent(EventIds.Interaction,
                new StringAttribute(player.Entity.Controls.FloorSitting ?
                    Constants.InteractionIds.Pee : Constants.InteractionIds.PeeSit));

        if (ConfigSystem.ConfigClient.PeeMode.IsStanding())
            _capi.Event.PushEvent(EventIds.Interaction,
                new StringAttribute(!player.Entity.Controls.TriesToMove && player.Entity.Controls.CtrlKey ?
                    Constants.InteractionIds.Pee : Constants.InteractionIds.PeeStand));

        if (ConfigSystem.ConfigClient.PeeMode != EnumPeeMode.None) return;
        ChoiceHud.TryOpen();
        player.IngameError(player, "peemodenotset", Lang.Get(Core.ModId+":peemodenotset") );
    }
    
    private void OnEntityAction(EnumEntityAction action, bool on, ref EnumHandling handled)
    {
        if (action != EnumEntityAction.InWorldRightMouseDown)
        {
            _lastPeeMessageTime = null;
            return;
        }
        var world = _capi.World;
        var player = world.Player.Entity;
        if (((!player.Player.IsBladderOverloaded() && world.ElapsedMilliseconds - _lastPeeTime >= PeeBufferMs) ||
             player.Controls.TriesToMove || !player.Controls.CtrlKey ||
             !player.RightHandItemSlot.Empty ||
             !ConfigSystem.ConfigClient.PeeMode.IsStanding()) &&
            (!player.Controls.FloorSitting ||
             !ConfigSystem.ConfigClient.PeeMode.IsSitting())) return;
        handled = EnumHandling.Handled;
        if (world.ElapsedMilliseconds - _lastPeeTime < PeeActionMs) return;
        var actionMs = world.ElapsedMilliseconds - _lastPeeMessageTime;
        _lastPeeTime = world.ElapsedMilliseconds;
        _lastPeeMessageTime = world.ElapsedMilliseconds;
        if (!actionMs.HasValue)
            return;
        _clientChannel.SendPacket(new PeeMessage.Request()
        {
            Position = player.BlockSelection?.Position,
            HitPostion = player.BlockSelection?.HitPosition,
            Color = ConfigSystem.ConfigClient.UrineColor == "default" ? null : ConfigSystem.ConfigClient.UrineColor,
            ActionMs = actionMs.Value
        });
    }

    #endregion
    
    #region Server

    private IServerNetworkChannel _serverChannel;
    private ICoreServerAPI _sapi;

    public override void StartServerSide(ICoreServerAPI api)
    {
        _sapi = api;
        _serverChannel =
            api.Network.RegisterChannel(Core.ModId + ".pee")
                .RegisterMessageType(typeof(PeeMessage.Request))
                .RegisterMessageType(typeof(PeeMessage.Response))
                .SetMessageHandler<PeeMessage.Request>(HandlePeeAction);
    }
    
    #endregion

    public override void Dispose()
    {
        ChoiceHud?.Dispose();
        base.Dispose();
    }
}