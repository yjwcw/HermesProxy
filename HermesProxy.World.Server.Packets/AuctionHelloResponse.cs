using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class AuctionHelloResponse : ServerPacket
{
	public WowGuid128 Guid;

	public uint AuctionHouseID;

	public bool OpenForBusiness = true;

	public AuctionHelloResponse()
		: base(Opcode.SMSG_AUCTION_HELLO_RESPONSE)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(Guid);
		_worldPacket.WriteUInt32(AuctionHouseID);
		_worldPacket.WriteBit(OpenForBusiness);
		_worldPacket.FlushBits();
	}
}
