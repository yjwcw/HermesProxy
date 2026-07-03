using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class SpellHealLog : ServerPacket
{
	public WowGuid128 CasterGUID;

	public WowGuid128 TargetGUID;

	public uint SpellID;

	public int HealAmount;

	public int OriginalHealAmount;

	public uint OverHeal;

	public uint Absorbed;

	public bool Crit;

	public float? CritRollMade;

	public float? CritRollNeeded;

	public SpellCastLogData LogData;

	public ContentTuningParams ContentTuning;

	public SpellHealLog()
		: base(Opcode.SMSG_SPELL_HEAL_LOG, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(TargetGUID);
		_worldPacket.WritePackedGuid128(CasterGUID);
		_worldPacket.WriteUInt32(SpellID);
		_worldPacket.WriteInt32(HealAmount);
		_worldPacket.WriteInt32(OriginalHealAmount);
		_worldPacket.WriteUInt32(OverHeal);
		_worldPacket.WriteUInt32(Absorbed);
		_worldPacket.WriteBit(Crit);
		_worldPacket.WriteBit(CritRollMade.HasValue);
		_worldPacket.WriteBit(CritRollNeeded.HasValue);
		_worldPacket.WriteBit(LogData != null);
		_worldPacket.WriteBit(ContentTuning != null);
		_worldPacket.FlushBits();
		if (LogData != null)
		{
			LogData.Write(_worldPacket);
		}
		if (CritRollMade.HasValue)
		{
			_worldPacket.WriteFloat(CritRollMade.Value);
		}
		if (CritRollNeeded.HasValue)
		{
			_worldPacket.WriteFloat(CritRollNeeded.Value);
		}
		if (ContentTuning != null)
		{
			ContentTuning.Write(_worldPacket);
		}
	}
}
