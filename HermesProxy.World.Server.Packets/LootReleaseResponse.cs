using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class LootReleaseResponse : ServerPacket
{
	public WowGuid128 LootObj;

	public WowGuid128 Owner;

	public LootReleaseResponse()
		: base(Opcode.SMSG_LOOT_RELEASE)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(LootObj);
		_worldPacket.WritePackedGuid128(Owner);
	}
}
