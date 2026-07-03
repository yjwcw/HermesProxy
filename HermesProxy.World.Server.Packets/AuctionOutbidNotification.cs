using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class AuctionOutbidNotification : ServerPacket
{
	public AuctionBidderNotification Info;

	public ulong BidAmount;

	public ulong MinIncrement;

	public AuctionOutbidNotification()
		: base(Opcode.SMSG_AUCTION_OUTBID_NOTIFICATION)
	{
	}

	public override void Write()
	{
		Info.Write(_worldPacket);
		_worldPacket.WriteUInt64(BidAmount);
		_worldPacket.WriteUInt64(MinIncrement);
	}
}
