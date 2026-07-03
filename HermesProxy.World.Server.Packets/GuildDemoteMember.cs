namespace HermesProxy.World.Server.Packets;

public class GuildDemoteMember : ClientPacket
{
	public WowGuid128 Demotee;

	public GuildDemoteMember(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		Demotee = _worldPacket.ReadPackedGuid128();
	}
}
