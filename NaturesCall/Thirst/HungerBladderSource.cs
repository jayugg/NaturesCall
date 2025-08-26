using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace NaturesCall.Thirst;

public class HungerBladderSource : IBladderSource
{
    public float GetCurrentThirst(IPlayer player)
    {
        var hungerTree = player.Entity.WatchedAttributes.GetTreeAttribute("hunger");
        return hungerTree?.TryGetFloat("currentsaturation") ?? 0;
    }
}