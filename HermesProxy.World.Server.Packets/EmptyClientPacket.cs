namespace HermesProxy.World.Server.Packets;

public class EmptyClientPacket : ClientPacket
{
	public EmptyClientPacket(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
	}
}
