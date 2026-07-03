namespace HermesProxy.World.Server.Packets;

public class SwapItem : ClientPacket
{
	public InvUpdate Inv;

	public byte SlotA;

	public byte ContainerSlotB;

	public byte SlotB;

	public byte ContainerSlotA;

	public SwapItem(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		Inv = new InvUpdate(_worldPacket);
		ContainerSlotB = _worldPacket.ReadUInt8();
		ContainerSlotA = _worldPacket.ReadUInt8();
		SlotB = _worldPacket.ReadUInt8();
		SlotA = _worldPacket.ReadUInt8();
	}
}
