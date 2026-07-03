using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class AuctionClosedNotification : ServerPacket
{
	public AuctionOwnerNotification Info;

	public float ProceedsMailDelay = 3600f;

	public bool Sold = true;

	public AuctionClosedNotification()
		: base(Opcode.SMSG_AUCTION_CLOSED_NOTIFICATION)
	{
	}

	public override void Write()
	{
		Info.Write(_worldPacket);
		_worldPacket.WriteFloat(ProceedsMailDelay);
		_worldPacket.WriteBit(Sold);
		_worldPacket.FlushBits();
	}
}
