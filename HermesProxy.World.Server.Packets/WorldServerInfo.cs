using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class WorldServerInfo : ServerPacket
{
	public uint DifficultyID;

	public byte IsTournamentRealm;

	public bool XRealmPvpAlert;

	public uint? RestrictedAccountMaxLevel;

	public ulong? RestrictedAccountMaxMoney;

	public uint? InstanceGroupSize;

	public WorldServerInfo()
		: base(Opcode.SMSG_WORLD_SERVER_INFO, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt32(DifficultyID);
		_worldPacket.WriteUInt8(IsTournamentRealm);
		_worldPacket.WriteBit(XRealmPvpAlert);
		_worldPacket.WriteBit(RestrictedAccountMaxLevel.HasValue);
		_worldPacket.WriteBit(RestrictedAccountMaxMoney.HasValue);
		_worldPacket.WriteBit(InstanceGroupSize.HasValue);
		_worldPacket.FlushBits();
		if (RestrictedAccountMaxLevel.HasValue)
		{
			_worldPacket.WriteUInt32(RestrictedAccountMaxLevel.Value);
		}
		if (RestrictedAccountMaxMoney.HasValue)
		{
			_worldPacket.WriteUInt64(RestrictedAccountMaxMoney.Value);
		}
		if (InstanceGroupSize.HasValue)
		{
			_worldPacket.WriteUInt32(InstanceGroupSize.Value);
		}
	}
}
