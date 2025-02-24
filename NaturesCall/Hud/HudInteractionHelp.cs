using System;
using System.Collections.Generic;
using NaturesCall.Util;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.Client.NoObf;

namespace NaturesCall.Hud;

public class HudInteractionHelp : HudElement
  {
    private Vec4f fadeCol = new Vec4f(1f, 1f, 1f, 1f);
    private long textActiveMs;
    private long errorTextActiveMs;
    private int durationVisibleMs = 210;
    private Queue<string> messageQueue = new Queue<string>();
    
    private DrawWorldInteractionUtil wiUtil;
    private WorldInteraction[] interactions;
    private double x;
    private double y;

    public override double InputOrder => 1.0;

    public override string ToggleKeyCombinationCode => (string) null;

    public HudInteractionHelp(ICoreClientAPI capi)
      : base(capi)
    {
      this.wiUtil = new DrawWorldInteractionUtil(capi, this.Composers, "-interactHelper");
      this.wiUtil.UnscaledLineHeight = 25.0;
      this.wiUtil.FontSize = 16f;
      capi.Event.RegisterEventBusListener(OnEvent, filterByEventName: EventIds.Interaction);
      capi.Event.AfterActiveSlotChanged += this.OnSlotFilled;
      capi.Event.RegisterGameTickListener(new Action<float>(this.OnGameTick), 20);
    }
    
    public override void OnOwnPlayerDataReceived() => this.ComposeGuis();
    
    private void OnEvent(string eventName, ref EnumHandling handling, IAttribute data)
    {
      this.errorTextActiveMs = this.capi.InWorldEllapsedMilliseconds;
      if (data?.GetValue() is string interactionId) {
        interactions = new[] { Constants.Interactions[interactionId] };
        this.wiUtil.ComposeBlockWorldInteractionHelp(interactions);
      }
    }

    private void OnSlotFilled(ActiveSlotChangeEventArgs slot)
    {
      var player = this.capi.World.Player;
      if (player.InventoryManager.ActiveHotbarSlot == null) return;
      this.wiUtil.ComposeBlockWorldInteractionHelp(Array.Empty<WorldInteraction>());
    }

    private void OnGameTick(float dt)
    {
      if (this.errorTextActiveMs == 0L)
        return;
      if (this.capi.InWorldEllapsedMilliseconds - this.errorTextActiveMs > durationVisibleMs)
      {
        this.errorTextActiveMs = 0L;
        this.wiUtil.ComposeBlockWorldInteractionHelp(Array.Empty<WorldInteraction>());
      }
    }
    
    public override void OnRenderGUI(float deltaTime)
    {
      if (this.fadeCol.A <= 0.0)
        return;
      float frameWidth = this.capi.Render.FrameWidth;
      float frameHeight = this.capi.Render.FrameHeight;
      ElementBounds bounds = this.wiUtil.Composer?.Bounds;
      if (bounds != null)
      {
        bounds.Alignment = EnumDialogArea.None;
        bounds.fixedOffsetX = 0.0;
        bounds.fixedOffsetY = 0.0;
        bounds.absFixedX = frameWidth / 2.0 - this.wiUtil.ActualWidth / 2.0;
        bounds.absFixedY = frameHeight - GuiElement.scaled(95.0) - bounds.OuterHeight;
        bounds.absMarginX = 0.0;
        bounds.absMarginY = 0.0;
      }
      base.OnRenderGUI(deltaTime);
    }

    public void ComposeGuis()
    {
      this.ClearComposers();
      this.TryOpen();
    }

    public override bool TryClose() => false;

    public override bool ShouldReceiveKeyboardEvents() => false;

    public override bool ShouldReceiveMouseEvents() => false;

    public override bool Focusable => false;
    
    public override void Dispose()
    {
      base.Dispose();
      this.wiUtil?.Dispose();
    }
    
  }