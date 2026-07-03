namespace HermesProxy.World.Server.Packets;

public struct LFGListBlacklistEntry
{
	public int ActivityID;

	public int Reason;

	public void Write(WorldPacket data)
	{
		data.WriteInt32(ActivityID);
		data.WriteInt32(Reason);
	}
}
