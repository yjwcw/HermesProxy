namespace HermesProxy.World.Server.Packets;

public class BuyBackItem : ClientPacket
{
	public WowGuid128 VendorGUID;

	public uint Slot;

	public BuyBackItem(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		VendorGUID = _worldPacket.ReadPackedGuid128();
		Slot = _worldPacket.ReadUInt32();
	}
}
