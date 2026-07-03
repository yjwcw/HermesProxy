using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class LootRollWon : ServerPacket
{
	public WowGuid128 LootObj;

	public WowGuid128 Winner;

	public int Roll;

	public RollType RollType;

	public LootItemData Item = new LootItemData();

	public byte MainSpec;

	public LootRollWon()
		: base(Opcode.SMSG_LOOT_ROLL_WON)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(LootObj);
		_worldPacket.WritePackedGuid128(Winner);
		_worldPacket.WriteInt32(Roll);
		_worldPacket.WriteUInt8((byte)RollType);
		Item.Write(_worldPacket);
		_worldPacket.WriteUInt8(MainSpec);
	}
}
