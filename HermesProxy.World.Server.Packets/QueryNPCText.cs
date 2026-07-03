namespace HermesProxy.World.Server.Packets;

public class QueryNPCText : ClientPacket
{
	public WowGuid128 Guid;

	public uint TextID;

	public QueryNPCText(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		TextID = _worldPacket.ReadUInt32();
		Guid = _worldPacket.ReadPackedGuid128();
	}
}
