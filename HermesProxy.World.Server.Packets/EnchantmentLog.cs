using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class EnchantmentLog : ServerPacket
{
	public WowGuid128 Owner;

	public WowGuid128 Caster;

	public WowGuid128 ItemGUID;

	public int ItemID;

	public int Enchantment;

	public int EnchantSlot = 1;

	public EnchantmentLog()
		: base(Opcode.SMSG_ENCHANTMENT_LOG)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(Owner);
		_worldPacket.WritePackedGuid128(Caster);
		_worldPacket.WritePackedGuid128(ItemGUID);
		_worldPacket.WriteInt32(ItemID);
		_worldPacket.WriteInt32(Enchantment);
		_worldPacket.WriteInt32(EnchantSlot);
	}
}
