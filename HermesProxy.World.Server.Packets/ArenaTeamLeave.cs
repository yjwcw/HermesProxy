namespace HermesProxy.World.Server.Packets;

public class ArenaTeamLeave : ClientPacket
{
	public uint TeamId;

	public ArenaTeamLeave(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		TeamId = _worldPacket.ReadUInt32();
	}
}
