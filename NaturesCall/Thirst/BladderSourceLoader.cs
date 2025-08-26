#nullable enable
using System.Collections.Generic;
using NaturesCall.Bladder;
using NaturesCall.Compatibility;
using NaturesCall.Config;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace NaturesCall.Thirst;
public class BladderSourceLoader : ModSystem
{
    public BladderSourceType SourceType;
    private readonly Dictionary<string, float> _playerThirstLevels = new();
    private IBladderSource? _bladderSource;
    public override void StartServerSide(ICoreServerAPI sapi)
    {
        base.StartServerSide(sapi);
        sapi.World.RegisterGameTickListener((dt) => OnServerGameTick(sapi), 200);
        if (sapi.ModLoader.IsModEnabled("hydrateordiedrate"))
        {
            SourceType = BladderSourceType.HydrateOrDiedrate;
            Core.Logger.Event("Thirst mod detected: Hydrate or Diedrate");
            _bladderSource = new HoDBladderSource();
        }
        else
        {
            SourceType = BladderSourceType.Hunger;
            Core.Logger.Event("No known thirst mod detected, using hunger as bladder source");
            _bladderSource = new HungerBladderSource();
        }
    }
    
    private void OnServerGameTick(ICoreAPI api)
    {
        if (_bladderSource == null) return;
        foreach (var player in api.World.AllPlayers)
        {
            if (!_playerThirstLevels.TryGetValue(player.PlayerUID, out var value))
            {
                value = 0;
                _playerThirstLevels.Add(player.PlayerUID, value);
            }
            var currentThirst = _bladderSource.GetCurrentThirst(player);
            var previousThirst = value;
            if (currentThirst < previousThirst)
            {
                var difference = previousThirst - currentThirst;
                var bladderFill = (float) (ConfigSystem.ConfigServer.BladderFillMultiplier * difference);
                player.Entity.GetBehavior<EntityBehaviorBladder>()?.ReceiveFluid(bladderFill);
            }
            _playerThirstLevels[player.PlayerUID] = currentThirst;
        }
    }

    public override void Dispose()
    {
        _playerThirstLevels.Clear();
        _bladderSource = null;
        base.Dispose();
    }
}