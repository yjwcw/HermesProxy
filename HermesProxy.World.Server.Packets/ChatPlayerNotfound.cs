using System;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class ChatPlayerNotfound : ServerPacket
{
	public string Name;

	public ChatPlayerNotfound()
		: base(Opcode.SMSG_CHAT_PLAYER_NOTFOUND)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteBits(Name.GetByteCount(), 9);
		_worldPacket.WriteString(Name);
	}
}
