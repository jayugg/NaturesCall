using System;

namespace NaturesCall.Util.StatModifier;

public static class StatModifierUtil
{
    public static float StatModifierSin(float ratio, float param)
    {
        return param * (float) Math.Sin(0.5*Math.PI*(1f - 2f*ratio));
    }

    public static float StatModifierLinear(float ratio, float param)
    {
        return param * (1f - 2f*ratio);
    }

    public static float StatModifierArcSin(float ratio, float param)
    {
        return param * (float) (2*Math.PI*Math.Asin(1f - 2f*ratio));
    }

    public static float StatModifierCubic(float ratio, float param)
    {
        return param * (float) Math.Pow(1f - 2f*ratio, 3);
    }

    public static float StatModifierICubic(float ratio, float param)
    {
        return - param * (float) Math.Pow(Math.Abs(1f - 2f*ratio), 1.0/3);
    }

    public static float StatModifierQuintic(float ratio, float param)
    {
        return param * (float) Math.Pow(1f - 2f*ratio, 5);
    }

    public static float StatModifierIQuintic(float ratio, float param)
    {
        return -param * (float) Math.Pow(Math.Abs(1f - 2f*ratio), 1.0/5);
    }

    public static float CalcStatModifier(float ratio, float param, EnumCurveType curveTypeType, EnumUpOrDown centering = EnumUpOrDown.Centered)
    {
        var res = curveTypeType switch
        {
            EnumCurveType.Linear => StatModifierLinear(ratio, param),
            EnumCurveType.Sin => StatModifierSin(ratio, param),
            EnumCurveType.Asin => StatModifierArcSin(ratio, param),
            EnumCurveType.Cubic => StatModifierCubic(ratio, param),
            EnumCurveType.InverseCubic => StatModifierICubic(ratio, param),
            EnumCurveType.Quintic => StatModifierQuintic(ratio, param),
            EnumCurveType.InverseQuintic => StatModifierIQuintic(ratio, param),
            EnumCurveType.Flat0 => 0,
            EnumCurveType.Flat1 => param * Math.Sign(0.5f - ratio),
            _ => throw new ArgumentOutOfRangeException(nameof(curveTypeType), curveTypeType, null)
        };
        if (centering == EnumUpOrDown.Centered)
            return res;
        return centering == EnumUpOrDown.Up ? 0.5f*(res + param) : 0.5f*(res - param);
    }
}