namespace NaturesCall.Util;

public enum EnumPeeMode
{
    None,
    Stand,
    Sit
}

public static class EnumPeeModeExtensions
{
    public static bool IsSitting(this EnumPeeMode peeMode)
    {
        return peeMode == EnumPeeMode.Sit;
    }

    public static bool IsStanding(this EnumPeeMode peeMode)
    {
        return peeMode == EnumPeeMode.Stand;
    }
}