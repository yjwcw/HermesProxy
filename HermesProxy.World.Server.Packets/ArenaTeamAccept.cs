namespace HermesProxy.World.Server.Packets;

public class ArenaTeamAccept : ClientPacket
{
	public WowGuid128 PlayerGuid;

	public WowGuid128 TeamGuid;

	public ArenaTeamAccept(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		PlayerGuid = _worldPacket.ReadPackedGuid128();
		TeamGuid = _worldPacket.ReadPackedGuid128();
	}
}
