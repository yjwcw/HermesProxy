using System;
using System.Collections.Generic;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class GuildBankQueryResults : ServerPacket
{
	public List<GuildBankItemInfo> ItemInfo;

	public List<GuildBankTabInfo> TabInfo;

	public int WithdrawalsRemaining;

	public int Tab;

	public ulong Money;

	public bool FullUpdate;

	public GuildBankQueryResults()
		: base(Opcode.SMSG_GUILD_BANK_QUERY_RESULTS)
	{
		ItemInfo = new List<GuildBankItemInfo>();
		TabInfo = new List<GuildBankTabInfo>();
	}

	public override void Write()
	{
		_worldPacket.WriteUInt64(Money);
		_worldPacket.WriteInt32(Tab);
		_worldPacket.WriteInt32(WithdrawalsRemaining);
		_worldPacket.WriteInt32(TabInfo.Count);
		_worldPacket.WriteInt32(ItemInfo.Count);
		_worldPacket.WriteBit(FullUpdate);
		_worldPacket.FlushBits();
		foreach (GuildBankTabInfo item in TabInfo)
		{
			_worldPacket.WriteInt32(item.TabIndex);
			_worldPacket.WriteBits(item.Name.GetByteCount(), 7);
			_worldPacket.WriteBits(item.Icon.GetByteCount(), 9);
			_worldPacket.WriteString(item.Name);
			_worldPacket.WriteString(item.Icon);
		}
		foreach (GuildBankItemInfo item2 in ItemInfo)
		{
			_worldPacket.WriteInt32(item2.Slot);
			_worldPacket.WriteInt32(item2.Count);
			_worldPacket.WriteInt32(item2.EnchantmentID);
			_worldPacket.WriteInt32(item2.Charges);
			_worldPacket.WriteInt32(item2.OnUseEnchantmentID);
			_worldPacket.WriteUInt32(item2.Flags);
			item2.Item.Write(_worldPacket);
			_worldPacket.WriteBits(item2.SocketEnchant.Count, 2);
			_worldPacket.WriteBit(item2.Locked);
			_worldPacket.FlushBits();
			foreach (ItemGemData item3 in item2.SocketEnchant)
			{
				item3.Write(_worldPacket);
			}
		}
	}
}
