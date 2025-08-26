using NaturesCall.Thirst;
using Vintagestory.API.Common;

namespace NaturesCall.Compatibility;

public class HoDBladderSource : IBladderSource
{
    public float GetCurrentThirst(IPlayer player)
    {
        var thirstTree = player.Entity.WatchedAttributes.GetTreeAttribute("thirst");
        return thirstTree?.TryGetFloat("currentThirst") ?? 0;
    }
}