namespace HermesProxy.World.Server.Packets;

public struct SavedThrottleObjectState
{
	public uint MaxTries;

	public uint PerMilliseconds;

	public uint TryCount;

	public uint LastResetTimeBeforeNow;

	public void Write(WorldPacket data)
	{
		data.WriteUInt32(MaxTries);
		data.WriteUInt32(PerMilliseconds);
		data.WriteUInt32(TryCount);
		data.WriteUInt32(LastResetTimeBeforeNow);
	}
}
