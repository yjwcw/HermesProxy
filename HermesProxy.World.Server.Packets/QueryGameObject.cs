namespace HermesProxy.World.Server.Packets;

public class QueryGameObject : ClientPacket
{
	public uint GameObjectID;

	public WowGuid128 Guid;

	public QueryGameObject(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		GameObjectID = _worldPacket.ReadUInt32();
		Guid = _worldPacket.ReadPackedGuid128();
	}
}
