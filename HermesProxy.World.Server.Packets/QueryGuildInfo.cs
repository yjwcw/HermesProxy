namespace HermesProxy.World.Server.Packets;

public class QueryGuildInfo : ClientPacket
{
	public WowGuid128 GuildGuid;

	public WowGuid128 PlayerGuid;

	public QueryGuildInfo(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		GuildGuid = _worldPacket.ReadPackedGuid128();
		PlayerGuid = _worldPacket.ReadPackedGuid128();
	}
}
