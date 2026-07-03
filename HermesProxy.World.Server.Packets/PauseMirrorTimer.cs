using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class PauseMirrorTimer : ServerPacket
{
	public MirrorTimerType Timer;

	public bool Paused;

	public PauseMirrorTimer()
		: base(Opcode.SMSG_PAUSE_MIRROR_TIMER)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteInt32((int)Timer);
		_worldPacket.WriteBit(Paused);
		_worldPacket.FlushBits();
	}
}
