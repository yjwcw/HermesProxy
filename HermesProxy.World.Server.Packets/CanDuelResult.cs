using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class CanDuelResult : ServerPacket
{
	public WowGuid128 TargetGUID;

	public bool Result;

	public CanDuelResult()
		: base(Opcode.SMSG_CAN_DUEL_RESULT)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(TargetGUID);
		_worldPacket.WriteBit(Result);
		_worldPacket.FlushBits();
	}
}
