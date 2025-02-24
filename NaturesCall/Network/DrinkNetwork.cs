using NaturesCall.Config;
using NaturesCall.Hud;
using NaturesCall.Network;
using NaturesCall.Util;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;
using Core = NaturesCall.Core;

namespace BalancedThirst.Systems;

public partial class DrinkNetwork : ModSystem
{
    public override double ExecuteOrder() => 2.02;

    #region Client
    IClientNetworkChannel _clientChannel;
    ICoreClientAPI _capi;
    private const int PeeBufferms = 2000;

    static SimpleParticleProperties _waterParticles;
    
    long _lastPeeTime;
    
    public override void StartClientSide(ICoreClientAPI api)
    {
        _capi = api;
        _clientChannel =
            api.Network.RegisterChannel(Core.Modid + ".pee")
                .RegisterMessageType(typeof(PeeMessage.Request))
                .RegisterMessageType(typeof(PeeMessage.Response))
                .SetMessageHandler<PeeMessage.Response>(OnServerPeeMessage);

        api.Input.InWorldAction += OnEntityAction;
        api.Event.AfterActiveSlotChanged += (slot) => this.OnClientTick(0);
        api.World.RegisterGameTickListener(OnClientTick, 200);
        api.Gui.RegisterDialog(new HudInteractionHelp(api));
        api.Input.RegisterHotKey(Constants.PeeKeyCode, Lang.Get(Constants.PeeKeyCode), GlKeys.Unknown, HotkeyType.CharacterControls);
        api.Input.SetHotKeyHandler(Constants.PeeKeyCode, OnPeeKeyPressed);
    }

    private void OnClientTick(float dt)
    {
        var player = _capi.World.Player;
        if (!ConfigSystem.ConfigServer.EnableBladder) return;
        if (!(player.IsBladderOverloaded() || _capi.World.ElapsedMilliseconds - _lastPeeTime < PeeBufferms ) || !player.Entity.RightHandItemSlot.Empty) return;
        if (ConfigSystem.ConfigClient.PeeMode.IsSitting())
            _capi.Event.PushEvent(EventIds.Interaction,
                new StringAttribute(player.Entity.Controls.FloorSitting ?
                    Constants.InteractionIds.Pee : Constants.InteractionIds.PeeSit));

        if (ConfigSystem.ConfigClient.PeeMode.IsStanding())
            _capi.Event.PushEvent(EventIds.Interaction,
                new StringAttribute(!player.Entity.Controls.TriesToMove && player.Entity.Controls.CtrlKey ?
                    Constants.InteractionIds.Pee : Constants.InteractionIds.PeeStand));

        if (ConfigSystem.ConfigClient.PeeMode == EnumPeeMode.None)
        {
            player.IngameError(player, "peemodenotset", Lang.Get(Core.Modid+":peemodenotset") );
        }
    }
    public void OnEntityAction(EnumEntityAction action, bool on, ref EnumHandling handled)
    {
        if (action != EnumEntityAction.InWorldRightMouseDown)
        {
            return;
        }
        var world = _capi.World;
        EntityPlayer player = world.Player.Entity;
        if (ConfigSystem.ConfigServer.EnableBladder &&
            (player.Player.IsBladderOverloaded() || world.ElapsedMilliseconds - _lastPeeTime < PeeBufferms) && 
            !player.Controls.TriesToMove && player.Controls.CtrlKey &&
            player.RightHandItemSlot.Empty && 
            ConfigSystem.ConfigClient.PeeMode.IsStanding() ||
            (player.Controls.FloorSitting &&
             ConfigSystem.ConfigClient.PeeMode.IsSitting()))
        {
            _lastPeeTime = world.ElapsedMilliseconds;
            _clientChannel.SendPacket(new PeeMessage.Request()
            {
                Position = player.BlockSelection?.Position,
                HitPostion = player.BlockSelection?.HitPosition,
                Color = ConfigSystem.ConfigClient.UrineColor == "default" ? null : ConfigSystem.ConfigClient.UrineColor
            });
            handled = EnumHandling.Handled;
        }
    }
    #endregion
    
    #region Server
    IServerNetworkChannel _serverChannel;
    ICoreServerAPI sapi;

    public override void StartServerSide(ICoreServerAPI api)
    {
        sapi = api;

        _serverChannel =
            api.Network.RegisterChannel(Core.Modid + ".pee")
                .RegisterMessageType(typeof(PeeMessage.Request))
                .RegisterMessageType(typeof(PeeMessage.Response))
                .SetMessageHandler<PeeMessage.Request>(HandlePeeAction);
    }
    
    #endregion
}