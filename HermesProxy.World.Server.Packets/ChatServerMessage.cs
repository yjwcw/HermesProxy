using System;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class ChatServerMessage : ServerPacket
{
	public int MessageID;

	public string StringParam = "";

	public ChatServerMessage()
		: base(Opcode.SMSG_CHAT_SERVER_MESSAGE)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteInt32(MessageID);
		_worldPacket.WriteBits(StringParam.GetByteCount(), 11);
		_worldPacket.WriteString(StringParam);
	}
}
