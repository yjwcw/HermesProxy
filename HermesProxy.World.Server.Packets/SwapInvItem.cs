namespace HermesProxy.World.Server.Packets;

public class SwapInvItem : ClientPacket
{
	public InvUpdate Inv;

	public byte Slot1;

	public byte Slot2;

	public SwapInvItem(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		Inv = new InvUpdate(_worldPacket);
		Slot2 = _worldPacket.ReadUInt8();
		Slot1 = _worldPacket.ReadUInt8();
	}
}
