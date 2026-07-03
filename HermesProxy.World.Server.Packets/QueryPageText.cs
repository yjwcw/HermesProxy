namespace HermesProxy.World.Server.Packets;

public class QueryPageText : ClientPacket
{
	public WowGuid128 ItemGUID;

	public uint PageTextID;

	public QueryPageText(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		PageTextID = _worldPacket.ReadUInt32();
		ItemGUID = _worldPacket.ReadPackedGuid128();
	}
}
