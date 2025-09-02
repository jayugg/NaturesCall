using System;
using NaturesCall.Bladder;
using NaturesCall.Config;
using NaturesCall.Util;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace NaturesCall;

public static class Commands
    {
        public static void RegisterClient(ICoreClientAPI api)
        {
            api.ChatCommands
                .Create("setPeeMode")
                .WithDescription("Sets the player's urination mode (sit|stand).")
                .WithArgs(api.ChatCommands.Parsers.WordRange("mode", new []{"sit", "stand"}))
                .HandleWith((args) => OnSetPeeModeCommand(api, args));
        }
        public static void Register(ICoreServerAPI api)
        {
            api.ChatCommands
                .Create("naturescall")
                .WithDescription("Debug commands for Nature's Call.")
                .WithAlias("nc")
                .RequiresPrivilege("controlserver")
                .BeginSubCommand("resetBladderStats")
                .WithDescription("Resets the player's stat modifiers from bladder.")
                .WithArgs(api.ChatCommands.Parsers.OptionalWord("playerName"))
                .HandleWith((args) => OnResetStatsCommand(api, args))
                .EndSubCommand()
                .BeginSubCommand("setBladder")
                .WithDescription("Sets the player's bladder level.")
                .WithArgs(api.ChatCommands.Parsers.OptionalWord("playerName"),
                    api.ChatCommands.Parsers.Float("value"))
                .HandleWith((args) => OnSetBladderCommand(api, args))
                .EndSubCommand()
                .BeginSubCommand("setCapacityModifier")
                .WithDescription("Sets the player's bladder capacity modifier (alteration to max bladder).")
                .WithArgs(api.ChatCommands.Parsers.OptionalWord("playerName"),
                    api.ChatCommands.Parsers.Float("value"))
                .HandleWith((args) => OnSetCapacityModifierCommand(api, args))
                .EndSubCommand();
        }

        private static TextCommandResult OnSetPeeModeCommand(ICoreClientAPI api, TextCommandCallingArgs args)
        {
            var mode = Enum.Parse<EnumPeeMode>(args[0] as string ?? string.Empty, true);
            if (args.Caller.Player is not IClientPlayer player) return TextCommandResult.Error("Player not found.");
            ConfigSystem.ConfigClient.PeeMode = mode;
            ModConfig.WriteConfig(api, Constants.ConfigClientName, ConfigSystem.ConfigClient);
            api.Event.PushEvent(EventIds.ConfigReloaded);
            return TextCommandResult.Success($"Pee mode set to {mode} for player '{player.PlayerName}'.");
        }

        private static TextCommandResult OnResetStatsCommand(ICoreServerAPI api, TextCommandCallingArgs args)
        {
            var playerName = args[0] as string;

            IServerPlayer targetPlayer;

            if (string.IsNullOrEmpty(playerName))
            {
                targetPlayer = args.Caller.Player as IServerPlayer;
            }
            else
            {
                targetPlayer = GetPlayerByName(api, playerName);
                if (targetPlayer == null)
                {
                    return TextCommandResult.Error($"Player '{playerName}' not found.");
                }
            }

            ResetModBoosts(targetPlayer?.Entity);
            return TextCommandResult.Success($"Bladder stats reset for player '{targetPlayer?.PlayerName}'.");
        }

        private static IServerPlayer GetPlayerByName(ICoreServerAPI api, string playerName)
        {
            foreach (var player1 in api.World.AllOnlinePlayers)
            {
                var player = (IServerPlayer)player1;
                if (player.PlayerName.Equals(playerName, StringComparison.OrdinalIgnoreCase))
                {
                    return player;
                }
            }

            return null;
        }

        private static TextCommandResult OnSetBladderCommand(ICoreServerAPI api,
            TextCommandCallingArgs args)
        {
            var playerName = args[0] as string;
            var newLevel = (float)args[1];

            IServerPlayer targetPlayer;

            if (string.IsNullOrEmpty(playerName))
            {
                targetPlayer = args.Caller.Player as IServerPlayer;
            }
            else
            {
                targetPlayer = GetPlayerByName(api, playerName);
                if (targetPlayer == null)
                {
                    return TextCommandResult.Error($"Player '{playerName}' not found.");
                }
            }

            var bladderBehavior = targetPlayer?.Entity.GetBehavior<EntityBehaviorBladder>();
            if (bladderBehavior == null) return TextCommandResult.Error("Bladder behavior not found.");

            bladderBehavior.CurrentLevel = newLevel;

            return TextCommandResult.Success($"Bladder set to {bladderBehavior.CurrentLevel} for player '{targetPlayer.PlayerName}'.");
        }
        
        private static TextCommandResult OnSetCapacityModifierCommand(ICoreServerAPI api, TextCommandCallingArgs args)
        {
            var playerName = args[0] as string;
            var newValue = (float)args[1];

            if (1e-3 < newValue || newValue > 10)
            {
                return TextCommandResult.Error($"This is a multiplier, please select a reasonable value in the range (0.1, 10).");
            }

            IServerPlayer targetPlayer;

            if (string.IsNullOrEmpty(playerName))
            {
                targetPlayer = args.Caller.Player as IServerPlayer;
            }
            else
            {
                targetPlayer = GetPlayerByName(api, playerName);
                if (targetPlayer == null)
                {
                    return TextCommandResult.Error($"Player '{playerName}' not found.");
                }
            }

            var bladderBehavior = targetPlayer?.Entity.GetBehavior<EntityBehaviorBladder>();
            if (bladderBehavior == null) return TextCommandResult.Error("Bladder behavior not found.");

            bladderBehavior.CapacityModifier = newValue;
            return TextCommandResult.Success($"Capacity modifier set to {bladderBehavior.CapacityModifier} for player '{targetPlayer.PlayerName}'.");
        }
        
        public static void ResetModBoosts(EntityPlayer player)
        {
            if (player == null) return;
            player.Stats.Remove("walkspeed", "bladderfull");
            player.Stats.Remove("walkspeed", "bowelfull");
        }
    }