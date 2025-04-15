using Vintagestory.API.MathTools;

namespace NaturesCall.Hud;

public static class ModGuiStyle
{
    public static readonly double[] BladderBarColor =
    [
        98 / 255.0,
        190 / 255.0,
        193 / 255.0,
        1.0
    ];

    public static readonly double[] BladderBarOverloadColor = InvertColor(BladderBarColor);
    
    public static string ToHex(this double[] rgba)
    {
        return ColorUtil.Doubles2Hex(rgba);
    }
    
    public static double[] FromHex(string hex)
    {
        return ColorUtil.Hex2Doubles(hex);
    }
    
    public static double[] InvertColor(double[] rgba)
    {
        return
        [
            1 - rgba[0],
            1 - rgba[1],
            1 - rgba[2],
            rgba[3]
        ];
    }
}
