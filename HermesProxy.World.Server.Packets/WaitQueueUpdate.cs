using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class WaitQueueUpdate : ServerPacket
{
	public AuthWaitInfo WaitInfo = new AuthWaitInfo();

	public WaitQueueUpdate()
		: base(Opcode.SMSG_WAIT_QUEUE_UPDATE)
	{
	}

	public override void Write()
	{
		WaitInfo.Write(_worldPacket);
	}
}
