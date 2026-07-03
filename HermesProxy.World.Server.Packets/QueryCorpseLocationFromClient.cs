namespace HermesProxy.World.Server.Packets;

public class QueryCorpseLocationFromClient : ClientPacket
{
	public WowGuid128 Player;

	public QueryCorpseLocationFromClient(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		Player = _worldPacket.ReadPackedGuid128();
	}
}
