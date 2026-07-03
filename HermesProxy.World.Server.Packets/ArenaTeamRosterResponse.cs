using System.Collections.Generic;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class ArenaTeamRosterResponse : ServerPacket
{
	public uint TeamId;

	public uint TeamSize;

	public uint TeamPlayed;

	public uint TeamWins;

	public uint SeasonPlayed;

	public uint SeasonWins;

	public uint TeamRating;

	public uint PlayerRating;

	public bool UnkBit;

	public List<ArenaTeamMember> Members = new List<ArenaTeamMember>();

	public ArenaTeamRosterResponse()
		: base(Opcode.SMSG_ARENA_TEAM_ROSTER)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt32(TeamId);
		_worldPacket.WriteUInt32(TeamSize);
		_worldPacket.WriteUInt32(TeamPlayed);
		_worldPacket.WriteUInt32(TeamWins);
		_worldPacket.WriteUInt32(SeasonPlayed);
		_worldPacket.WriteUInt32(SeasonWins);
		_worldPacket.WriteUInt32(TeamRating);
		_worldPacket.WriteUInt32(PlayerRating);
		_worldPacket.WriteInt32(Members.Count);
		if (ModernVersion.AddedInClassicVersion(1, 14, 2, 2, 5, 3))
		{
			_worldPacket.WriteBit(UnkBit);
			_worldPacket.FlushBits();
		}
		foreach (ArenaTeamMember member in Members)
		{
			member.Write(_worldPacket);
		}
	}
}
