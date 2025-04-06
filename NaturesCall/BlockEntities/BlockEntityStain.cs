using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace NaturesCall.BlockEntities;

public class BlockEntityStain : BlockEntity
{
    public MeshData Mesh { get; protected set; }

    public override void Initialize(ICoreAPI api)
    {
        base.Initialize(api);
        if (Mesh == null)
        {
            Init();
        }
        api.World.RegisterGameTickListener(OnGameTick, 1);
    }

    private void OnGameTick(float dt)
    {
        var shouldDie = Api.World.BlockAccessor.GetBlock(Pos.UpCopy()).BlockMaterial == EnumBlockMaterial.Liquid ||
                        Api.World.BlockAccessor.GetDistanceToRainFall(Pos, 2) < 2;
        if (shouldDie)
            Api.World.BlockAccessor.SetBlock(0, Pos);
        Block.OnBlockRemoved(Api.World, Pos);
    }

    protected void Init()
    {
        if (Api is not ICoreClientAPI capi)
        {
            return;
        }

        Mesh = capi.TesselatorManager.GetDefaultBlockMesh(Block);
    }

    public override void OnBlockPlaced(ItemStack byItemStack = null) => Init();

    public override void OnBlockUnloaded() => Mesh?.Dispose();

    public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
    {
        mesher.AddMeshData(Mesh);
        return false;
    }
}