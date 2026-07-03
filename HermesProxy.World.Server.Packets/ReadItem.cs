namespace HermesProxy.World.Server.Packets;

internal class ReadItem : ClientPacket
{
	public byte PackSlot;

	public byte Slot;

	public ReadItem(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		PackSlot = _worldPacket.ReadUInt8();
		Slot = _worldPacket.ReadUInt8();
	}
}
