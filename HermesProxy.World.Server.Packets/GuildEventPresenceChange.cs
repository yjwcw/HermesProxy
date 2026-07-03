using System;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class GuildEventPresenceChange : ServerPacket
{
	public WowGuid128 Guid;

	public uint VirtualRealmAddress;

	public bool LoggedOn;

	public bool Mobile;

	public string Name;

	public GuildEventPresenceChange()
		: base(Opcode.SMSG_GUILD_EVENT_PRESENCE_CHANGE)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(Guid);
		_worldPacket.WriteUInt32(VirtualRealmAddress);
		_worldPacket.WriteBits(Name.GetByteCount(), 6);
		_worldPacket.WriteBit(LoggedOn);
		_worldPacket.WriteBit(Mobile);
		_worldPacket.WriteString(Name);
	}
}
