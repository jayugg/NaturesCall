using Vintagestory.API.MathTools;

namespace NaturesCall.Hud;

public static class ModGuiStyle
{
    public static readonly double[] ThirstBarColor = new[]
    {
        0.2078431397676468,
        0.3137255012989044,
        0.43921568989753723,
        1.0
    };
    
    public static readonly double[] BladderBarColor = new[]
    {
        98 / 255.0,
        190 / 255.0,
        193 / 255.0,
        1.0
    };
    
    public static readonly double[] ThirstBarColor2 = new[]
    {
        38 / 255.0,
        70 / 255.0,
        83 / 255.0,
        1.0
    };
    public static readonly double[] ThirstBarColor3 = new[]
    {
        98 / 255.0,
        190 / 255.0,
        193 / 255.0,
        1.0
    };
    
    public static string ToHex(this double[] rgba)
    {
        return ColorUtil.Doubles2Hex(rgba);
    }
    
    public static double[] FromHex(string hex)
    {
        return ColorUtil.Hex2Doubles(hex);
    }
}
