using System;
using Cairo;
using NaturesCall.Util;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace NaturesCall.Hud;

public class GuiElementOverloadableBar : GuiElementTextBase
{
    private float _minValue;
    private float _maxValue = 100;
    private float _value = 32;
    private float _lineInterval = 10;

    private readonly double[] _color;
    private readonly double[] _overloadColor;
    private readonly bool _rightToLeft;
    public bool HideWhenFull { get; set; }
    public float HideWhenLessThan { get; set; }
    private bool ShowValueOnHover { get; set; } = true;
    private bool IsOverloaded => _value > _maxValue;

    private LoadedTexture _baseTexture;
    private LoadedTexture _barTexture;
    private LoadedTexture _flashTexture;
    private LoadedTexture _valueTexture;
    private int ValueHeight => (int)Bounds.OuterHeight + 1;
    private int ValueWidth => Bounds.OuterWidthInt + 1;

    public bool ShouldFlash;
    private float _flashTime;
    private bool _valuesSet;
    private readonly bool _hideable;
    private StatbarValueDelegate _onGetStatbarValue;
    private readonly CairoFont _valueFont = CairoFont.WhiteSmallText().WithStroke(ColorUtil.BlackArgbDouble, 0.75);

    /// <summary>
    /// Creates a new stat bar that can be overloaded.
    /// </summary>
    /// <param name="capi">The client API</param>
    /// <param name="bounds">The bounds of the stat bar.</param>
    /// <param name="color">The color of the stat bar.</param>
    /// <param name="overloadColor">The color of the overloaded stat bar.</param>
    /// <param name="rightToLeft">Determines the direction that the bar fills.</param>
    /// <param name="hideable"></param>
    public GuiElementOverloadableBar(ICoreClientAPI capi, ElementBounds bounds, double[] color, double[] overloadColor, bool rightToLeft, bool hideable) : base(capi, "", CairoFont.WhiteDetailText(), bounds)
    {
        _barTexture = new LoadedTexture(capi);
        _flashTexture = new LoadedTexture(capi);
        _valueTexture = new LoadedTexture(capi);

        if (hideable) _baseTexture = new LoadedTexture(capi);

        _hideable = hideable;
        _color = color;
        _overloadColor = overloadColor;
        _rightToLeft = rightToLeft;

        _onGetStatbarValue = () => (float)Math.Round(_value, 1) + " / " + (int)_maxValue;
    }

    public override void ComposeElements(Context ctx, ImageSurface surface)
    {
        Bounds.CalcWorldBounds();
        if (_hideable)
        {
            surface = new ImageSurface(Format.Argb32, ValueWidth, ValueHeight);
            ctx = new Context(surface);
            RoundRectangle(ctx, 0, 0, Bounds.InnerWidth, Bounds.InnerHeight, 1);
            ctx.SetSourceRGBA(0.15, 0.15, 0.15, 1);
            ctx.Fill();
            EmbossRoundRectangleElement(ctx, 0, 0, Bounds.InnerWidth, Bounds.InnerHeight, false, 3, 1);
        } else
        {
            ctx.Operator = Operator.Over; // WTH man, somewhere within this code or within cairo, the main context operator is being changed
            RoundRectangle(ctx, Bounds.drawX, Bounds.drawY, Bounds.InnerWidth, Bounds.InnerHeight, 1);
            ctx.SetSourceRGBA(0.15, 0.15, 0.15, 1);
            ctx.Fill();
            EmbossRoundRectangleElement(ctx, Bounds, false, 3, 1);
        }
        
        if (_valuesSet)
            RecomposeOverlays();

        if (!_hideable) return;
        generateTexture(surface, ref _baseTexture);
        surface.Dispose();
        ctx.Dispose();
    }

    private void RecomposeOverlays()
    {
        TyronThreadPool.QueueTask(() =>
        {
            ComposeValueOverlay();
            ComposeFlashOverlay();
        });
        if (ShowValueOnHover)
        {
            api.Gui.TextTexture.GenOrUpdateTextTexture(_onGetStatbarValue(), _valueFont, ref _valueTexture, new TextBackground
            {
                FillColor = GuiStyle.DialogStrongBgColor,
                Padding = 5,
                BorderWidth = 2
            });
        }
    }
    
    private void ComposeValueOverlay()
    {
        Bounds.CalcWorldBounds();

        var widthRel = (double)_value / (_maxValue - _minValue);
        var overloadWidthRel = (double)(_value - _maxValue) / (_maxValue - _minValue);
        var surface = new ImageSurface(Format.Argb32, ValueWidth, ValueHeight);
        var ctx = new Context(surface);

        if (widthRel > 0.01)
        {
            DrawColorBar(ctx, surface, widthRel, _color);
        }

        if (IsOverloaded && overloadWidthRel > 0.01)
        {
            DrawColorBar(ctx, surface, overloadWidthRel, _overloadColor);
        }
        ctx.SetSourceRGBA(0, 0, 0, 0.5);
        ctx.LineWidth = scaled(2.2);

        var lines = Math.Min(50, (int)((_maxValue - _minValue) / _lineInterval));
        
        for (var i = 1; i < lines; i++)
        {
            ctx.NewPath();
            ctx.SetSourceRGBA(0, 0, 0, 0.5);
            var x = (Bounds.InnerWidth * i) / lines;
            ctx.MoveTo(x, 0);
            ctx.LineTo(x, Math.Max(3, Bounds.InnerHeight - 1));
            ctx.ClosePath();
            ctx.Stroke();
        }

        api.Event.EnqueueMainThreadTask(() =>
        {
            generateTexture(surface, ref _barTexture);
            ctx.Dispose();
            surface.Dispose();
        }, "recompstatbar");
    }

    private void DrawColorBar(Context ctx, ImageSurface surface, double widthRel, double[] color)
    {
        var width = Bounds.OuterWidth * widthRel;
        var x = _rightToLeft ? Bounds.OuterWidth - width : 0;
        RoundRectangle(ctx, x, 0, width, Bounds.OuterHeight, 1);
        ctx.SetSourceRGB(color[0], color[1], color[2]);
        ctx.FillPreserve();
        ctx.SetSourceRGB(color[0] * 0.4, color[1] * 0.4, color[2] * 0.4);
        ctx.LineWidth = scaled(3);
        ctx.StrokePreserve();
        surface.BlurFull(3);
        width = Bounds.InnerWidth * widthRel;
        x = _rightToLeft ? Bounds.InnerWidth - width : 0;
        EmbossRoundRectangleElement(ctx, x, 0, width, Bounds.InnerHeight, false, 2, 1);
    }

    private void ComposeFlashOverlay()
    {
        var surface = new ImageSurface(Format.Argb32, Bounds.OuterWidthInt + 28, Bounds.OuterHeightInt + 28);
        var ctx = new Context(surface);

        ctx.SetSourceRGBA(0,0,0,0);
        ctx.Paint();

        RoundRectangle(ctx, 12, 12, Bounds.OuterWidthInt + 4, Bounds.OuterHeightInt + 4, 1);
        ctx.SetSourceRGB(_color[0], _color[1], _color[2]);
        ctx.FillPreserve();
        surface.BlurFull(3);
        ctx.Fill();
        surface.BlurFull(2);

        RoundRectangle(ctx, 15, 15, Bounds.OuterWidthInt - 2, Bounds.OuterHeightInt - 2, 1);
        ctx.Operator = Operator.Clear;
        ctx.SetSourceRGBA(0, 0, 0, 0);
        ctx.Fill();

        api.Event.EnqueueMainThreadTask(() =>
        {
            generateTexture(surface, ref _flashTexture);
            ctx.Dispose();
            surface.Dispose();
        }, "recompstatbar");
    }

    public override void RenderInteractiveElements(float deltaTime)
    {
        var x = Bounds.renderX;
        var y = Bounds.renderY;
        
        if (_value.NearlyEqual(_maxValue) && HideWhenFull) return;
        if (_value - HideWhenLessThan < 0) return;

        if (_hideable)
            api.Render.RenderTexture(_baseTexture.TextureId, x, y, Bounds.OuterWidthInt + 1, Bounds.OuterHeightInt + 1);

        float alpha = 0;
        if (ShouldFlash)
        {
            _flashTime += 6*deltaTime;
            alpha = GameMath.Sin(_flashTime);
            if (alpha < 0)
            {
                ShouldFlash = false;
                _flashTime = 0;
            }
            if (_flashTime < GameMath.PIHALF)
            {
                alpha = Math.Min(1, alpha * 3);
            }
        }

        if (alpha > 0)
            api.Render.RenderTexture(_flashTexture.TextureId, x - 14, y - 14, Bounds.OuterWidthInt + 28, Bounds.OuterHeightInt + 28, 50, new Vec4f(1.5f, 1, 1, alpha));

        if (_barTexture.TextureId > 0)
            api.Render.RenderTexture(_barTexture.TextureId, x, y, Bounds.OuterWidthInt + 1, ValueHeight);

        if (!ShowValueOnHover || !Bounds.PointInside(api.Input.MouseX, api.Input.MouseY)) return;
        double tx = api.Input.MouseX + 16;
        double ty = api.Input.MouseY + _valueTexture.Height - 4;
        api.Render.RenderTexture(_valueTexture.TextureId, tx, ty, _valueTexture.Width, _valueTexture.Height, 2000);

    }

    /// <summary>
    /// Sets the line interval for the Status Bar.
    /// </summary>
    /// <param name="value">The value to set for the line interval/</param>
    public void SetLineInterval(float value)
    {
        _lineInterval = value;
    }

    /// <summary>
    /// Sets the value for the status bar and updates the bar.
    /// </summary>
    /// <param name="value">The new value of the status bar.</param>
    public void SetValue(float value)
    {
        _value = value;
        _valuesSet = true;
        RecomposeOverlays();
    }

    public float GetValue()
    {
        return _value;
    }

    /// <summary>
    /// Sets the value for the status bar as well as the minimum and maximum values.
    /// </summary>
    /// <param name="value">The new value of the status bar.</param>
    /// <param name="min">The minimum value of the status bar.</param>
    /// <param name="max">The maximum value of the status bar.</param>
    public void SetValues(float value, float min, float max)
    {
        _valuesSet = true;
        _value = value;
        _minValue = min;
        _maxValue = max;
        RecomposeOverlays();
    }

    /// <summary>
    /// Sets the minimum and maximum values of the status bar.
    /// </summary>
    /// <param name="min">The minimum value of the status bar.</param>
    /// <param name="max">The maximum value of the status bar.</param>
    public void SetMinMax(float min, float max)
    {
        _minValue = min;
        _maxValue = max;
        RecomposeOverlays();
    }
    
    public override void Dispose()
    {
        base.Dispose();
        _baseTexture?.Dispose();
        _barTexture.Dispose();
        _flashTexture.Dispose();
        _valueTexture.Dispose();
    }
}