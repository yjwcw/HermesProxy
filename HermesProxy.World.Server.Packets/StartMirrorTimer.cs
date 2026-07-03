using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class StartMirrorTimer : ServerPacket
{
	public MirrorTimerType Timer;

	public int Value;

	public int MaxValue;

	public int Scale;

	public int SpellID;

	public bool Paused;

	public StartMirrorTimer()
		: base(Opcode.SMSG_START_MIRROR_TIMER)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteInt32((int)Timer);
		_worldPacket.WriteInt32(Value);
		_worldPacket.WriteInt32(MaxValue);
		_worldPacket.WriteInt32(Scale);
		_worldPacket.WriteInt32(SpellID);
		_worldPacket.WriteBit(Paused);
		_worldPacket.FlushBits();
	}
}
