namespace NaturesCall.Util.StatModifier;

public class StatMultiplier
{
    public float Multiplier { get; set; } 
    public EnumUpOrDown Centering { get; set; } = EnumUpOrDown.Centered;
    public EnumCurveType CurveType { get; set; } = EnumCurveType.Sin;
    public EnumCurveType LowerHalfCurveType { get; set; } = EnumCurveType.None;
    public bool Inverted { get; set; }
    
    public float CalcModifier(float ratio)
    {
        if (Inverted)
        {
            ratio = 1 - ratio;
        }
        if (LowerHalfCurveType != EnumCurveType.None)
        {
            return ratio < 0.5
                ? StatModifierUtil.CalcStatModifier(ratio, Multiplier,
                    LowerHalfCurveType, Centering)
                : StatModifierUtil.CalcStatModifier(ratio, Multiplier,
                    CurveType, Centering);
        }
        return StatModifierUtil.CalcStatModifier(ratio, Multiplier,
            CurveType, Centering);
    }
}