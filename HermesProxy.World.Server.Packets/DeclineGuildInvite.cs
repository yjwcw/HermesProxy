namespace HermesProxy.World.Server.Packets;

public class DeclineGuildInvite : ClientPacket
{
	public DeclineGuildInvite(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
	}
}
