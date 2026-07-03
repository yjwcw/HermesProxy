using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class LootItemData
{
	public byte Type;

	public LootSlotTypeModern UIType;

	public uint Quantity;

	public byte LootItemType;

	public byte LootListID;

	public bool CanTradeToTapList;

	public ItemInstance Loot = new ItemInstance();

	public void Write(WorldPacket data)
	{
		data.WriteBits(Type, 2);
		data.WriteBits(UIType, 3);
		data.WriteBit(CanTradeToTapList);
		data.FlushBits();
		Loot.Write(data);
		data.WriteUInt32(Quantity);
		data.WriteUInt8(LootItemType);
		data.WriteUInt8(LootListID);
	}
}
