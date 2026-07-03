using System;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class PrintNotification : ServerPacket
{
	public string NotifyText;

	public PrintNotification()
		: base(Opcode.SMSG_PRINT_NOTIFICATION)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteBits(NotifyText.GetByteCount(), 12);
		_worldPacket.WriteString(NotifyText);
	}
}
