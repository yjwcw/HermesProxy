namespace HermesProxy.World.Server.Packets;

internal class AuctionRemoveItem : ClientPacket
{
	public WowGuid128 Auctioneer;

	public uint AuctionID;

	public AddOnInfo TaintedBy;

	public AuctionRemoveItem(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		Auctioneer = _worldPacket.ReadPackedGuid128();
		AuctionID = _worldPacket.ReadUInt32();
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
