using System;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class GuildEventMotd : ServerPacket
{
	public string MotdText;

	public GuildEventMotd()
		: base(Opcode.SMSG_GUILD_EVENT_MOTD)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteBits(MotdText.GetByteCount(), 11);
		_worldPacket.WriteString(MotdText);
	}
}
