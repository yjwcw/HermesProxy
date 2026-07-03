using System.Collections.Generic;
using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class LootResponse : ServerPacket
{
	public WowGuid128 Owner;

	public WowGuid128 LootObj;

	public LootError FailureReason = LootError.NoLoot;

	public LootType AcquireReason;

	public LootMethod LootMethod;

	public byte Threshold = 2;

	public uint Coins;

	public List<LootItemData> Items = new List<LootItemData>();

	public List<LootCurrency> Currencies = new List<LootCurrency>();

	public bool Acquired = true;

	public bool AELooting;

	public LootResponse()
		: base(Opcode.SMSG_LOOT_RESPONSE, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(Owner);
		_worldPacket.WritePackedGuid128(LootObj);
		_worldPacket.WriteUInt8((byte)FailureReason);
		_worldPacket.WriteUInt8((byte)AcquireReason);
		_worldPacket.WriteUInt8((byte)LootMethod);
		_worldPacket.WriteUInt8(Threshold);
		_worldPacket.WriteUInt32(Coins);
		_worldPacket.WriteInt32(Items.Count);
		_worldPacket.WriteInt32(Currencies.Count);
		_worldPacket.WriteBit(Acquired);
		_worldPacket.WriteBit(AELooting);
		_worldPacket.FlushBits();
		foreach (LootItemData item in Items)
		{
			item.Write(_worldPacket);
		}
		foreach (LootCurrency currency in Currencies)
		{
			_worldPacket.WriteUInt32(currency.CurrencyID);
			_worldPacket.WriteUInt32(currency.Quantity);
			_worldPacket.WriteUInt8(currency.LootListID);
			_worldPacket.WriteBits(currency.UIType, 3);
			_worldPacket.FlushBits();
		}
	}
}
