using System.Collections.Generic;
using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class SpellPeriodicAuraLog : ServerPacket
{
	public class PeriodicalAuraLogEffectDebugInfo
	{
		public float CritRollMade { get; set; }

		public float CritRollNeeded { get; set; }
	}

	public class SpellLogEffect
	{
		public uint Effect;

		public int Amount;

		public int OriginalDamage;

		public uint OverHealOrKill;

		public uint SchoolMaskOrPower;

		public uint AbsorbedOrAmplitude;

		public uint Resisted;

		public bool Crit;

		public PeriodicalAuraLogEffectDebugInfo DebugInfo;

		public ContentTuningParams ContentTuning;

		public void Write(WorldPacket data)
		{
			data.WriteUInt32(Effect);
			data.WriteInt32(Amount);
			data.WriteInt32(OriginalDamage);
			data.WriteUInt32(OverHealOrKill);
			data.WriteUInt32(SchoolMaskOrPower);
			data.WriteUInt32(AbsorbedOrAmplitude);
			data.WriteUInt32(Resisted);
			data.WriteBit(Crit);
			data.WriteBit(DebugInfo != null);
			data.WriteBit(ContentTuning != null);
			data.FlushBits();
			if (ContentTuning != null)
			{
				ContentTuning.Write(data);
			}
			if (DebugInfo != null)
			{
				data.WriteFloat(DebugInfo.CritRollMade);
				data.WriteFloat(DebugInfo.CritRollNeeded);
			}
		}
	}

	public WowGuid128 TargetGUID;

	public WowGuid128 CasterGUID;

	public uint SpellID;

	public SpellCastLogData LogData;

	public List<SpellLogEffect> Effects = new List<SpellLogEffect>();

	public SpellPeriodicAuraLog()
		: base(Opcode.SMSG_SPELL_PERIODIC_AURA_LOG, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(TargetGUID);
		_worldPacket.WritePackedGuid128(CasterGUID);
		_worldPacket.WriteUInt32(SpellID);
		_worldPacket.WriteInt32(Effects.Count);
		_worldPacket.WriteBit(LogData != null);
		_worldPacket.FlushBits();
		foreach (SpellLogEffect effect in Effects)
		{
			effect.Write(_worldPacket);
		}
		if (LogData != null)
		{
			LogData.Write(_worldPacket);
		}
	}
}
