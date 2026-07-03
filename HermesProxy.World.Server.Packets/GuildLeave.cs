namespace HermesProxy.World.Server.Packets;

public class GuildLeave : ClientPacket
{
	public GuildLeave(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
	}
}
