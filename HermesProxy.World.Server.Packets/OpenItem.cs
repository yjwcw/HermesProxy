namespace HermesProxy.World.Server.Packets;

public class OpenItem : ClientPacket
{
	public byte PackSlot;

	public byte Slot;

	public OpenItem(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		PackSlot = _worldPacket.ReadUInt8();
		Slot = _worldPacket.ReadUInt8();
	}
}
