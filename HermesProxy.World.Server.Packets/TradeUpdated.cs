using System.Collections.Generic;
using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class TradeUpdated : ServerPacket
{
	public class UnwrappedTradeItem
	{
		public int EnchantID;

		public int OnUseEnchantmentID;

		public WowGuid128 Creator;

		public int Charges;

		public bool Lock;

		public uint MaxDurability;

		public uint Durability;

		public List<ItemGemData> Gems = new List<ItemGemData>();

		public void Write(WorldPacket data)
		{
			data.WriteInt32(EnchantID);
			data.WriteInt32(OnUseEnchantmentID);
			data.WritePackedGuid128(Creator);
			data.WriteInt32(Charges);
			data.WriteUInt32(MaxDurability);
			data.WriteUInt32(Durability);
			data.WriteBits(Gems.Count, 2);
			data.WriteBit(Lock);
			data.FlushBits();
			foreach (ItemGemData gem in Gems)
			{
				gem.Write(data);
			}
		}
	}

	public class TradeItem
	{
		public byte Slot;

		public ItemInstance Item = new ItemInstance();

		public int StackCount;

		public WowGuid128 GiftCreator;

		public UnwrappedTradeItem Unwrapped;

		public void Write(WorldPacket data)
		{
			data.WriteUInt8(Slot);
			data.WriteInt32(StackCount);
			data.WritePackedGuid128(GiftCreator);
			Item.Write(data);
			data.WriteBit(Unwrapped != null);
			data.FlushBits();
			if (Unwrapped != null)
			{
				Unwrapped.Write(data);
			}
		}
	}

	public ulong Gold;

	public uint CurrentStateIndex;

	public byte WhichPlayer;

	public uint ClientStateIndex;

	public List<TradeItem> Items = new List<TradeItem>();

	public int CurrencyType;

	public uint Id;

	public int ProposedEnchantment;

	public int CurrencyQuantity;

	public TradeUpdated()
		: base(Opcode.SMSG_TRADE_UPDATED, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt8(WhichPlayer);
		_worldPacket.WriteUInt32(Id);
		_worldPacket.WriteUInt32(ClientStateIndex);
		_worldPacket.WriteUInt32(CurrentStateIndex);
		_worldPacket.WriteUInt64(Gold);
		_worldPacket.WriteInt32(CurrencyType);
		_worldPacket.WriteInt32(CurrencyQuantity);
		_worldPacket.WriteInt32(ProposedEnchantment);
		_worldPacket.WriteInt32(Items.Count);
		Items.ForEach(delegate(TradeItem item)
		{
			item.Write(_worldPacket);
		});
	}
}
