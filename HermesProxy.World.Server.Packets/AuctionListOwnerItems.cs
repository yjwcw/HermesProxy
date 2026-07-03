namespace HermesProxy.World.Server.Packets;

internal class AuctionListOwnerItems : ClientPacket
{
	public WowGuid128 Auctioneer;

	public uint Offset;

	public AuctionListOwnerItems(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		Auctioneer = _worldPacket.ReadPackedGuid128();
		Offset = _worldPacket.ReadUInt32();
	}
}
