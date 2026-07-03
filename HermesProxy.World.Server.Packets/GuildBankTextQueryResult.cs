using System;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class GuildBankTextQueryResult : ServerPacket
{
	public int Tab;

	public string Text;

	public GuildBankTextQueryResult()
		: base(Opcode.SMSG_GUILD_BANK_TEXT_QUERY_RESULT)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteInt32(Tab);
		_worldPacket.WriteBits(Text.GetByteCount(), 14);
		_worldPacket.WriteString(Text);
	}
}
