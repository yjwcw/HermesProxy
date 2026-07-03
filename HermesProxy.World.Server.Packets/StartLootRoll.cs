using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class StartLootRoll : ServerPacket
{
	public WowGuid128 LootObj;

	public uint MapID;

	public uint RollTime;

	public LootMethod Method = LootMethod.GroupLoot;

	public RollMask ValidRolls;

	public LootItemData Item = new LootItemData();

	public StartLootRoll()
		: base(Opcode.SMSG_LOOT_START_ROLL)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(LootObj);
		_worldPacket.WriteUInt32(MapID);
		_worldPacket.WriteUInt32(RollTime);
		_worldPacket.WriteUInt8((byte)ValidRolls);
		_worldPacket.WriteUInt8((byte)Method);
		Item.Write(_worldPacket);
	}
}
