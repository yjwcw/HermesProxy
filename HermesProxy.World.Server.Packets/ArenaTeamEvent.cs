using System;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class ArenaTeamEvent : ServerPacket
{
	public ArenaTeamEventModern Event;

	public string Param1 = "";

	public string Param2 = "";

	public string Param3 = "";

	public ArenaTeamEvent()
		: base(Opcode.SMSG_ARENA_TEAM_EVENT)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt8((byte)Event);
		_worldPacket.WriteBits(Param1.GetByteCount(), 9);
		_worldPacket.WriteBits(Param2.GetByteCount(), 9);
		_worldPacket.WriteBits(Param3.GetByteCount(), 9);
		_worldPacket.FlushBits();
		_worldPacket.WriteString(Param1);
		_worldPacket.WriteString(Param2);
		_worldPacket.WriteString(Param3);
	}
}
