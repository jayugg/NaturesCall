using System.Collections.Generic;
using NaturesCall.Bladder;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace NaturesCall.Compatibility;

public class HoDCompat : ModSystem
{
    private readonly Dictionary<string, float> _playerThirstLevels = new();
    public override double ExecuteOrder() => 1.03;
    public override bool ShouldLoad(ICoreAPI api) => api.ModLoader.IsModEnabled("hydrateordiedrate");
    public bool IsLoaded;
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

    private static float GetCurrentThirst(IPlayer player)
    {
        var thirstTree = player.Entity.WatchedAttributes.GetTreeAttribute("thirst");
        return thirstTree?.TryGetFloat("currentThirst") ?? 0;
    }
    
    private void OnServerGameTick(ICoreAPI api, float dt)
    {
        foreach (var player in api.World.AllPlayers)
        {
            if (!_playerThirstLevels.TryGetValue(player.PlayerUID, out var value))
            {
                value = 0;
                _playerThirstLevels.Add(player.PlayerUID, value);
            }
            var currentThirst = GetCurrentThirst(player);
            var previousThirst = value;
            if (currentThirst < previousThirst)
            {
                var difference = previousThirst - currentThirst;
                player.Entity.GetBehavior<EntityBehaviorBladder>()?.ReceiveFluid(difference);
            }
            _playerThirstLevels[player.PlayerUID] = currentThirst;
        }
    }

    public override void Dispose()
    {
        _playerThirstLevels.Clear();
        IsLoaded = false;
        base.Dispose();
    }
}