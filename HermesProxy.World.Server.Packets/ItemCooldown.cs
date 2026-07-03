using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class ItemCooldown : ServerPacket
{
	public WowGuid128 ItemGuid;

	public uint SpellID;

	public uint Cooldown;

	public ItemCooldown()
		: base(Opcode.SMSG_ITEM_COOLDOWN)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(ItemGuid);
		_worldPacket.WriteUInt32(SpellID);
		_worldPacket.WriteUInt32(Cooldown);
	}
}
