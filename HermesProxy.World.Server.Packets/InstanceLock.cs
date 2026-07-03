using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class InstanceLock
{
	public uint MapID;

	public DifficultyModern DifficultyID;

	public ulong InstanceID;

	public int TimeRemaining;

	public uint CompletedMask = 1u;

	public bool Locked = true;

	public bool Extended;

	public void Write(WorldPacket data)
	{
		data.WriteUInt32(MapID);
		data.WriteUInt32((uint)DifficultyID);
		data.WriteUInt64(InstanceID);
		data.WriteInt32(TimeRemaining);
		data.WriteUInt32(CompletedMask);
		data.WriteBit(Locked);
		data.WriteBit(Extended);
		data.FlushBits();
	}
}
