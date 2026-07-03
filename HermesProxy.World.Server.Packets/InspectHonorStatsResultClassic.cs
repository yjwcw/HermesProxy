using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class InspectHonorStatsResultClassic : ServerPacket
{
	public WowGuid128 PlayerGUID;

	public byte LifetimeHighestRank;

	public ushort TodayHonorableKills;

	public ushort TodayDishonorableKills;

	public ushort YesterdayHonorableKills;

	public ushort YesterdayDishonorableKills;

	public ushort LastWeekHonorableKills;

	public ushort LastWeekDishonorableKills;

	public ushort ThisWeekHonorableKills;

	public ushort ThisWeekDishonorableKills;

	public uint LifetimeHonorableKills;

	public uint LifetimeDishonorableKills;

	public uint YesterdayHonor;

	public uint LastWeekHonor;

	public uint ThisWeekHonor;

	public uint Standing;

	public byte RankProgress;

	public InspectHonorStatsResultClassic()
		: base(Opcode.SMSG_INSPECT_HONOR_STATS)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(PlayerGUID);
		_worldPacket.WriteUInt8(LifetimeHighestRank);
		_worldPacket.WriteUInt16(TodayHonorableKills);
		_worldPacket.WriteUInt16(TodayDishonorableKills);
		_worldPacket.WriteUInt16(YesterdayHonorableKills);
		_worldPacket.WriteUInt16(YesterdayDishonorableKills);
		_worldPacket.WriteUInt16(LastWeekHonorableKills);
		_worldPacket.WriteUInt16(LastWeekDishonorableKills);
		_worldPacket.WriteUInt16(ThisWeekHonorableKills);
		_worldPacket.WriteUInt16(ThisWeekDishonorableKills);
		_worldPacket.WriteUInt32(LifetimeHonorableKills);
		_worldPacket.WriteUInt32(LifetimeDishonorableKills);
		_worldPacket.WriteUInt32(YesterdayHonor);
		_worldPacket.WriteUInt32(LastWeekHonor);
		_worldPacket.WriteUInt32(ThisWeekHonor);
		_worldPacket.WriteUInt32(Standing);
		_worldPacket.WriteUInt8(RankProgress);
	}
}
