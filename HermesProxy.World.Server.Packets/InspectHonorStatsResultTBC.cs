using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class InspectHonorStatsResultTBC : ServerPacket
{
	public WowGuid128 PlayerGUID;

	public byte LifetimeHighestRank;

	public ushort Unused1;

	public ushort YesterdayHonorableKills;

	public ushort Unused3;

	public ushort LifetimeHonorableKills;

	public uint Unused4;

	public uint Unused5;

	public uint Unused6;

	public uint Unused7;

	public uint Unused8;

	public byte Unused9;

	public InspectHonorStatsResultTBC()
		: base(Opcode.SMSG_INSPECT_HONOR_STATS)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(PlayerGUID);
		_worldPacket.WriteUInt8(LifetimeHighestRank);
		_worldPacket.WriteUInt16(Unused1);
		_worldPacket.WriteUInt16(YesterdayHonorableKills);
		_worldPacket.WriteUInt16(Unused3);
		_worldPacket.WriteUInt16(LifetimeHonorableKills);
		_worldPacket.WriteUInt32(Unused4);
		_worldPacket.WriteUInt32(Unused5);
		_worldPacket.WriteUInt32(Unused6);
		_worldPacket.WriteUInt32(Unused7);
		_worldPacket.WriteUInt32(Unused8);
		_worldPacket.WriteUInt8(Unused9);
	}
}
