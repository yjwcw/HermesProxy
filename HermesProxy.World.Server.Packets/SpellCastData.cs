using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets;

public class SpellCastData
{
	public WowGuid128 CasterGUID;

	public WowGuid128 CasterUnit;

	public WowGuid128 CastID = WowGuid128.Empty;

	public WowGuid128 OriginalCastID = WowGuid128.Empty;

	public int SpellID;

	public uint SpellXSpellVisualID;

	public uint CastFlags;

	public uint CastFlagsEx;

	public uint CastTime;

	public List<WowGuid128> HitTargets = new List<WowGuid128>();

	public List<WowGuid128> MissTargets = new List<WowGuid128>();

	public List<SpellMissStatus> MissStatus = new List<SpellMissStatus>();

	public SpellTargetData Target = new SpellTargetData();

	public List<SpellPowerData> RemainingPower = new List<SpellPowerData>();

	public RuneData RemainingRunes;

	public MissileTrajectoryResult MissileTrajectory;

	public int? AmmoDisplayId;

	public int? AmmoInventoryType;

	public byte DestLocSpellCastIndex;

	public List<TargetLocation> TargetPoints = new List<TargetLocation>();

	public CreatureImmunities Immunities;

	public SpellHealPrediction Predict = new SpellHealPrediction();

	public void Write(WorldPacket data)
	{
		data.WritePackedGuid128(CasterGUID);
		data.WritePackedGuid128(CasterUnit);
		data.WritePackedGuid128(CastID);
		data.WritePackedGuid128(OriginalCastID);
		data.WriteInt32(SpellID);
		data.WriteUInt32(SpellXSpellVisualID);
		data.WriteUInt32(CastFlags);
		data.WriteUInt32(CastFlagsEx);
		data.WriteUInt32(CastTime);
		MissileTrajectory.Write(data);
		data.WriteUInt8(DestLocSpellCastIndex);
		Immunities.Write(data);
		Predict.Write(data);
		data.WriteBits(HitTargets.Count, 16);
		data.WriteBits(MissTargets.Count, 16);
		data.WriteBits(MissStatus.Count, 16);
		data.WriteBits(RemainingPower.Count, 9);
		data.WriteBit(RemainingRunes != null);
		data.WriteBits(TargetPoints.Count, 16);
		data.WriteBit(AmmoDisplayId.HasValue);
		data.WriteBit(AmmoInventoryType.HasValue);
		data.FlushBits();
		foreach (SpellMissStatus item in MissStatus)
		{
			item.Write(data);
		}
		Target.Write(data);
		foreach (WowGuid128 hitTarget in HitTargets)
		{
			data.WritePackedGuid128(hitTarget);
		}
		foreach (WowGuid128 missTarget in MissTargets)
		{
			data.WritePackedGuid128(missTarget);
		}
		foreach (SpellPowerData item2 in RemainingPower)
		{
			item2.Write(data);
		}
		if (RemainingRunes != null)
		{
			RemainingRunes.Write(data);
		}
		foreach (TargetLocation targetPoint in TargetPoints)
		{
			targetPoint.Write(data);
		}
		if (AmmoDisplayId.HasValue)
		{
			data.WriteInt32(AmmoDisplayId.Value);
		}
		if (AmmoInventoryType.HasValue)
		{
			data.WriteInt32(AmmoInventoryType.Value);
		}
	}
}
