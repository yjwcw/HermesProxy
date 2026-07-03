namespace HermesProxy.World.Server.Packets;

internal struct FactionStandingData
{
	public int Index;

	public int Standing;

	public void Write(WorldPacket data)
	{
		data.WriteInt32(Index);
		data.WriteInt32(Standing);
	}
}
