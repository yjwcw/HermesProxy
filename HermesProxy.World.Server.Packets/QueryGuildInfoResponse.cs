using System;
using System.Collections.Generic;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class QueryGuildInfoResponse : ServerPacket
{
	public class GuildInfo
	{
		public struct RankInfo
		{
			public uint RankID;

			public uint RankOrder;

			public string RankName;

			public RankInfo(uint id, uint order, string name)
			{
				RankID = id;
				RankOrder = order;
				RankName = name;
			}
		}

		public WowGuid128 GuildGuid;

		public uint VirtualRealmAddress;

		public uint EmblemStyle;

		public uint EmblemColor;

		public uint BorderStyle;

		public uint BorderColor;

		public uint BackgroundColor;

		public List<RankInfo> Ranks = new List<RankInfo>();

		public string GuildName = "";
	}

	public WowGuid128 GuildGUID;

	public WowGuid128 PlayerGuid;

	public GuildInfo Info = new GuildInfo();

	public bool HasGuildInfo;

	public QueryGuildInfoResponse()
		: base(Opcode.SMSG_QUERY_GUILD_INFO_RESPONSE)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(GuildGUID);
		if (ModernVersion.RemovedInVersion(9, 2, 0, 1, 14, 2, 2, 5, 3))
		{
			_worldPacket.WritePackedGuid128(PlayerGuid);
		}
		_worldPacket.WriteBit(HasGuildInfo);
		_worldPacket.FlushBits();
		if (!HasGuildInfo)
		{
			return;
		}
		_worldPacket.WritePackedGuid128(Info.GuildGuid);
		_worldPacket.WriteUInt32(Info.VirtualRealmAddress);
		_worldPacket.WriteInt32(Info.Ranks.Count);
		_worldPacket.WriteUInt32(Info.EmblemStyle);
		_worldPacket.WriteUInt32(Info.EmblemColor);
		_worldPacket.WriteUInt32(Info.BorderStyle);
		_worldPacket.WriteUInt32(Info.BorderColor);
		_worldPacket.WriteUInt32(Info.BackgroundColor);
		_worldPacket.WriteBits(Info.GuildName.GetByteCount(), 7);
		_worldPacket.FlushBits();
		foreach (GuildInfo.RankInfo rank in Info.Ranks)
		{
			_worldPacket.WriteUInt32(rank.RankID);
			_worldPacket.WriteUInt32(rank.RankOrder);
			_worldPacket.WriteBits(rank.RankName.GetByteCount(), 7);
			_worldPacket.WriteString(rank.RankName);
		}
		_worldPacket.WriteString(Info.GuildName);
	}
}
