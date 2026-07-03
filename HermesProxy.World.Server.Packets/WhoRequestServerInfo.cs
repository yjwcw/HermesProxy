namespace HermesProxy.World.Server.Packets;

public class WhoRequestServerInfo
{
	public int FactionGroup;

	public int Locale;

	public uint RequesterVirtualRealmAddress;

	public void Read(WorldPacket data)
	{
		FactionGroup = data.ReadInt32();
		Locale = data.ReadInt32();
		RequesterVirtualRealmAddress = data.ReadUInt32();
	}
}
