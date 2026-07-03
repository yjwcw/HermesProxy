namespace HermesProxy.World.Server.Packets;

public class AttackStop : ClientPacket
{
	public AttackStop(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
	}
}
