using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class LootRollsComplete : ServerPacket
{
	public WowGuid128 LootObj;

	public byte LootListID;

	public LootRollsComplete()
		: base(Opcode.SMSG_LOOT_ROLLS_COMPLETE)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(LootObj);
		_worldPacket.WriteUInt8(LootListID);
	}
}
