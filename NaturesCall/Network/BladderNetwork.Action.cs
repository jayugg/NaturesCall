using System;
using System.Linq;
using NaturesCall.Bladder;
using NaturesCall.Config;
using NaturesCall.Util;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace NaturesCall.Network;

public partial class BladderNetwork
{
    #region Client
    
    private void OnServerPeeMessage(PeeMessage.Response response)
    {
        var entity = _capi.World.Player.Entity;
        SpawnPeeParticles(entity, response.Position, entity.BlockSelection?.HitPosition, ConfigSystem.ConfigClient.UrineColor == "default" ? null : ConfigSystem.ConfigClient.UrineColor);
        EntityBehaviorBladder.PlayPeeSound(entity);
    }
    
    private bool OnPeeKeyPressed(KeyCombination t1)
    {
        var world = _capi?.World;
        var player = world?.Player;
        if (world == null || player == null) return false;
        if ( (player.Entity.World.Side & EnumAppSide.Server) != 0) return false;
        
        if (ConfigSystem.ConfigClient.PeeMode == EnumPeeMode.None)
        {
            ChoiceHud.TryOpen();
            player.IngameError(player, "peemodenotset", Lang.Get(Core.ModId+":peemodenotset") );
        }
        if (world.ElapsedMilliseconds - _lastPeeTime < PeeActionMs) return false;
        if ((player.Entity.Controls.TriesToMove ||
               !player.Entity.RightHandItemSlot.Empty ||
               !ConfigSystem.ConfigClient.PeeMode.IsStanding()) &&
            (!player.Entity.Controls.FloorSitting ||
             !ConfigSystem.ConfigClient.PeeMode.IsSitting())) return false;
        var actionMs = world.ElapsedMilliseconds - _lastPeeMessageTime;
        _lastPeeTime = world.ElapsedMilliseconds;
        _lastPeeMessageTime = world.ElapsedMilliseconds;
        if (!actionMs.HasValue)
            return false;
        _clientChannel.SendPacket(new PeeMessage.Request()
        {
            Position = player.Entity.BlockSelection?.Position,
            HitPostion = player.Entity.BlockSelection?.HitPosition,
            Color = ConfigSystem.ConfigClient.UrineColor == "default" ? null : ConfigSystem.ConfigClient.UrineColor,
            ActionMs = actionMs.Value
        });
        return true;

    }
    
    #endregion
    
    #region Server
    
    private void HandlePeeAction(IServerPlayer player, PeeMessage.Request request)
    {
        if (!player.Entity.HasBehavior<EntityBehaviorBladder>()) return;
        if (request.Position == null) return;
        var bh = player.Entity.GetBehavior<EntityBehaviorBladder>();
        var urinationDs = request.ActionMs / 100f;
        var drainAmount = ConfigSystem.ConfigServer.UrineDrainRate * urinationDs * 4;
        if (!bh.Drain(drainAmount)) return;
        EntityBehaviorBladder.PlayPeeSound(player.Entity);
        SpawnPeeParticles(player.Entity, request.Position, player.CurrentBlockSelection?.HitPosition, request.Color);
        var world = player.Entity.World;
        var block = world.BlockAccessor.GetBlock(request.Position);
        _serverChannel.SendPacket(new PeeMessage.Response() { Position = request.Position }, player);
        if (block is BlockLiquidContainerBase container)
        {
            var waterStack = new ItemStack(world.GetItem(Core.ModId+":urineportion"))
                {
                    StackSize = 128
                };
            var desiredLiters = drainAmount * 3f/1800; // 3 liters per 1800 hydration
            var storedLitres = player.Entity.Attributes.GetFloat($"{Core.ModId}:urineVirtualLiters");
            var totalLiters = desiredLiters + storedLitres;
            var virtualLiters = totalLiters % 0.01f;
            var physicalLiters = totalLiters - virtualLiters;
            var stackSizeIn = container.TryPutLiquid(request.Position, waterStack, physicalLiters);
            container.DoLiquidMovedEffects(player, waterStack, stackSizeIn, BlockLiquidContainerBase.EnumLiquidDirection.Fill);
        } else if (block is BlockFarmland)
        {
            FertiliseFarmland(world, request.Position);
        } 
        else if (world.BlockAccessor.GetBlock(request.Position.DownCopy()) is BlockFarmland)
        {
            FertiliseFarmland(world, request.Position.DownCopy());
        }
        else if (world.BlockAccessor.GetBlockEntity(request.Position) is IFarmlandBlockEntity)
        {
            FertiliseIFarmland(world, request.Position);
        }
        else if (world.BlockAccessor.GetBlockEntity(request.Position.DownCopy()) is IFarmlandBlockEntity)
        {
            FertiliseIFarmland(world, request.Position.DownCopy());
        }
        else if (block is BlockToolMold)
        {
            var be = world.BlockAccessor.GetBlockEntity(request.Position) as BlockEntityToolMold; 
            be?.CoolWithWater();
        }
        else if (block is BlockIngotMold)
        {
            var be = world.BlockAccessor.GetBlockEntity(request.Position) as BlockEntityIngotMold; 
            be?.CoolWithWater();
        }
        if (!ConfigSystem.ConfigServer.UrineStains || player.CurrentBlockSelection == null) return;
        if (!player.CurrentBlockSelection.Block.SideIsSolid(player.CurrentBlockSelection.Position, player.CurrentBlockSelection.Face.Index)) return;
        var rand = world.Rand.Next(0, 24);
        var x = rand % 6 + 1;
        var y = rand / 6 + 1;
        var stain = world.GetBlock(new AssetLocation(Core.ModId, $"caveart-stain-urine-1-{x}-{y}"));
        if (SuitableStainPosition(world.BlockAccessor, player.CurrentBlockSelection))
            world.BlockAccessor.SetDecor(stain, request.Position,
                player.CurrentBlockSelection.ToDecorIndex());
    }

    private void FertiliseIFarmland(IWorldAccessor world, BlockPos position)
    {
        if (position == null) return;
        var be = world.BlockAccessor.GetBlockEntity(position);
        var tree = new TreeAttribute();
        be.ToTreeAttributes(tree);
        if (!tree.HasAttribute("moistureLevel")) return;
        var moistureLevel = tree.GetFloat("moistureLevel");
        moistureLevel = Math.Min(1f, moistureLevel+ 0.05f / 2f);
        tree.SetFloat("moistureLevel", moistureLevel);
        if (ConfigSystem.ConfigServer.UrineNutrientChance > world.Rand.NextDouble())
        {
            var nutrients = ConfigSystem.ConfigServer.UrineNutrientLevels;
            var n = tree.GetFloat("n") + nutrients[0];
            var p = tree.GetFloat("p") + nutrients[(EnumSoilNutrient)1];
            var k = tree.GetFloat("k") + nutrients[(EnumSoilNutrient)2];
            tree.SetFloat("n", n);
            tree.SetFloat("p", p);
            tree.SetFloat("k", k);
        }
        be.FromTreeAttributes(tree, world);
    }

    private static void FertiliseFarmland(IWorldAccessor world, BlockPos position)
    {
        if (position == null) return;
        var be = world.BlockAccessor.GetBlockEntity(position) as BlockEntityFarmland; 
        be?.WaterFarmland(0.05f);
        if (ConfigSystem.ConfigServer.UrineNutrientChance > world.Rand.NextDouble())
        {
            be?.IncreaseNutrients(ConfigSystem.ConfigServer.UrineNutrientLevels);
        }
    }
    
    public static void SpawnPeeParticles(Entity byEntity, BlockPos pos, Vec3d hitPos, string color = null)
    {
        if (hitPos == null || pos == null) return;
        var entityPos = byEntity.Pos.XYZ.AddCopy(byEntity.LocalEyePos.SubCopy(0, 0.2, 0));
        var posVec = pos.ToVec3d().AddCopy(hitPos);
        var dist = (posVec - entityPos);
        var addVertical = new Vec3f(0, (float)(0.5f*Math.Sqrt(dist.NoY().LengthSq())), 0);
        var velocity = 2.5f * dist.ToVec3f().AddCopy(addVertical).Normalize();
        var xyz = entityPos.AddCopy(0.5 * dist.Normalize());
        var one = new Vec3f(1, 1, 1);

        _waterParticles = new SimpleParticleProperties(1f, 1f, -1, xyz, new Vec3d(), velocity.AddCopy(0.2f*one), velocity.AddCopy(-0.2f*one), minSize: 0.33f, maxSize: 0.75f)
        {
            AddPos = new Vec3d(),
            SizeEvolve = new EvolvingNatFloat(EnumTransformFunction.LINEAR, -1f),
            ClimateColorMap = "climateWaterTint",
            AddQuantity = 5f,
            GravityEffect = 0.6f,
            ShouldDieInLiquid = true,
            Bounciness = 0,
            WithTerrainCollision = false
        };
        
        if (color != null)
        {
            switch (color)
            {
                case "gaymer":
                {
                    // Make it change color over time using a sine wave in hsv space
                    var hue = (Math.Sin(byEntity.World.ElapsedMilliseconds / 1000.0) * 0.5 + 0.5) * 255;
                    var rgbaColor = ColorUtil.HsvToRgba((int)hue, 255, 255);
                    _waterParticles.Color = (rgbaColor & 0x00FFFFFF) | (120 << 24); // Ensure alpha is set to 160
                    _waterParticles.ClimateColorMap = null;
                    break;
                }
                case "blahaj":
                {
                    // Set the color to the trans flag colors
                    var transFlagColors = new[] { "#5BCEFA", "#F5A9B8", "#FFFFFF", "#F5A9B8", "#5BCEFA" }; // Light blue, pink, white
                    var colorIndex = (int)((byEntity.World.ElapsedMilliseconds / 1000.0) % transFlagColors.Length);
                    var colors = ColorUtil.Hex2Doubles(transFlagColors[colorIndex]);
                    _waterParticles.Color = ColorUtil.ToRgba(160, (int)(colors[0] * 255), (int)(colors[1] * 255), (int)(colors[2] * 255));
                    _waterParticles.ClimateColorMap = null;
                    break;
                }
                default:
                {
                    var colors = ColorUtil.Hex2Doubles(color);
                    _waterParticles.Color = ColorUtil.ToRgba(160, (int)(colors[0]*255), (int)(colors[1]*255), (int)(colors[2]*255));
                    _waterParticles.ClimateColorMap = null;
                    break;
                }
            }
        }
        
        byEntity.World.SpawnParticles(_waterParticles, byEntity is EntityPlayer entityPlayer ? entityPlayer.Player : null);
    }
    
    private bool SuitableStainPosition(IBlockAccessor blockAccessor, BlockSelection blockSel)
    {
        var block = blockAccessor.GetBlock(blockSel.Position);
        if (block.decorBehaviorFlags != 0) return false;
        if (block.SideSolid[blockSel.Face.Index] || block is BlockMicroBlock && (blockAccessor.GetBlockEntity(blockSel.Position) is BlockEntityMicroBlock blockEntity ? (blockEntity.sideAlmostSolid[blockSel.Face.Index] ? 1 : 0) : 0) != 0)
        {
            var blockMaterial = block.GetBlockMaterial(blockAccessor, blockSel.Position);
            return ConfigSystem.ConfigServer.UrineStainableMaterials.Any(t => blockMaterial == t);
        }
        return false;
    }
    
    #endregion
}