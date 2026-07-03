namespace HermesProxy.World.Server.Packets;

public class GuildOfficerRemoveMember : ClientPacket
{
	public WowGuid128 Removee;

	public GuildOfficerRemoveMember(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		Removee = _worldPacket.ReadPackedGuid128();
	}
}
