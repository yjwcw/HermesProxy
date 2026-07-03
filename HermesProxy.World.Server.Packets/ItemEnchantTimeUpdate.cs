using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class ItemEnchantTimeUpdate : ServerPacket
{
	public WowGuid128 ItemGuid;

	public uint DurationLeft;

	public uint Slot;

	public WowGuid128 OwnerGuid;

	public ItemEnchantTimeUpdate()
		: base(Opcode.SMSG_ITEM_ENCHANT_TIME_UPDATE, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(ItemGuid);
		_worldPacket.WriteUInt32(DurationLeft);
		_worldPacket.WriteUInt32(Slot);
		_worldPacket.WritePackedGuid128(OwnerGuid);
	}
}
