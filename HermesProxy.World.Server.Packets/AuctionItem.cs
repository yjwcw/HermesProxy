using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets;

public class AuctionItem
{
	public ItemInstance Item;

	public int Count;

	public int Charges;

	public List<ItemEnchantData> Enchantments = new List<ItemEnchantData>();

	public uint Flags = 196608u;

	public uint AuctionID;

	public WowGuid128 Owner;

	public ulong? MinBid;

	public ulong? MinIncrement;

	public ulong? BuyoutPrice;

	public ulong? UnitPrice;

	public int DurationLeft;

	public byte DeleteReason;

	public bool CensorServerSideInfo;

	public bool CensorBidInfo;

	public WowGuid128 ItemGuid = WowGuid128.Empty;

	public WowGuid128 OwnerAccountID;

	public uint EndTime;

	public WowGuid128 Creator;

	public WowGuid128 Bidder;

	public ulong? BidAmount;

	public List<ItemGemData> Gems = new List<ItemGemData>();

	public AuctionBucketKey AuctionBucketKey;

	public void Write(WorldPacket data)
	{
		data.WriteBit(Item != null);
		data.WriteBits(Enchantments.Count, 4);
		data.WriteBits(Gems.Count, 2);
		data.WriteBit(MinBid.HasValue);
		data.WriteBit(MinIncrement.HasValue);
		data.WriteBit(BuyoutPrice.HasValue);
		data.WriteBit(UnitPrice.HasValue);
		data.WriteBit(CensorServerSideInfo);
		data.WriteBit(CensorBidInfo);
		data.WriteBit(AuctionBucketKey != null);
		data.WriteBit(Creator != null);
		if (!CensorBidInfo)
		{
			data.WriteBit(Bidder != null);
			data.WriteBit(BidAmount.HasValue);
		}
		data.FlushBits();
		if (Item != null)
		{
			Item.Write(data);
		}
		data.WriteInt32(Count);
		data.WriteInt32(Charges);
		data.WriteUInt32(Flags);
		data.WriteUInt32(AuctionID);
		data.WritePackedGuid128(Owner);
		data.WriteInt32(DurationLeft);
		data.WriteUInt8(DeleteReason);
		foreach (ItemEnchantData enchantment in Enchantments)
		{
			enchantment.Write(data);
		}
		if (MinBid.HasValue)
		{
			data.WriteUInt64(MinBid.Value);
		}
		if (MinIncrement.HasValue)
		{
			data.WriteUInt64(MinIncrement.Value);
		}
		if (BuyoutPrice.HasValue)
		{
			data.WriteUInt64(BuyoutPrice.Value);
		}
		if (UnitPrice.HasValue)
		{
			data.WriteUInt64(UnitPrice.Value);
		}
		if (!CensorServerSideInfo)
		{
			data.WritePackedGuid128(ItemGuid);
			data.WritePackedGuid128(OwnerAccountID);
			data.WriteUInt32(EndTime);
		}
		if (Creator != null)
		{
			data.WritePackedGuid128(Creator);
		}
		if (!CensorBidInfo)
		{
			if (Bidder != null)
			{
				data.WritePackedGuid128(Bidder);
			}
			if (BidAmount.HasValue)
			{
				data.WriteUInt64(BidAmount.Value);
			}
		}
		foreach (ItemGemData gem in Gems)
		{
			gem.Write(data);
		}
		if (AuctionBucketKey != null)
		{
			AuctionBucketKey.Write(data);
		}
	}
}
