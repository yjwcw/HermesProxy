namespace HermesProxy.World.Server.Packets;

public class GuildGetRoster : ClientPacket
{
	public GuildGetRoster(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
	}
}
