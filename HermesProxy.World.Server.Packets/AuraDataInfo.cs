using System.Collections.Generic;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class AuraDataInfo
{
	public WowGuid128 CastID;

	public uint SpellID;

	public uint SpellXSpellVisualID;

	public AuraFlagsModern Flags;

	public uint ActiveFlags;

	public ushort CastLevel = 1;

	public byte Applications = 1;

	public int ContentTuningID;

	private ContentTuningParams ContentTuning;

	public WowGuid128 CastUnit;

	public int? Duration;

	public int? Remaining;

	private float? TimeMod;

	public List<float> Points = new List<float>();

	public List<float> EstimatedPoints = new List<float>();

	public void Write(WorldPacket data)
	{
		data.WritePackedGuid128(CastID);
		data.WriteUInt32(SpellID);
		data.WriteUInt32(SpellXSpellVisualID);
		data.WriteUInt16((ushort)Flags);
		data.WriteUInt32(ActiveFlags);
		data.WriteUInt16(CastLevel);
		data.WriteUInt8(Applications);
		data.WriteInt32(ContentTuningID);
		data.WriteBit(CastUnit != null);
		data.WriteBit(Duration.HasValue);
		data.WriteBit(Remaining.HasValue);
		data.WriteBit(TimeMod.HasValue);
		data.WriteBits(Points.Count, 6);
		data.WriteBits(EstimatedPoints.Count, 6);
		data.WriteBit(ContentTuning != null);
		if (ContentTuning != null)
		{
			ContentTuning.Write(data);
		}
		if (CastUnit != null)
		{
			data.WritePackedGuid128(CastUnit);
		}
		if (Duration.HasValue)
		{
			data.WriteInt32(Duration.Value);
		}
		if (Remaining.HasValue)
		{
			data.WriteInt32(Remaining.Value);
		}
		if (TimeMod.HasValue)
		{
			data.WriteFloat(TimeMod.Value);
		}
		foreach (float point in Points)
		{
			data.WriteFloat(point);
		}
		foreach (float estimatedPoint in EstimatedPoints)
		{
			data.WriteFloat(estimatedPoint);
		}
	}
}
