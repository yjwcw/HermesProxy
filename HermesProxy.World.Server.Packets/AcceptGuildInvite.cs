namespace HermesProxy.World.Server.Packets;

public class AcceptGuildInvite : ClientPacket
{
	public AcceptGuildInvite(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
	}
}
