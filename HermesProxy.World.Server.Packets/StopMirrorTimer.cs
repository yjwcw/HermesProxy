using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class StopMirrorTimer : ServerPacket
{
	public MirrorTimerType Timer;

	public StopMirrorTimer()
		: base(Opcode.SMSG_STOP_MIRROR_TIMER)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteInt32((int)Timer);
	}
}
