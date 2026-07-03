namespace HermesProxy.World.Server.Packets;

public class AutoDeclineGuildInvite : ClientPacket
{
	public AutoDeclineGuildInvite(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
	}
}
