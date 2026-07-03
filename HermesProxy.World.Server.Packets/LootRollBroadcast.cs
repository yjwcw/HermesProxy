using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class LootRollBroadcast : ServerPacket
{
	public WowGuid128 LootObj;

	public WowGuid128 Player;

	public int Roll;

	public RollType RollType;

	public LootItemData Item = new LootItemData();

	public bool Autopassed;

	public LootRollBroadcast()
		: base(Opcode.SMSG_LOOT_ROLL)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(LootObj);
		_worldPacket.WritePackedGuid128(Player);
		_worldPacket.WriteInt32(Roll);
		_worldPacket.WriteUInt8((byte)RollType);
		Item.Write(_worldPacket);
		_worldPacket.WriteBit(Autopassed);
		_worldPacket.FlushBits();
	}
}
