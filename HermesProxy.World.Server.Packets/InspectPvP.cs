using System.Collections.Generic;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class InspectPvP : ServerPacket
{
	public WowGuid128 PlayerGUID;

	public List<PvPBracketInspectData> Brackets = new List<PvPBracketInspectData>();

	public List<ArenaTeamInspectData> ArenaTeams = new List<ArenaTeamInspectData>();

	public InspectPvP()
		: base(Opcode.SMSG_INSPECT_PVP)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(PlayerGUID);
		_worldPacket.WriteBits(Brackets.Count, 3);
		_worldPacket.WriteBits(ArenaTeams.Count, 2);
		_worldPacket.FlushBits();
		foreach (PvPBracketInspectData bracket in Brackets)
		{
			bracket.Write(_worldPacket);
		}
		foreach (ArenaTeamInspectData arenaTeam in ArenaTeams)
		{
			arenaTeam.Write(_worldPacket);
		}
	}
}
