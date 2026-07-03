using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class SpellChannelStart : ServerPacket
{
	public WowGuid128 CasterGUID;

	public uint SpellID;

	public uint SpellXSpellVisualID;

	public uint Duration;

	public SpellChannelStartInterruptImmunities InterruptImmunities;

	public SpellTargetedHealPrediction HealPrediction;

	public SpellChannelStart()
		: base(Opcode.SMSG_SPELL_CHANNEL_START, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(CasterGUID);
		_worldPacket.WriteUInt32(SpellID);
		_worldPacket.WriteUInt32(SpellXSpellVisualID);
		_worldPacket.WriteUInt32(Duration);
		_worldPacket.WriteBit(InterruptImmunities != null);
		_worldPacket.WriteBit(HealPrediction != null);
		_worldPacket.FlushBits();
		if (InterruptImmunities != null)
		{
			InterruptImmunities.Write(_worldPacket);
		}
		if (HealPrediction != null)
		{
			HealPrediction.Write(_worldPacket);
		}
	}
}
