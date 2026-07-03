namespace HermesProxy.World.Server.Packets;

internal class AutoEquipItemSlot : ClientPacket
{
	public WowGuid128 Item;

	public byte ItemDstSlot;

	public InvUpdate Inv;

	public AutoEquipItemSlot(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		Inv = new InvUpdate(_worldPacket);
		Item = _worldPacket.ReadPackedGuid128();
		ItemDstSlot = _worldPacket.ReadUInt8();
	}
}
