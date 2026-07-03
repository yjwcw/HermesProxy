using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class InstanceResetFailed : ServerPacket
{
	public uint MapID;

	public ResetFailedReason ResetFailedReason;

	public InstanceResetFailed()
		: base(Opcode.SMSG_INSTANCE_RESET_FAILED)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt32(MapID);
		_worldPacket.WriteBits(ResetFailedReason, 2);
		_worldPacket.FlushBits();
	}
}
