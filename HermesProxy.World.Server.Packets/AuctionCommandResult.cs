using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class AuctionCommandResult : ServerPacket
{
	public uint AuctionID;

	public AuctionHouseAction Command;

	public AuctionHouseError ErrorCode;

	public InventoryResult BagResult = InventoryResult.InternalBagError;

	public WowGuid128 Guid = WowGuid128.Empty;

	public ulong MinIncrement;

	public ulong Money;

	public uint DesiredDelay;

	public AuctionCommandResult()
		: base(Opcode.SMSG_AUCTION_COMMAND_RESULT)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt32(AuctionID);
		_worldPacket.WriteInt32((int)Command);
		_worldPacket.WriteInt32((int)ErrorCode);
		_worldPacket.WriteInt32((int)BagResult);
		_worldPacket.WritePackedGuid128(Guid);
		_worldPacket.WriteUInt64(MinIncrement);
		_worldPacket.WriteUInt64(Money);
		_worldPacket.WriteUInt32(DesiredDelay);
	}
}
