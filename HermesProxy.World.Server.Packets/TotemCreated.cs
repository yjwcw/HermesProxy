using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class TotemCreated : ServerPacket
{
	public byte Slot;

	public WowGuid128 Totem;

	public uint Duration;

	public uint SpellId;

	public float TimeMod = 1f;

	public bool CannotDismiss;

	public TotemCreated()
		: base(Opcode.SMSG_TOTEM_CREATED)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt8(Slot);
		_worldPacket.WritePackedGuid128(Totem);
		_worldPacket.WriteUInt32(Duration);
		_worldPacket.WriteUInt32(SpellId);
		_worldPacket.WriteFloat(TimeMod);
		_worldPacket.WriteBit(CannotDismiss);
		_worldPacket.FlushBits();
	}
}
