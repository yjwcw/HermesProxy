using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class TransferAborted : ServerPacket
{
	public uint MapID;

	public byte Arg;

	public int MapDifficultyXConditionID = -6;

	public TransferAbortReasonModern Reason;

	public TransferAborted()
		: base(Opcode.SMSG_TRANSFER_ABORTED)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt32(MapID);
		_worldPacket.WriteUInt8(Arg);
		_worldPacket.WriteInt32(MapDifficultyXConditionID);
		_worldPacket.WriteBits(Reason, 6);
		_worldPacket.FlushBits();
	}
}
