using Vintagestory.API.Client;

namespace NaturesCall.Hud;

public class BetterGuiElementStatbar : GuiElementStatbar
{
    public float HideWhenLessThan { get; set; }
    public float MinValue { get; set; }
    public float MaxValue { get; set; }
    public bool Hide { get; set; }
    
    public void SetValues(float value, float min, float max)
    {
        MinValue = min;
        MaxValue = max;
        base.SetValues(value, min, max);
    }
    
    public BetterGuiElementStatbar(ICoreClientAPI capi, ElementBounds bounds, double[] color, bool rightToLeft, bool hideable) : base(capi, bounds, color, rightToLeft, hideable)
    {
    }

    public override void RenderInteractiveElements(float deltaTime)
    {
        if (Hide) return;
        if (GetValue() <= HideWhenLessThan*MaxValue) return;
        base.RenderInteractiveElements(deltaTime);
    }

}