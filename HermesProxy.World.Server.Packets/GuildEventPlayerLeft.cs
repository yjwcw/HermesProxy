using System;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class GuildEventPlayerLeft : ServerPacket
{
	public bool Removed;

	public WowGuid128 RemoverGUID;

	public uint RemoverVirtualRealmAddress;

	public string RemoverName;

	public WowGuid128 LeaverGUID;

	public uint LeaverVirtualRealmAddress;

	public string LeaverName;

	public GuildEventPlayerLeft()
		: base(Opcode.SMSG_GUILD_EVENT_PLAYER_LEFT)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteBit(Removed);
		_worldPacket.WriteBits(LeaverName.GetByteCount(), 6);
		if (Removed)
		{
			_worldPacket.WriteBits(RemoverName.GetByteCount(), 6);
			_worldPacket.WritePackedGuid128(RemoverGUID);
			_worldPacket.WriteUInt32(RemoverVirtualRealmAddress);
			_worldPacket.WriteString(RemoverName);
		}
		_worldPacket.WritePackedGuid128(LeaverGUID);
		_worldPacket.WriteUInt32(LeaverVirtualRealmAddress);
		_worldPacket.WriteString(LeaverName);
	}
}
