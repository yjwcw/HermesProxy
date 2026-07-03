namespace HermesProxy.World.Server.Packets;

public class AuctionOwnerNotification
{
	public uint AuctionID;

	public ulong BidAmount;

	public ItemInstance Item = new ItemInstance();

	public void Write(WorldPacket data)
	{
		data.WriteUInt32(AuctionID);
		data.WriteUInt64(BidAmount);
		Item.Write(data);
	}
}
