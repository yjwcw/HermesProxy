namespace HermesProxy.World.Server.Packets;

public class GuildGetRanks : ClientPacket
{
	public WowGuid128 GuildGUID;

	public GuildGetRanks(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		GuildGUID = _worldPacket.ReadPackedGuid128();
	}
}
