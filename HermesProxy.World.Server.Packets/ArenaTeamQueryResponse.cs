using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class ArenaTeamQueryResponse : ServerPacket
{
	public uint TeamId;

	public ArenaTeamEmblem Emblem;

	public ArenaTeamQueryResponse()
		: base(Opcode.SMSG_QUERY_ARENA_TEAM_RESPONSE)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt32(TeamId);
		_worldPacket.WriteBit(Emblem != null);
		_worldPacket.FlushBits();
		if (Emblem != null)
		{
			Emblem.Write(_worldPacket);
		}
	}
}
