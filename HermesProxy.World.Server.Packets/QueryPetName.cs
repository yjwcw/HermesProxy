namespace HermesProxy.World.Server.Packets;

internal class QueryPetName : ClientPacket
{
	public WowGuid128 UnitGUID;

	public QueryPetName(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		UnitGUID = _worldPacket.ReadPackedGuid128();
	}
}
