using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class SummonRequest : ServerPacket
{
	public enum SummonReason
	{
		Spell,
		Scenario
	}

	public WowGuid128 SummonerGUID;

	public uint SummonerVirtualRealmAddress;

	public int AreaID;

	public SummonReason Reason;

	public bool SkipStartingArea;

	public SummonRequest()
		: base(Opcode.SMSG_SUMMON_REQUEST, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(SummonerGUID);
		_worldPacket.WriteUInt32(SummonerVirtualRealmAddress);
		_worldPacket.WriteInt32(AreaID);
		_worldPacket.WriteUInt8((byte)Reason);
		_worldPacket.WriteBit(SkipStartingArea);
		_worldPacket.FlushBits();
	}
}
