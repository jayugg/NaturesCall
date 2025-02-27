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
        private BetterGuiElementStatbar _bladderBar;
        
        private float _lastBladderLevel;
        private float _lastBladderCapacity;
        
        bool ShouldShowBladderBar => true;
        public double[] BladderBarColor => ModGuiStyle.FromHex(ConfigSystem.ConfigClient.BladderBarColor);
        public bool FirstComposed { get; private set; }
        
        public BladderBarHudElement(ICoreClientAPI capi) : base(capi)
        {
            capi.Event.RegisterGameTickListener(OnGameTick, 20);
            capi.Event.RegisterGameTickListener(this.OnFlashStatbars, 2500);
            capi.Event.RegisterEventBusListener(ReloadBars, filterByEventName: EventIds.ConfigReloaded);
        }

        private void ReloadBars(string eventname, ref EnumHandling handling, IAttribute data)
        {
            if (!FirstComposed) return;
            this.ClearComposers();
            this.Dispose();
            this.ComposeGuis();
            if (ShouldShowBladderBar)
                UpdateBladderBar(true);
        }

        private void OnGameTick(float dt)
        {
            if (ShouldShowBladderBar)
                UpdateBladderBar();
        }
        
        public override void OnOwnPlayerDataReceived()
        {
            ComposeGuis();
            this.OnGameTick(1);
        }
        
        private void UpdateBladderBar(bool forceReload = false)
        {
            var bladderTree = this.capi.World.Player.Entity.WatchedAttributes.GetTreeAttribute(Core.Modid+":bladder");
            if (bladderTree == null || _bladderBar == null) return;

            float? currentLevel = bladderTree.TryGetFloat("currentlevel");
            float? capacity = bladderTree.TryGetFloat("capacity");

            if (!currentLevel.HasValue || !capacity.HasValue) return;

            bool isLevelChanged = Math.Abs(_lastBladderLevel - currentLevel.Value) >= 0.1;
            bool isCapacityChanged = Math.Abs(_lastBladderCapacity - capacity.Value) >= 0.1;

            if (!isLevelChanged && !isCapacityChanged && !forceReload) return;

            _bladderBar.SetLineInterval(100f);
            _bladderBar.SetValues(currentLevel.Value, 0.0f, capacity.Value);

            _lastBladderLevel = currentLevel.Value;
            _lastBladderCapacity = capacity.Value;
        }
        
        private void OnFlashStatbars(float dt)
        {
            var bladderTree  = this.capi.World.Player.Entity.WatchedAttributes.GetTreeAttribute(Core.Modid+":bladder");

            if (bladderTree == null || this._bladderBar == null) return;
            float? currentlevel = bladderTree.TryGetFloat("currentlevel");
            float? capacity = bladderTree.TryGetFloat("capacity");
            double? ratio = currentlevel.HasValue & capacity.HasValue
                ? currentlevel.GetValueOrDefault() / (double)capacity.GetValueOrDefault()
                : new double?();
            if (ratio.GetValueOrDefault() > 1 & ratio.HasValue)
                this._bladderBar.ShouldFlash = true;
        }
        
        private void ComposeGuis()
        {
            FirstComposed = true;
            var num = 850f;
            ElementBounds parentBounds = GenParentBounds();

            if (ShouldShowBladderBar)
            {
                ElementBounds bladderBarBounds = ElementStdBounds.Statbar(EnumDialogArea.RightBottom, num * 0.41)
                    .WithFixedAlignmentOffset(
                        -2.0 + ConfigSystem.ConfigClient.BladderBarX,
                        10 + ConfigSystem.ConfigClient.BladderBarY - (HoDCompat.IsLoaded ? 7 : 0)
                        );
                bladderBarBounds.WithFixedHeight(6.0);

                var compo2 = capi.Gui.CreateCompo("bladderbar", parentBounds.FlatCopy().FixedGrow(0.0, 20.0));

                _bladderBar = new BetterGuiElementStatbar(capi, bladderBarBounds, BladderBarColor, true, true);

                compo2.BeginChildElements(parentBounds)
                    .AddInteractiveElement(_bladderBar, "bladderbar")
                    .EndChildElements()
                    .Compose();
                
                this._bladderBar.HideWhenLessThan = ConfigSystem.ConfigClient.HideBladderBarAt;
                this._bladderBar.Hide = !ShouldShowBladderBar;

                this.Composers["bladderbar"] = compo2;
            }
            
            TryOpen();
        }

        private ElementBounds GenParentBounds()
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