using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class ItemPushResult : ServerPacket
{
	public enum DisplayType
	{
		Hidden,
		Received,
		EncounterLoot,
		Loot
	}

	public WowGuid128 PlayerGUID;

	public byte Slot;

	public int SlotInBag;

	public ItemInstance Item = new ItemInstance();

	public int QuestLogItemID;

	public uint Quantity;

	public uint QuantityInInventory;

	public int DungeonEncounterID;

	public int BattlePetSpeciesID;

	public int BattlePetBreedID;

	public uint BattlePetBreedQuality;

	public int BattlePetLevel;

	public WowGuid128 ItemGUID;

	public bool Pushed;

	public DisplayType DisplayText;

	public bool Created;

	public bool IsBonusRoll;

	public bool IsEncounterLoot;

	public ItemPushResult()
		: base(Opcode.SMSG_ITEM_PUSH_RESULT)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(PlayerGUID);
		_worldPacket.WriteUInt8(Slot);
		_worldPacket.WriteInt32(SlotInBag);
		_worldPacket.WriteInt32(QuestLogItemID);
		_worldPacket.WriteUInt32(Quantity);
		_worldPacket.WriteUInt32(QuantityInInventory);
		_worldPacket.WriteInt32(DungeonEncounterID);
		_worldPacket.WriteInt32(BattlePetSpeciesID);
		_worldPacket.WriteInt32(BattlePetBreedID);
		_worldPacket.WriteUInt32(BattlePetBreedQuality);
		_worldPacket.WriteInt32(BattlePetLevel);
		_worldPacket.WritePackedGuid128(ItemGUID);
		_worldPacket.WriteBit(Pushed);
		_worldPacket.WriteBit(Created);
		_worldPacket.WriteBits((uint)DisplayText, 3);
		_worldPacket.WriteBit(IsBonusRoll);
		_worldPacket.WriteBit(IsEncounterLoot);
		_worldPacket.FlushBits();
		Item.Write(_worldPacket);
	}
}
