using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class PlayedTime : ServerPacket
{
	public uint TotalTime;

	public uint LevelTime;

	public bool TriggerEvent;

	public PlayedTime()
		: base(Opcode.SMSG_PLAYED_TIME, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt32(TotalTime);
		_worldPacket.WriteUInt32(LevelTime);
		_worldPacket.WriteBit(TriggerEvent);
		_worldPacket.FlushBits();
	}
}
