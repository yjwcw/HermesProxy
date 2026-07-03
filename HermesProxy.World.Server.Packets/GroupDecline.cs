using System;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class GroupDecline : ServerPacket
{
	public string Name;

	public GroupDecline()
		: base(Opcode.SMSG_GROUP_DECLINE)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteBits(Name.GetByteCount(), 9);
		_worldPacket.FlushBits();
		_worldPacket.WriteString(Name);
	}
}
