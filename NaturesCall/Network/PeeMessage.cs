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
        [ProtoMember(4)]
        public long ActionMs;
    }

    [ProtoContract]
    public class Response
    {
        [ProtoMember(1)]
        public BlockPos Position;
    }
}