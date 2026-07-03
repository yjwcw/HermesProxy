using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class LootAllPassed : ServerPacket
{
	public WowGuid128 LootObj;

	public LootItemData Item = new LootItemData();

	public LootAllPassed()
		: base(Opcode.SMSG_LOOT_ALL_PASSED)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(LootObj);
		Item.Write(_worldPacket);
	}
}
