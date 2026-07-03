namespace HermesProxy.World.Server.Packets;

public class ArenaTeamRemove : ClientPacket
{
	public uint TeamId;

	public WowGuid128 PlayerGuid;

	public ArenaTeamRemove(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		TeamId = _worldPacket.ReadUInt32();
		PlayerGuid = _worldPacket.ReadPackedGuid128();
	}
}
