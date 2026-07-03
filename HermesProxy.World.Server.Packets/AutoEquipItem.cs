namespace HermesProxy.World.Server.Packets;

public class AutoEquipItem : ClientPacket
{
	public byte Slot;

	public InvUpdate Inv;

	public byte PackSlot;

	public AutoEquipItem(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		Inv = new InvUpdate(_worldPacket);
		PackSlot = _worldPacket.ReadUInt8();
		Slot = _worldPacket.ReadUInt8();
	}
}
