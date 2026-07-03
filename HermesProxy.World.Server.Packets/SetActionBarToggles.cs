namespace HermesProxy.World.Server.Packets;

public class SetActionBarToggles : ClientPacket
{
	public byte Mask;

	public SetActionBarToggles(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		Mask = _worldPacket.ReadUInt8();
	}
}
