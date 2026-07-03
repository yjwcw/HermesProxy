using System;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class GuildEventTabModified : ServerPacket
{
	public int Tab;

	public string Name;

	public string Icon;

	public GuildEventTabModified()
		: base(Opcode.SMSG_GUILD_EVENT_TAB_MODIFIED)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteInt32(Tab);
		_worldPacket.WriteBits(Name.GetByteCount(), 7);
		_worldPacket.WriteBits(Icon.GetByteCount(), 9);
		_worldPacket.FlushBits();
		_worldPacket.WriteString(Name);
		_worldPacket.WriteString(Icon);
	}
}
