using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class CorpseReclaimDelay : ServerPacket
{
	public uint Remaining;

	public CorpseReclaimDelay()
		: base(Opcode.SMSG_CORPSE_RECLAIM_DELAY, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt32(Remaining);
	}
}
