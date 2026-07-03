namespace HermesProxy.World.Server.Packets;

public class GuildPermissionsQuery : ClientPacket
{
	public GuildPermissionsQuery(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
	}
}
