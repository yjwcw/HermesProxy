namespace HermesProxy.World.Server.Packets;

internal class ContentTuningParams
{
	public enum ContentTuningType
	{
		CreatureToPlayerDamage = 1,
		PlayerToCreatureDamage = 2,
		CreatureToCreatureDamage = 4,
		PlayerToSandboxScaling = 7,
		PlayerToPlayerExpectedStat = 8
	}

	public enum ContentTuningFlags
	{
		NoLevelScaling = 1,
		NoItemLevelScaling
	}

	public ContentTuningType TuningType;

	public short PlayerLevelDelta;

	public float PlayerItemLevel;

	public float TargetItemLevel;

	public ushort ScalingHealthItemLevelCurveID;

	public byte TargetLevel;

	public byte Expansion;

	public byte TargetMinScalingLevel;

	public byte TargetMaxScalingLevel;

	public sbyte TargetScalingLevelDelta;

	public ContentTuningFlags Flags = (ContentTuningFlags)3;

	public void Write(WorldPacket data)
	{
		data.WriteFloat(PlayerItemLevel);
		data.WriteFloat(TargetItemLevel);
		data.WriteInt16(PlayerLevelDelta);
		data.WriteUInt16(ScalingHealthItemLevelCurveID);
		data.WriteUInt8(TargetLevel);
		data.WriteUInt8(Expansion);
		data.WriteUInt8(TargetMinScalingLevel);
		data.WriteUInt8(TargetMaxScalingLevel);
		data.WriteInt8(TargetScalingLevelDelta);
		data.WriteUInt32((uint)Flags);
		data.WriteBits(TuningType, 4);
		data.FlushBits();
	}
}
