namespace HermesProxy.World.Server.Packets;

public struct AuctionItemForSale
{
	public WowGuid128 Guid;

	public uint UseCount;

	public AuctionItemForSale(WorldPacket data)
	{
		Guid = data.ReadPackedGuid128();
		UseCount = data.ReadUInt32();
	}
}
