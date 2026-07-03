namespace HermesProxy.World.Server.Packets;

public class SellItem : ClientPacket
{
	public WowGuid128 VendorGUID;

	public WowGuid128 ItemGUID;

	public uint Amount;

	public SellItem(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		VendorGUID = _worldPacket.ReadPackedGuid128();
		ItemGUID = _worldPacket.ReadPackedGuid128();
		Amount = _worldPacket.ReadUInt32();
	}
}
