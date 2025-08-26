using System;
using NaturesCall.Compatibility;
using NaturesCall.Config;
using NaturesCall.Util;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace NaturesCall.Hud
{
    public class BladderBarHudElement : HudElement
    {
        private const string ComposerKey = "bladderbar";
        private GuiElementOverloadableBar _bladderBar;
        private float _lastBladderLevel;
        private float _lastBladderCapacity;
        private static double[] BladderBarColor => ModGuiStyle.FromHex(ConfigSystem.ConfigClient.BladderBarColor);
        private static double[] OverloadBarColor => ModGuiStyle.FromHex(ConfigSystem.ConfigClient.BladderBarOverloadColor);
        private bool FirstComposed { get; set; }
        
        public BladderBarHudElement(ICoreClientAPI capi) : base(capi)
        {
            capi.Event.RegisterGameTickListener(OnGameTick, 20);
            capi.Event.RegisterGameTickListener(OnFlashStatbars, 2500);
            capi.Event.RegisterEventBusListener(ReloadBars, filterByEventName: EventIds.ConfigReloaded);
        }

        private void ReloadBars(string eventname, ref EnumHandling handling, IAttribute data)
        {
            if (!FirstComposed) return;
            ClearComposers();
            Dispose();
            ComposeGuis();
            UpdateBladderBar(true);
        }

        private void OnGameTick(float dt)
        {
            UpdateBladderBar();
        }
        
        public override void OnOwnPlayerDataReceived()
        {
            ComposeGuis();
            OnGameTick(1);
        }
        
        private void UpdateBladderBar(bool forceReload = false)
        {
            var bladderTree = capi.World.Player.Entity.WatchedAttributes.GetTreeAttribute($"{Core.ModId}:bladder");
            if (bladderTree == null || _bladderBar == null) return;

            var currentLevel = bladderTree.TryGetFloat("currentlevel");
            var capacity = bladderTree.TryGetFloat("capacity");

            if (!currentLevel.HasValue || !capacity.HasValue) return;

            var isLevelChanged = Math.Abs(_lastBladderLevel - currentLevel.Value) >= 0.1;
            var isCapacityChanged = Math.Abs(_lastBladderCapacity - capacity.Value) >= 0.1;

            if (!isLevelChanged && !isCapacityChanged && !forceReload) return;

            _bladderBar.SetLineInterval(100f);
            _bladderBar.SetValues(currentLevel.Value, 0.0f, capacity.Value);

            _lastBladderLevel = currentLevel.Value;
            _lastBladderCapacity = capacity.Value;
        }
        
        private void OnFlashStatbars(float dt)
        {
            var bladderTree  = capi.World.Player.Entity.WatchedAttributes.GetTreeAttribute(Core.ModId+":bladder");

            if (bladderTree == null || _bladderBar == null) return;
            var currentlevel = bladderTree.TryGetFloat("currentlevel");
            var capacity = bladderTree.TryGetFloat("capacity");
            var ratio = currentlevel.HasValue & capacity.HasValue
                ? currentlevel.GetValueOrDefault() / (double)capacity.GetValueOrDefault()
                : new double?();
            if (ratio.GetValueOrDefault() > 1 & ratio.HasValue)
                _bladderBar.ShouldFlash = true;
        }
        
        private void ComposeGuis()
        {
            FirstComposed = true;
            var parentBounds = GenParentBounds();
            var hodLoaded = false || capi.ModLoader.GetModSystem<HoDCompat>()?.IsLoaded == true;
            const double xOffset = -2.0;
            const double yOffset = 10.0;
            var yOffsetHod = yOffset - (hodLoaded ? 7 : 0);
            var bladderBarBounds = ElementStdBounds.Statbar(EnumDialogArea.RightBottom, 850f * 0.41)
                .WithFixedAlignmentOffset(
                    ConfigSystem.ConfigClient.BladderBarX + xOffset,
                    ConfigSystem.ConfigClient.BladderBarY + yOffsetHod);
            bladderBarBounds.WithFixedHeight(6.0);

            var composer = capi.Gui.CreateCompo(ComposerKey, parentBounds.FlatCopy().FixedGrow(0.0, 20.0));
            _bladderBar = new GuiElementOverloadableBar(
                capi,
                bladderBarBounds,
                BladderBarColor,
                OverloadBarColor,
                true,
                true);
            
            composer.BeginChildElements(parentBounds)
                .AddInteractiveElement(_bladderBar, ComposerKey)
                .EndChildElements()
                .Compose();

            _bladderBar.HideWhenLessThan = ConfigSystem.ConfigClient.HideBladderBarAt;
            _bladderBar.HideWhenFull = false;

            Composers[ComposerKey] = composer;
            
            TryOpen();
        }

        private static ElementBounds GenParentBounds()
        {
            return new ElementBounds()
            {
                Alignment = EnumDialogArea.CenterBottom,
                BothSizing = ElementSizing.Fixed,
                fixedWidth = 850f,
                fixedHeight = 25.0
            }.WithFixedAlignmentOffset(0.0, -55.0);
        }

        public override bool TryClose() => false;

        public override bool ShouldReceiveKeyboardEvents() => false;

        public override bool Focusable => false;
    }
}