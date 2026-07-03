using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class ResetFailedNotify : ServerPacket
{
	public ResetFailedNotify()
		: base(Opcode.SMSG_RESET_FAILED_NOTIFY)
	{
	}

	public override void Write()
	{
	}
}
