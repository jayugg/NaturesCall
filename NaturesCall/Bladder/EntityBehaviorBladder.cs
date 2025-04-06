using System;
using NaturesCall.Config;
using NaturesCall.Util;
using NaturesCall.Util.StatModifier;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;

namespace NaturesCall.Bladder;

public class EntityBehaviorBladder : EntityBehavior
{
    private ITreeAttribute _bladderTree;
    private ICoreAPI _api;
    public override string PropertyName() => AttributeKey;
    private long _listenerId;
    private string AttributeKey => Core.ModId + ":bladder";
    
    public StatMultiplier WalkSpeedMultiplier = new()
    {
        Multiplier = ConfigSystem.ConfigServer.BladderWalkSpeedDebuff,
        Centering = EnumUpOrDown.Down,
        CurveType = EnumCurveType.Linear,
        LowerHalfCurveType = EnumCurveType.Linear,
        Inverted = false
    }; 

    public float CapacityOverload {
        get
        {
            var capacityOverload = (float) Math.Round(CapacityModifier*ConfigSystem.ConfigServer.BladderCapacityOverload*ConfigSystem.ConfigServer.BladderCapacity);
            _bladderTree?.SetFloat("capacityoverload", capacityOverload);
            entity.WatchedAttributes.MarkPathDirty(AttributeKey);
            return capacityOverload;
        }
    }

    public float Capacity
    {
        get
        {
            var capacity = (float) Math.Round(CapacityModifier*ConfigSystem.ConfigServer.BladderCapacity);
            _bladderTree?.SetFloat("capacity", capacity);
            entity.WatchedAttributes.MarkPathDirty(AttributeKey);
            return capacity;
        }
    }

    public float CurrentLevel
    {
        get => Math.Min(_bladderTree?.GetFloat("currentlevel")  ?? 0, EffectiveCapacity);
        set
        {
            _bladderTree?.SetFloat("currentlevel", Math.Min(value, EffectiveCapacity));
            entity.WatchedAttributes.MarkPathDirty(AttributeKey);
        }
    }
    
    public float CapacityModifier
    {
        get => _bladderTree?.GetFloat("capacitymodifier") ?? 1;
        set
        {
            _bladderTree?.SetFloat("capacitymodifier", value);
            entity.WatchedAttributes.MarkPathDirty(AttributeKey);
        }
    }
    
    public float EffectiveCapacity => Capacity + CapacityOverload;
    
    public EntityBehaviorBladder(Entity entity) : base(entity)
    {
    }

    public override void Initialize(EntityProperties properties, JsonObject typeAttributes)
    {
        _bladderTree = entity.WatchedAttributes.GetTreeAttribute(AttributeKey);
        _api = entity.World.Api;

        if (_bladderTree == null || _bladderTree.GetFloat("capacity") == 0 || _bladderTree.GetFloat("capacityoverload") == 0)
        {
            entity.WatchedAttributes.SetAttribute(AttributeKey, _bladderTree = new TreeAttribute());
            CurrentLevel = typeAttributes["currentlevel"].AsFloat(0);
            CapacityModifier = typeAttributes["capacitymodifier"].AsFloat(1);
        }
        _listenerId = entity.World.RegisterGameTickListener(SlowTick, 500);
    }
    
    public void ReceiveFluid(float fluidAmount)
    {
        _api?.Event.PushEvent(EventIds.BladderReceiveFluid, new FloatAttribute(fluidAmount));
        CurrentLevel = Math.Clamp(CurrentLevel + fluidAmount, 0.0f, EffectiveCapacity);
    }
    
    public override void OnEntityReceiveDamage(DamageSource damageSource, ref float damage)
    {
        if (damageSource.Type != EnumDamageType.Heal || damageSource.Source != EnumDamageSource.Revive)
            return;
        CurrentLevel = 0f;
    }
    
    public bool Drain(float quantity = 1)
    {
        var currentLevel = CurrentLevel;
        if (currentLevel < 0.0) return false;
        var newLevel = Math.Clamp(currentLevel - quantity, 0.0f, EffectiveCapacity);
        CurrentLevel = newLevel;
        return newLevel < currentLevel;
    }
    
    public static void PlayPeeSound(EntityAgent byEntity, int soundRepeats = 1)
    {
        IPlayer dualCallByPlayer = null;
        if (byEntity is EntityPlayer)
            dualCallByPlayer = byEntity.World.PlayerByUid(((EntityPlayer) byEntity).PlayerUID);
        byEntity.World.PlaySoundAt(new AssetLocation("sounds/effect/watering"), byEntity, dualCallByPlayer, true, 16, 0.2f);
        soundRepeats--;
        if (soundRepeats <= 0)
            return;
        byEntity.World.RegisterCallback(_ => PlayPeeSound(byEntity, soundRepeats), 300);
    }
    
    private void SlowTick(float dt)
    { 
        if (entity is EntityPlayer player &&
            player.World.PlayerByUid(player.PlayerUID).WorldData.CurrentGameMode ==
            EnumGameMode.Creative)
            return;
        if (!IsOverloaded())
        {
            entity.Stats.Remove("walkspeed", "bladderfull");
        }
        else
        {
            WalkSpeedMultiplier.Multiplier = ConfigSystem.ConfigServer.BladderWalkSpeedDebuff;
            entity.Stats.Set("walkspeed", "bladderfull", WalkSpeedMultiplier.CalcModifier((CurrentLevel - Capacity)/CapacityOverload));
        }
    }
    
    private bool IsOverloaded()
    {
        return CurrentLevel > Capacity;
    }
    
}