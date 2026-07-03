using System;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class DefenseMessage : ServerPacket
{
	public uint ZoneID;

	public string MessageText = "";

	public DefenseMessage()
		: base(Opcode.SMSG_DEFENSE_MESSAGE)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt32(ZoneID);
		_worldPacket.WriteBits(MessageText.GetByteCount(), 12);
		_worldPacket.FlushBits();
		_worldPacket.WriteString(MessageText);
	}
}
