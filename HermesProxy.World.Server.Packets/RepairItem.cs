namespace HermesProxy.World.Server.Packets;

public class RepairItem : ClientPacket
{
	public WowGuid128 VendorGUID;

	public WowGuid128 ItemGUID;

	public bool UseGuildBank;

	public RepairItem(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		VendorGUID = _worldPacket.ReadPackedGuid128();
		ItemGUID = _worldPacket.ReadPackedGuid128();
		UseGuildBank = _worldPacket.HasBit();
	}
}
