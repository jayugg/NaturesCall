using System.Collections.Generic;
using NaturesCall.Bladder;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace NaturesCall.Compatibility;

public class HoDCompat : ModSystem
{
    private Dictionary<string, float> PlayerThirstLevels = new Dictionary<string, float>();
    
    public override double ExecuteOrder() => 1.03;
    public override bool ShouldLoad(ICoreAPI api) => api.ModLoader.IsModEnabled("hydrateordiedrate");
    public static bool IsLoaded;
    
    public override void Start(ICoreAPI api)
    {
        IsLoaded = true;
        Core.Logger.Event("Thirst mod detected: Hydrate or Diedrate");
    }
    public override void StartServerSide(ICoreServerAPI sapi)
    {
        base.Start(sapi);
        sapi.World.RegisterGameTickListener((dt) => OnServerGameTick(sapi, dt), 200);
    }
    
    private void OnServerGameTick(ICoreAPI api, float dt)
    {
        foreach (var player in api.World.AllPlayers)
        {
            if (!PlayerThirstLevels.TryGetValue(player.PlayerUID, out float value))
            {
                value = 0;
                PlayerThirstLevels.Add(player.PlayerUID, value);
            }
            float currentThirst = player.Entity.WatchedAttributes.GetFloat("currentThirst");
            float previousThirst = value;
            if (currentThirst < previousThirst)
            {
                float difference = previousThirst - currentThirst;
                player.Entity.GetBehavior<EntityBehaviorBladder>()?.ReceiveFluid(difference);
            }
            PlayerThirstLevels[player.PlayerUID] = currentThirst;
        }
    }
}