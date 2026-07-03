using System;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class GuildEventPlayerJoined : ServerPacket
{
	public WowGuid128 Guid;

	public uint VirtualRealmAddress;

	public string Name;

	public GuildEventPlayerJoined()
		: base(Opcode.SMSG_GUILD_EVENT_PLAYER_JOINED)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(Guid);
		_worldPacket.WriteUInt32(VirtualRealmAddress);
		_worldPacket.WriteBits(Name.GetByteCount(), 6);
		_worldPacket.WriteString(Name);
	}
}
