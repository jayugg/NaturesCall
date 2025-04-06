using NaturesCall.Config;
using NaturesCall.Util;
using Vintagestory.API.Client;
using Vintagestory.API.Config;

namespace NaturesCall.Hud;

public class HudChoice : GuiDialog
{
    private static CairoFont Font => CairoFont
        .WhiteSmallText()
        .WithLineHeightMultiplier(1.5)
        .WithFontSize((float)GuiStyle.SmallFontSize);
        //.WithOrientation(EnumTextOrientation.Right);

    private static CairoFont ButtonFont => CairoFont
        .SmallButtonText();
    
    public HudChoice(ICoreClientAPI capi) : base(capi)
    {
    }

    public override void OnGuiOpened()
    {
        LoadHud();
        base.OnGuiOpened();
    }

    private void LoadHud()
    {
        var textBounds = ElementBounds.Fixed(0.0, 0.0, 220.0, 50.0);
        var buttonBounds = textBounds.BelowCopy().WithFixedWidth(100).WithFixedHeight(30);
        var overlayBounds = new ElementBounds().WithSizing(ElementSizing.FitToChildren).WithFixedPadding(GuiStyle.ElementToDialogPadding / 2.0);
        overlayBounds.WithChildren(textBounds, buttonBounds);
        var dialogBounds = overlayBounds.ForkBoundingParent().WithAlignment(EnumDialogArea.None).WithAlignment(EnumDialogArea.RightMiddle).WithFixedPosition(0.0, -225.0);
        var pageText = Lang.Get($"{Core.ModId}:hud-choosepeemode");
        SingleComposer?.Dispose();
        SingleComposer = capi.Gui
            .CreateCompo($"{Core.ModId}:hud-choosepeemode", dialogBounds)
            .AddGameOverlay(overlayBounds, GuiStyle.DialogLightBgColor)
            .AddRichtext(pageText, Font, textBounds, "richtext")
            .AddButton(Lang.Get($"{Core.ModId}:peemode-stand"), OnChooseStand, buttonBounds, ButtonFont, EnumButtonStyle.Small, "standbutton")
            .AddButton(Lang.Get($"{Core.ModId}:peemode-sit"), OnChooseSit, buttonBounds.RightCopy(10), ButtonFont, EnumButtonStyle.Small, "sitbutton")
            .Compose();
    }

    private bool OnChooseSit()
    {
        ConfigSystem.ConfigClient.PeeMode = EnumPeeMode.Sit;
        SaveSelectionAndClose();
        return true;
    }

    private bool OnChooseStand()
    {
        ConfigSystem.ConfigClient.PeeMode = EnumPeeMode.Stand;
        SaveSelectionAndClose();
        return true;
    }
    
    private void SaveSelectionAndClose()
    {
        ModConfig.WriteConfig(capi, Constants.ConfigClientName, ConfigSystem.ConfigClient);
        capi.Event.PushEvent(EventIds.ConfigReloaded);
        TryClose();
    }

    public override bool TryOpen()
    {
        LoadHud();
        return base.TryOpen();
    }

    public override EnumDialogType DialogType => EnumDialogType.Dialog;

    public override string ToggleKeyCombinationCode => null;

    public override bool PrefersUngrabbedMouse => true;
}