namespace HermesProxy.World.Server.Packets;

public class ArenaTeamRosterRequest : ClientPacket
{
	public uint TeamIndex;

	public ArenaTeamRosterRequest(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		TeamIndex = _worldPacket.ReadUInt32();
	}
}
