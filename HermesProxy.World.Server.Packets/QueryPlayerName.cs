namespace HermesProxy.World.Server.Packets;

public class QueryPlayerName : ClientPacket
{
	public WowGuid128 Player;

	public QueryPlayerName(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		Player = _worldPacket.ReadPackedGuid128();
	}
}
