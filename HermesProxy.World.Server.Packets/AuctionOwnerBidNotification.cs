using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class AuctionOwnerBidNotification : ServerPacket
{
	public AuctionOwnerNotification Info;

	public ulong MinIncrement;

	public WowGuid128 Bidder;

	public AuctionOwnerBidNotification()
		: base(Opcode.SMSG_AUCTION_OWNER_BID_NOTIFICATION)
	{
	}

	public override void Write()
	{
		Info.Write(_worldPacket);
		_worldPacket.WriteUInt64(MinIncrement);
		_worldPacket.WritePackedGuid128(Bidder);
	}
}
