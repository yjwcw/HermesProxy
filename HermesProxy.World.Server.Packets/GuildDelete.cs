namespace HermesProxy.World.Server.Packets;

public class GuildDelete : ClientPacket
{
	public GuildDelete(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
	}
}
