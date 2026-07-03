using System;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class GuildEventNewLeader : ServerPacket
{
	public bool SelfPromoted;

	public WowGuid128 NewLeaderGUID;

	public uint NewLeaderVirtualRealmAddress;

	public string NewLeaderName;

	public WowGuid128 OldLeaderGUID;

	public uint OldLeaderVirtualRealmAddress;

	public string OldLeaderName;

	public GuildEventNewLeader()
		: base(Opcode.SMSG_GUILD_EVENT_NEW_LEADER)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteBit(SelfPromoted);
		_worldPacket.WriteBits(OldLeaderName.GetByteCount(), 6);
		_worldPacket.WriteBits(NewLeaderName.GetByteCount(), 6);
		_worldPacket.WritePackedGuid128(OldLeaderGUID);
		_worldPacket.WriteUInt32(OldLeaderVirtualRealmAddress);
		_worldPacket.WritePackedGuid128(NewLeaderGUID);
		_worldPacket.WriteUInt32(NewLeaderVirtualRealmAddress);
		_worldPacket.WriteString(OldLeaderName);
		_worldPacket.WriteString(NewLeaderName);
	}
}
