using System;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class GroupNewLeader : ServerPacket
{
	public sbyte PartyIndex;

	public string Name;

	public GroupNewLeader()
		: base(Opcode.SMSG_GROUP_NEW_LEADER)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteInt8(PartyIndex);
		_worldPacket.WriteBits(Name.GetByteCount(), 9);
		_worldPacket.WriteString(Name);
	}
}
