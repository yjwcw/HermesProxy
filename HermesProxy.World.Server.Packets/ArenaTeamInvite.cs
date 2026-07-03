using System;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class ArenaTeamInvite : ServerPacket
{
	public WowGuid128 PlayerGuid;

	public uint PlayerVirtualAddress;

	public WowGuid128 TeamGuid;

	public string PlayerName;

	public string TeamName;

	public ArenaTeamInvite()
		: base(Opcode.SMSG_ARENA_TEAM_INVITE)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(PlayerGuid);
		_worldPacket.WriteUInt32(PlayerVirtualAddress);
		_worldPacket.WritePackedGuid128(TeamGuid);
		_worldPacket.WriteBits(PlayerName.GetByteCount(), 6);
		_worldPacket.WriteBits(TeamName.GetByteCount(), 7);
		_worldPacket.FlushBits();
		_worldPacket.WriteString(PlayerName);
		_worldPacket.WriteString(TeamName);
	}
}
