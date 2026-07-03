namespace HermesProxy.World.Server.Packets;

public struct GuildRosterProfessionData
{
	public int DbID;

	public int Rank;

	public int Step;

	public void Write(WorldPacket data)
	{
		data.WriteInt32(DbID);
		data.WriteInt32(Rank);
		data.WriteInt32(Step);
	}
}
