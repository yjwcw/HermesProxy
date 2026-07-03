namespace HermesProxy.World.Server.Packets;

public class SetActionButton : ClientPacket
{
	public ushort Action;

	public ushort Type;

	public byte Index;

	public SetActionButton(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		Action = _worldPacket.ReadUInt16();
		Type = _worldPacket.ReadUInt16();
		Index = _worldPacket.ReadUInt8();
	}
}
