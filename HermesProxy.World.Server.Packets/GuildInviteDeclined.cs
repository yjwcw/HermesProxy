using System;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class GuildInviteDeclined : ServerPacket
{
	public bool AutoDecline;

	public uint InviterVirtualRealmAddress;

	public string InviterName;

	public GuildInviteDeclined()
		: base(Opcode.SMSG_GUILD_INVITE_DECLINED)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteBits(InviterName.GetByteCount(), 6);
		_worldPacket.WriteBit(AutoDecline);
		_worldPacket.FlushBits();
		_worldPacket.WriteUInt32(InviterVirtualRealmAddress);
		_worldPacket.WriteString(InviterName);
	}
}
