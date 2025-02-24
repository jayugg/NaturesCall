using ProtoBuf;
using Vintagestory.API.MathTools;

namespace NaturesCall.Network;

public class PeeMessage
{
    [ProtoContract]
    public class Request
    {
        [ProtoMember(1)]
        public BlockPos Position;
        [ProtoMember(2)]
        public Vec3d HitPostion;
        [ProtoMember(3)]
        public string Color;
    }

    [ProtoContract]
    public class Response
    {
        [ProtoMember(1)]
        public BlockPos Position;
    }
}