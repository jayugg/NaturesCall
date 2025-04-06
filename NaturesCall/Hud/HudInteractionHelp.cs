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
      wiUtil = new DrawWorldInteractionUtil(capi, Composers, "-interactHelper");
      wiUtil.UnscaledLineHeight = 25.0;
      wiUtil.FontSize = 16f;
      capi.Event.RegisterEventBusListener(OnEvent, filterByEventName: EventIds.Interaction);
      capi.Event.AfterActiveSlotChanged += OnSlotFilled;
      capi.Event.RegisterGameTickListener(new Action<float>(OnGameTick), 20);
    }
    
    public override void OnOwnPlayerDataReceived() => ComposeGuis();
    
    private void OnEvent(string eventName, ref EnumHandling handling, IAttribute data)
    {
      errorTextActiveMs = capi.InWorldEllapsedMilliseconds;
      if (data?.GetValue() is string interactionId) {
        interactions = new[] { Constants.Interactions[interactionId] };
        wiUtil.ComposeBlockWorldInteractionHelp(interactions);
      }
    }

    private void OnSlotFilled(ActiveSlotChangeEventArgs slot)
    {
      var player = capi.World.Player;
      if (player.InventoryManager.ActiveHotbarSlot == null) return;
      wiUtil.ComposeBlockWorldInteractionHelp(Array.Empty<WorldInteraction>());
    }

    private void OnGameTick(float dt)
    {
      if (errorTextActiveMs == 0L)
        return;
      if (capi.InWorldEllapsedMilliseconds - errorTextActiveMs > durationVisibleMs)
      {
        errorTextActiveMs = 0L;
        wiUtil.ComposeBlockWorldInteractionHelp(Array.Empty<WorldInteraction>());
      }
    }
    
    public override void OnRenderGUI(float deltaTime)
    {
      if (fadeCol.A <= 0.0)
        return;
      float frameWidth = capi.Render.FrameWidth;
      float frameHeight = capi.Render.FrameHeight;
      var bounds = wiUtil.Composer?.Bounds;
      if (bounds != null)
      {
        bounds.Alignment = EnumDialogArea.None;
        bounds.fixedOffsetX = 0.0;
        bounds.fixedOffsetY = 0.0;
        bounds.absFixedX = frameWidth / 2.0 - wiUtil.ActualWidth / 2.0;
        bounds.absFixedY = frameHeight - GuiElement.scaled(95.0) - bounds.OuterHeight;
        bounds.absMarginX = 0.0;
        bounds.absMarginY = 0.0;
      }
      base.OnRenderGUI(deltaTime);
    }

    public void ComposeGuis()
    {
      ClearComposers();
      TryOpen();
    }

    public override bool TryClose() => false;

    public override bool ShouldReceiveKeyboardEvents() => false;

    public override bool ShouldReceiveMouseEvents() => false;

    public override bool Focusable => false;
    
    public override void Dispose()
    {
      base.Dispose();
      wiUtil?.Dispose();
    }
    
  }