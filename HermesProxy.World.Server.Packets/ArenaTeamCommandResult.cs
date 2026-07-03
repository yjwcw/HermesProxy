using System;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class ArenaTeamCommandResult : ServerPacket
{
	public ArenaTeamCommandType Action;

	public ArenaTeamCommandErrorModern Error;

	public string TeamName;

	public string PlayerName;

	public ArenaTeamCommandResult()
		: base(Opcode.SMSG_ARENA_TEAM_COMMAND_RESULT)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt8((byte)Action);
		_worldPacket.WriteUInt8((byte)Error);
		_worldPacket.WriteBits(TeamName.GetByteCount(), 7);
		_worldPacket.WriteBits(PlayerName.GetByteCount(), 6);
		_worldPacket.FlushBits();
		_worldPacket.WriteString(TeamName);
		_worldPacket.WriteString(PlayerName);
	}
}
