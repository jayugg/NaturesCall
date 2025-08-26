using Vintagestory.API.Common;

namespace NaturesCall.Thirst;

public enum BladderSourceType
{
    Hunger,
    HydrateOrDiedrate
}
public interface IBladderSource
{
    // Only use on the server side
    public float GetCurrentThirst(IPlayer player);
}