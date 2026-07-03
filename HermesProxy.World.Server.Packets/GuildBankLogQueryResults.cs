using System.Collections.Generic;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class GuildBankLogQueryResults : ServerPacket
{
	public int Tab;

	public List<GuildBankLogEntry> Entry;

	public ulong? WeeklyBonusMoney;

	public GuildBankLogQueryResults()
		: base(Opcode.SMSG_GUILD_BANK_LOG_QUERY_RESULTS)
	{
		Entry = new List<GuildBankLogEntry>();
	}

	public override void Write()
	{
		_worldPacket.WriteInt32(Tab);
		_worldPacket.WriteInt32(Entry.Count);
		_worldPacket.WriteBit(WeeklyBonusMoney.HasValue);
		_worldPacket.FlushBits();
		foreach (GuildBankLogEntry item in Entry)
		{
			_worldPacket.WritePackedGuid128(item.PlayerGUID);
			_worldPacket.WriteUInt32(item.TimeOffset);
			_worldPacket.WriteInt8(item.EntryType);
			_worldPacket.WriteBit(item.Money.HasValue);
			_worldPacket.WriteBit(item.ItemID.HasValue);
			_worldPacket.WriteBit(item.Count.HasValue);
			_worldPacket.WriteBit(item.OtherTab.HasValue);
			_worldPacket.FlushBits();
			if (item.Money.HasValue)
			{
				_worldPacket.WriteUInt64(item.Money.Value);
			}
			if (item.ItemID.HasValue)
			{
				_worldPacket.WriteInt32(item.ItemID.Value);
			}
			if (item.Count.HasValue)
			{
				_worldPacket.WriteInt32(item.Count.Value);
			}
			if (item.OtherTab.HasValue)
			{
				_worldPacket.WriteInt8(item.OtherTab.Value);
			}
		}
		if (WeeklyBonusMoney.HasValue)
		{
			_worldPacket.WriteUInt64(WeeklyBonusMoney.Value);
		}
	}
}
