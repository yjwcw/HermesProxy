using System;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class GuildCommandResult : ServerPacket
{
	public string Name;

	public GuildCommandError Result;

	public GuildCommandType Command;

	public GuildCommandResult()
		: base(Opcode.SMSG_GUILD_COMMAND_RESULT)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt32((uint)Result);
		_worldPacket.WriteUInt32((uint)Command);
		_worldPacket.WriteBits(Name.GetByteCount(), 8);
		_worldPacket.WriteString(Name);
	}
}
