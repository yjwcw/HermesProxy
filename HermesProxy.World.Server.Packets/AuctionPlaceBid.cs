namespace HermesProxy.World.Server.Packets;

internal class AuctionPlaceBid : ClientPacket
{
	public WowGuid128 Auctioneer;

	public ulong BidAmount;

	public uint AuctionID;

	public AddOnInfo TaintedBy;

	public AuctionPlaceBid(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		Auctioneer = _worldPacket.ReadPackedGuid128();
		AuctionID = _worldPacket.ReadUInt32();
		BidAmount = _worldPacket.ReadUInt64();
		if (_worldPacket.HasBit())
		{
			TaintedBy = new AddOnInfo();
		}
		if (TaintedBy != null)
		{
			TaintedBy.Read(_worldPacket);
		}
	}
}
