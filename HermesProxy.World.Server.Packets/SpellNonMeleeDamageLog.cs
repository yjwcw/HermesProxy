using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class SpellNonMeleeDamageLog : ServerPacket
{
	public WowGuid128 TargetGUID;

	public WowGuid128 CasterGUID;

	public WowGuid128 CastID;

	public uint SpellID;

	public uint SpellXSpellVisualID;

	public int Damage;

	public int OriginalDamage;

	public int Overkill = -1;

	public byte SchoolMask;

	public int ShieldBlock;

	public int Resisted;

	public bool Periodic;

	public int Absorbed;

	public SpellHitType Flags;

	public SpellCastLogData LogData;

	public ContentTuningParams ContentTuning;

	public SpellNonMeleeDamageLog()
		: base(Opcode.SMSG_SPELL_NON_MELEE_DAMAGE_LOG, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(TargetGUID);
		_worldPacket.WritePackedGuid128(CasterGUID);
		_worldPacket.WritePackedGuid128(CastID);
		_worldPacket.WriteUInt32(SpellID);
		_worldPacket.WriteUInt32(SpellXSpellVisualID);
		_worldPacket.WriteInt32(Damage);
		_worldPacket.WriteInt32(OriginalDamage);
		_worldPacket.WriteInt32(Overkill);
		_worldPacket.WriteUInt8(SchoolMask);
		_worldPacket.WriteInt32(Absorbed);
		_worldPacket.WriteInt32(Resisted);
		_worldPacket.WriteInt32(ShieldBlock);
		_worldPacket.WriteBit(Periodic);
		_worldPacket.WriteBits((uint)Flags, 7);
		_worldPacket.WriteBit(bit: false);
		_worldPacket.WriteBit(LogData != null);
		_worldPacket.WriteBit(ContentTuning != null);
		_worldPacket.FlushBits();
		if (LogData != null)
		{
			LogData.Write(_worldPacket);
		}
		if (ContentTuning != null)
		{
			ContentTuning.Write(_worldPacket);
		}
	}
}
