namespace HermesProxy.World.Server.Packets;

internal class AuctionBidderNotification
{
	public uint Command = 2u;

	public uint AuctionID;

	public WowGuid128 Bidder;

	public ItemInstance Item = new ItemInstance();

	public void Write(WorldPacket data)
	{
		data.WriteUInt32(Command);
		data.WriteUInt32(AuctionID);
		data.WritePackedGuid128(Bidder);
		Item.Write(data);
	}
}
