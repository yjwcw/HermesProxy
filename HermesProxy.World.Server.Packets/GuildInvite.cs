using System;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class GuildInvite : ServerPacket
{
	public WowGuid128 GuildGUID;

	public WowGuid128 OldGuildGUID = WowGuid128.Empty;

	public uint EmblemColor;

	public uint EmblemStyle;

	public uint BorderStyle;

	public uint BorderColor;

	public uint BackgroundColor;

	public int AchievementPoints = -1;

	public uint GuildVirtualRealmAddress;

	public uint OldGuildVirtualRealmAddress;

	public uint InviterVirtualRealmAddress;

	public string InviterName;

	public string GuildName;

	public string OldGuildName = "";

	public GuildInvite()
		: base(Opcode.SMSG_GUILD_INVITE)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteBits(InviterName.GetByteCount(), 6);
		_worldPacket.WriteBits(GuildName.GetByteCount(), 7);
		_worldPacket.WriteBits(OldGuildName.GetByteCount(), 7);
		_worldPacket.WriteUInt32(InviterVirtualRealmAddress);
		_worldPacket.WriteUInt32(GuildVirtualRealmAddress);
		_worldPacket.WritePackedGuid128(GuildGUID);
		_worldPacket.WriteUInt32(OldGuildVirtualRealmAddress);
		_worldPacket.WritePackedGuid128(OldGuildGUID);
		_worldPacket.WriteUInt32(EmblemStyle);
		_worldPacket.WriteUInt32(EmblemColor);
		_worldPacket.WriteUInt32(BorderStyle);
		_worldPacket.WriteUInt32(BorderColor);
		_worldPacket.WriteUInt32(BackgroundColor);
		_worldPacket.WriteInt32(AchievementPoints);
		_worldPacket.WriteString(InviterName);
		_worldPacket.WriteString(GuildName);
		_worldPacket.WriteString(OldGuildName);
	}
}
