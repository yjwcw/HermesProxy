using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class LootRemoved : ServerPacket
{
	public WowGuid128 Owner;

	public WowGuid128 LootObj;

	public byte LootListID;

	public LootRemoved()
		: base(Opcode.SMSG_LOOT_REMOVED, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(Owner);
		_worldPacket.WritePackedGuid128(LootObj);
		_worldPacket.WriteUInt8(LootListID);
	}
}
