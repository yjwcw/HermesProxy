namespace HermesProxy.World.Server.Packets;

public class DestroyItem : ClientPacket
{
	public uint Count;

	public byte SlotNum;

	public byte ContainerId;

	public DestroyItem(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		Count = _worldPacket.ReadUInt32();
		ContainerId = _worldPacket.ReadUInt8();
		SlotNum = _worldPacket.ReadUInt8();
	}
}
