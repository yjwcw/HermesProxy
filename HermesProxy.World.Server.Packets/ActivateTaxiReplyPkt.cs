using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class ActivateTaxiReplyPkt : ServerPacket
{
	public ActivateTaxiReply Reply;

	public ActivateTaxiReplyPkt()
		: base(Opcode.SMSG_ACTIVATE_TAXI_REPLY)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteBits(Reply, 4);
		_worldPacket.FlushBits();
	}
}
